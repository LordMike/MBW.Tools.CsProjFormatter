using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using MBW.Tools.CsProjFormatter.Configuration.EditorConfig;
using MBW.Tools.CsProjFormatter.Library;
using MBW.Tools.CsProjFormatter.Library.Configuration;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace MBW.Tools.CsProjFormatter
{
    public class SettingsModel
    {
        [Option("-l|--log-level", Description = "Logging level")]
        public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

        [Option("-n", Description = "Enable to make no changes")]
        public bool DryRun { get; set; }

        [Option("-d", Description = "Do not exclude 'obj/*.targets' and 'bin/*.targets' by default.")]
        public bool NoDefaultExcludes { get; set; }

        [Option("--exclude", Description = "Exclude this globbing pattern. Can be set multiple times")]
        public string[] Excludes { get; set; }

        [Option("--include", Description = "Include this globbing pattern, defaults to '**/*.csproj' and '**/*.targets'")]
        public string[] Includes { get; set; } = { "**/*.csproj", "**/*.targets" };

        [Required]
        [Argument(0, "Directories")]
        public string[] Directories { get; set; }
    }

    enum ExitCode
    {
        Ok = 0,
        Error = 1
    }

    class FormatterProgram
    {
        private readonly SettingsModel _settings;
        private readonly Formatter _formatter;
        private readonly ILogger<FormatterProgram> _logger;

        public FormatterProgram(SettingsModel settings, Formatter formatter, ILogger<FormatterProgram> logger)
        {
            _settings = settings;
            _formatter = formatter;
            _logger = logger;
        }

        public ExitCode Run()
        {
            _logger.LogDebug("Identifying file to process in {Count} directories. {CountInclude} includes, {CountExclude} excludes were provided", _settings.Directories.Length, _settings.Includes?.Length ?? 0, _settings.Excludes?.Length ?? 0);

            Matcher globber = new Matcher(StringComparison.OrdinalIgnoreCase);

            if (_settings.Includes != null)
                globber.AddIncludePatterns(_settings.Includes);

            if (_settings.Excludes != null)
                globber.AddExcludePatterns(_settings.Excludes);

            if (!_settings.NoDefaultExcludes)
            {
                globber.AddExclude("**/obj/**.targets");
                globber.AddExclude("**/bin/**.targets");
            }

            foreach (string directory in _settings.Directories)
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    {"Directory", directory}
                }))
                {
                    _logger.LogDebug("Searching for files in {Directory}", directory);

                    if (Directory.Exists(directory))
                    {
                        IEnumerable<string> files = globber.GetResultsInFullPath(directory);

                        foreach (string file in files)
                        {
                            HandleFile(_formatter, file);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("{Directory} was not found", directory);
                    }
                }
            }

            return ExitCode.Ok;
        }

        private void HandleFile(Formatter formatter, string file)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                {"File", file}
            }))
            {
                if (_settings.DryRun)
                {
                    _logger.LogInformation("Processing file {File} (DRY-RUN)", file);
                    return;
                }

                _logger.LogInformation("Processing file {File}", file);

                formatter.FormatFile(file);
            }
        }
    }

    internal static class Extensions
    {
        public static ILogger<T> GetLogger<T>(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ILogger<T>>();
        }

        public static string[] ExpandPath(string path)
        {
            var lastIdx = path.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });

            if (lastIdx < 0)
                return new[] { path };

            var dir = path.Substring(0, lastIdx);
            var name = path.Substring(lastIdx + 1);

            var dirs = Directory.GetDirectories(dir, name);
            if (dirs.Any())
                return dirs;

            return new[] { path };
        }
    }

    class Program
    {
        static int Main(string[] args)
        {
            CommandLineApplication<SettingsModel> app = new CommandLineApplication<SettingsModel>();

            app.Conventions
                .UseDefaultConventions();

            app.OnExecute(() =>
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Is(app.Model.LogLevel)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();

                // Post-process settings
                app.Model.Directories = app.Model.Directories.SelectMany(Extensions.ExpandPath).ToArray();

                // Setup host
                IServiceCollection services = new ServiceCollection();

                services.AddSingleton(app.Model);
                services.AddSingleton<FormatterProgram>();

                services
                    .AddSingleton<IFormatterSettingsFactory>(x =>
                    {
                        ILogger<Program> logger = x.GetLogger<Program>();
                        logger.LogDebug("Prepared formatter with EditorConfig support");

                        FormatterSettingsFactory formatterSettingsFactory = ActivatorUtilities.CreateInstance<FormatterSettingsFactory>(x);
                        formatterSettingsFactory.AddEditorConfig();

                        return formatterSettingsFactory;
                    })
                    .AddSingleton<Formatter>();

                services.AddLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddSerilog(Log.Logger);
                });

                ExitCode result;
                using (ServiceProvider provider = services.BuildServiceProvider())
                {
                    ILogger<Program> logger = provider.GetLogger<Program>();
                    FormatterProgram program = provider.GetRequiredService<FormatterProgram>();

                    try
                    {
                        result = program.Run();
                    }
                    catch (Exception e)
                    {
                        logger.LogCritical(e, "An error occurred while running the program");
                        result = ExitCode.Error;
                    }
                }

                return (int)result;
            });

            app.OnValidationError(result =>
            {
                app.ShowHelp();
            });

            return app.Execute(args);
        }
    }
}
