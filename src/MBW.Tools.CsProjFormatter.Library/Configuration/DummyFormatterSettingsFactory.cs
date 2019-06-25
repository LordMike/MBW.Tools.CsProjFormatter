namespace MBW.Tools.CsProjFormatter.Library.Configuration
{
    internal class DummyFormatterSettingsFactory : IFormatterSettingsFactory
    {
        private readonly FormatterSettings _settings;

        public DummyFormatterSettingsFactory(FormatterSettings settings)
        {
            _settings = settings;
        }

        public FormatterSettings GetSettings(string file)
        {
            return _settings;
        }
    }
}