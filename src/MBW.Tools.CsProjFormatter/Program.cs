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
using Microsoft.Extensions.Logging;

namespace MBW.Tools.CsProjFormatter
{
    public class SettingsModel
    {
        [Option("-l|--log-level", Description = "Logging level")]
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        [Option("-e", Description = "Extensions to format, comma separated. Default: 'csproj,targets'")]
        public string ExtensionsToFormat { get; set; } = "csproj,targets";

        [Option("-n", Description = "Enable to make no changes")]
        public bool DryRun { get; set; }

        [Required]
        [Argument(0, "Files or directories")]
        public string[] Arguments { get; set; }
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
            _logger.LogDebug("Beginning formatting run, {Count} arguments were provided", _settings.Arguments.Length);

            string[] extensionsToFind = _settings.ExtensionsToFormat.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                {
                    if (s.StartsWith("."))
                        return "*" + s;
                    if (s.StartsWith("*."))
                        return s;
                    return "*." + s;
                })
                .ToArray();

            foreach (string argument in _settings.Arguments)
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    {"Argument", argument}
                }))
                {
                    _logger.LogDebug("Beginning formatting run on {Argument}", argument);

                    if (File.Exists(argument))
                    {
                        _logger.LogTrace("{Argument} is a file", argument);

                        HandleFile(_formatter, argument);
                    }
                    else if (Directory.Exists(argument))
                    {
                        _logger.LogTrace("{Argument} is a directory", argument);

                        IEnumerable<string> files = extensionsToFind.SelectMany(s => Directory.EnumerateFiles(argument, s, SearchOption.AllDirectories));

                        foreach (string file in files)
                        {
                            HandleFile(_formatter, file);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("{Argument} was not found", argument);
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
                IServiceCollection services = new ServiceCollection();

                services.AddSingleton(app.Model);
                services.AddSingleton<FormatterProgram>();

                services
                    .AddSingleton(x =>
                    {
                        ILogger<Program> logger = x.GetLogger<Program>();
                        logger.LogDebug("Prepared formatter with EditorConfig support");

                        return new FormatterSettingsFactory().AddEditorConfig();
                    })
                    .AddSingleton<Formatter>();

                services.AddLogging(builder =>
                {
                    builder.SetMinimumLevel(app.Model.LogLevel);
                    builder.AddConsole();
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
