namespace Sirstrap.Core.Tests.Support
{
    public sealed class FakeSettingsService : ISettingsService
    {
        public int LoadCalls { get; private set; }

        public int SaveCalls { get; private set; }

        public int MetricCalls { get; private set; }

        public void EmitSettingsMetrics() => MetricCalls++;

        public void LoadSettings(string? settingsFilePath = null) => LoadCalls++;

        public void SaveSettings(string? settingsFilePath = null) => SaveCalls++;
    }
}
