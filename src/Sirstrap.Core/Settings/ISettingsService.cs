namespace Sirstrap.Core.Settings
{
    public interface ISettingsService
    {
        void EmitSettingsMetrics();

        void LoadSettings(string? settingsFilePath = null);

        void SaveSettings(string? settingsFilePath = null);
    }
}
