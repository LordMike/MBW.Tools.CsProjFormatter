using System;
using System.Diagnostics;
using System.Linq;
using MBW.Tools.CsProjFormatter.Configuration.EditorConfig;
using MBW.Tools.CsProjFormatter.Library;
using MBW.Tools.CsProjFormatter.Library.Configuration;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MBW.Tools.CsProjFormatter
{
    internal class Program
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

                    Stopwatch sw = new Stopwatch();
                    try
                    {
                        sw.Start();
                        result = program.Run();
                        sw.Stop();

                        logger.LogInformation("Completed processing of {Count} files in {Time}", program.ProcessedFiles, sw.Elapsed);
                    }
                    catch (Exception e)
                    {
                        sw.Stop();

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
