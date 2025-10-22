namespace Sirstrap.Core.Interfaces
{
    public interface ISirstrapConfigurationService
    {
        public void GetSettings(string? settingsPath = null);
        public void SetSettings(string? settingsPath = null);
    }
}
