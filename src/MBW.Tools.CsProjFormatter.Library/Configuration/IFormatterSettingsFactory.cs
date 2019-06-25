namespace MBW.Tools.CsProjFormatter.Library.Configuration
{
    public interface IFormatterSettingsFactory
    {
        FormatterSettings GetSettings(string file);
    }
}