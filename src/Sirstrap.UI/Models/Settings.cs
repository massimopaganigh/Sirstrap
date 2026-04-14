namespace Sirstrap.UI.Models
{
    public partial class Settings : ModelBase
    {
        [ObservableProperty]
        private bool _autoUpdate = true;

        [ObservableProperty]
        private string _channelName = string.Empty;

        [ObservableProperty]
        private string _fontFamily = "JetBrains Mono";

        [ObservableProperty]
        private bool _incognito = false;

        [ObservableProperty]
        private string _installationPath = string.Empty;

        [ObservableProperty]
        private bool _multiInstance = true;

        [ObservableProperty]
        private bool _robloxApi = false;

        [ObservableProperty]
        [SuppressMessage("SonarAnalyzer.CSharp", "S1075", Justification = "Sybau")]
        private string _robloxCdnUri = "https://setup.rbxcdn.com";

        [ObservableProperty]
        private string _robloxVersionOverride = string.Empty;

        [ObservableProperty]
        private bool _runSirHurtEnabled = false;

        [ObservableProperty]
        private string _sirHurtPath = string.Empty;

        [ObservableProperty]
        private bool _telemetry = true;

        [ObservableProperty]
        private TrayMode _trayMode = TrayMode.None;

        public Settings()
        {
            SirstrapConfigurationService.LoadSettings();

            AutoUpdate = SirstrapConfiguration.AutoUpdate;
            ChannelName = SirstrapConfiguration.ChannelName;
            FontFamily = SirstrapConfiguration.FontFamily;
            Incognito = SirstrapConfiguration.Incognito;
            InstallationPath = SirstrapConfiguration.InstallationPath;
            MultiInstance = SirstrapConfiguration.MultiInstance;
            RobloxApi = SirstrapConfiguration.RobloxApi;
            RobloxCdnUri = SirstrapConfiguration.RobloxCdnUri;
            RobloxVersionOverride = SirstrapConfiguration.RobloxVersionOverride;
            RunSirHurtEnabled = File.Exists(Path.Combine(SirstrapConfiguration.SirHurtPath, "bootstrapper.exe"));
            SirHurtPath = SirstrapConfiguration.SirHurtPath;
            Telemetry = SirstrapConfiguration.Telemetry;
            TrayMode = SirstrapConfiguration.TrayMode;
        }

        partial void OnMultiInstanceChanged(bool value)
        {
            if (!value
                && Incognito)
                Incognito = false;
        }

        public void Set()
        {
            if (!string.Equals(SirstrapConfiguration.InstallationPath, InstallationPath, StringComparison.OrdinalIgnoreCase))
                SirstrapConfiguration.PreviousInstallationPath = SirstrapConfiguration.InstallationPath;

            SirstrapConfiguration.AutoUpdate = AutoUpdate;
            SirstrapConfiguration.ChannelName = ChannelName;
            SirstrapConfiguration.FontFamily = FontFamily;
            SirstrapConfiguration.Incognito = Incognito;
            SirstrapConfiguration.InstallationPath = InstallationPath;
            SirstrapConfiguration.MultiInstance = MultiInstance;
            SirstrapConfiguration.RobloxApi = RobloxApi;
            SirstrapConfiguration.RobloxCdnUri = RobloxCdnUri;
            SirstrapConfiguration.RobloxVersionOverride = RobloxVersionOverride;
            SirstrapConfiguration.Telemetry = Telemetry;
            SirstrapConfiguration.TrayMode = TrayMode;

            SirstrapConfigurationService.SaveSettings();
            SirstrapConfigurationService.LoadSettings();

            PathManager.PurgePreviousInstallationPath();
        }
    }
}
