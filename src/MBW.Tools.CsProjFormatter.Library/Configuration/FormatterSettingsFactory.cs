using System.Collections.Generic;

namespace MBW.Tools.CsProjFormatter.Library.Configuration
{
    public class FormatterSettingsFactory : IFormatterSettingsFactory
    {
        public delegate void SettingsProvider(string file, FormatterSettings settings);

        private List<SettingsProvider> _providers;

        public FormatterSettingsFactory()
        {
            _providers = new List<SettingsProvider>();
        }

        public FormatterSettingsFactory AddProvider(SettingsProvider settingsProvider)
        {
            _providers.Add(settingsProvider);
            return this;
        }

        FormatterSettings IFormatterSettingsFactory.GetSettings(string file)
        {
            FormatterSettings settings = new FormatterSettings();

            foreach (SettingsProvider provider in _providers)
            {
                provider(file, settings);
            }

            return settings;
        }
    }
}