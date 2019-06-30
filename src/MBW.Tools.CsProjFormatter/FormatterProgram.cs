using System;
using System.Collections.Generic;
using System.IO;
using MBW.Tools.CsProjFormatter.Library;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;

namespace MBW.Tools.CsProjFormatter
{
    internal class FormatterProgram
    {
        private readonly SettingsModel _settings;
        private readonly Formatter _formatter;
        private readonly ILogger<FormatterProgram> _logger;

        public int ProcessedFiles { get; private set; }

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

                            ProcessedFiles++;
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

                _logger.LogDebug("Processing file {File}", file);

                formatter.FormatFile(file);
            }
        }
    }
}