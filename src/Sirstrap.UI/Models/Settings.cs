namespace Sirstrap.UI.Models
{
    public partial class Settings : ModelBase
    {
        private readonly SirstrapConfiguration _configuration;
        private readonly ISettingsService _settingsService;
        private readonly IPathManager _pathManager;

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
        private string _robloxCdnUriOverride = string.Empty;

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

        public Settings(SirstrapConfiguration configuration, ISettingsService settingsService, IPathManager pathManager, ISirHurtService sirHurtService)
        {
            _configuration = configuration;
            _settingsService = settingsService;
            _pathManager = pathManager;

            _settingsService.LoadSettings();

            var sirHurtPath = sirHurtService.GetSirHurtPath();

            AutoUpdate = configuration.AutoUpdate;
            ChannelName = configuration.ChannelName;
            FontFamily = configuration.FontFamily;
            Incognito = configuration.Incognito;
            InstallationPath = configuration.InstallationPath;
            MultiInstance = configuration.MultiInstance;
            RobloxApi = configuration.RobloxApi;
            RobloxCdnUriOverride = configuration.RobloxCdnUriOverride;
            RobloxVersionOverride = configuration.RobloxVersionOverride;
            RunSirHurtEnabled = File.Exists(Path.Combine(sirHurtPath, "bootstrapper.exe"));
            SirHurtPath = sirHurtPath;
            Telemetry = configuration.Telemetry;
            TrayMode = configuration.TrayMode;
        }

        public void Set()
        {
            if (!string.Equals(_configuration.InstallationPath, InstallationPath, StringComparison.OrdinalIgnoreCase))
                _configuration.PreviousInstallationPath = _configuration.InstallationPath;

            _configuration.AutoUpdate = AutoUpdate;
            _configuration.ChannelName = ChannelName;
            _configuration.FontFamily = FontFamily;
            _configuration.Incognito = Incognito;
            _configuration.InstallationPath = InstallationPath;
            _configuration.MultiInstance = MultiInstance;
            _configuration.RobloxApi = RobloxApi;
            _configuration.RobloxCdnUriOverride = RobloxCdnService.NormalizeCdnUriOverride(RobloxCdnUriOverride);
            RobloxCdnUriOverride = _configuration.RobloxCdnUriOverride;
            _configuration.RobloxVersionOverride = RobloxVersionOverride;
            _configuration.Telemetry = Telemetry;
            _configuration.TrayMode = TrayMode;

            _settingsService.SaveSettings();
            _settingsService.LoadSettings();

            _pathManager.PurgePreviousInstallationPath();
        }

        partial void OnMultiInstanceChanged(bool value)
        {
            if (!value
                && Incognito)
                Incognito = false;
        }
    }
}
