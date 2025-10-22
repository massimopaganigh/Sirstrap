namespace Sirstrap.Core.Interfaces
{
    public interface IConfigurationService
    {
        public Configuration GetConfiguration(string[] rawArguments);
    }
}
