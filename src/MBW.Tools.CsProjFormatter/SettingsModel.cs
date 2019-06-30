using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using Serilog.Events;

namespace MBW.Tools.CsProjFormatter
{
    internal class SettingsModel
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
}