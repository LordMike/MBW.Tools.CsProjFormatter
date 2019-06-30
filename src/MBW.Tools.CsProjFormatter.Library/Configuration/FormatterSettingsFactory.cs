using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MBW.Tools.CsProjFormatter.Library.Configuration
{
    public class FormatterSettingsFactory : IFormatterSettingsFactory
    {
        private readonly ILogger<FormatterSettingsFactory> _logger;

        public delegate void SettingsProvider(string file, FormatterSettings settings);

        private List<SettingsProvider> _providers;

        public FormatterSettingsFactory(ILogger<FormatterSettingsFactory> logger = null)
        {
            _logger = logger ?? new NullLogger<FormatterSettingsFactory>();
            _providers = new List<SettingsProvider>();
        }

        public FormatterSettingsFactory AddProvider(SettingsProvider settingsProvider)
        {
            _providers.Add(settingsProvider);
            return this;
        }

        FormatterSettings IFormatterSettingsFactory.GetSettings(string file)
        {
            _logger.LogDebug("Producing formatter settings for file {File}", file);

            FormatterSettings settings = new FormatterSettings();

            foreach (SettingsProvider provider in _providers)
            {
                provider(file, settings);
            }

            return settings;
        }
    }
}