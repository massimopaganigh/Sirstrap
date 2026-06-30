namespace Sirstrap.UI.Models
{
    public partial class Settings : ModelBase
    {
        private readonly SirstrapConfiguration _configuration;
        private readonly ISettingsService _settingsService;
        private readonly IPathManager _pathManager;

        [ObservableProperty]
        private bool _sirstrapAutoUpdate = true;

        [ObservableProperty]
        private string _sirstrapChannel = string.Empty;

        [ObservableProperty]
        private string _sirstrapFontFamily = "JetBrains Mono";

        [ObservableProperty]
        private bool _sirstrapTelemetry = true;

        [ObservableProperty]
        private TrayMode _sirstrapTrayMode = TrayMode.None;

        [ObservableProperty]
        private bool _robloxIncognito = false;

        [ObservableProperty]
        private string _robloxInstallationPath = string.Empty;

        [ObservableProperty]
        private bool _robloxMultiInstance = true;

        [ObservableProperty]
        private string _robloxVersionSource = RobloxVersionSources.SirHurt;

        [ObservableProperty]
        private string _robloxCdnUriOverride = string.Empty;

        [ObservableProperty]
        private bool _runSirHurtEnabled = false;

        [ObservableProperty]
        private string _sirHurtPath = string.Empty;

        public Settings(SirstrapConfiguration configuration, ISettingsService settingsService, IPathManager pathManager, ISirHurtService sirHurtService)
        {
            _configuration = configuration;
            _settingsService = settingsService;
            _pathManager = pathManager;

            _settingsService.LoadSettings();

            var sirHurtPath = sirHurtService.GetSirHurtPath();

            SirstrapAutoUpdate = configuration.SirstrapAutoUpdate;
            SirstrapChannel = configuration.SirstrapChannel;
            SirstrapFontFamily = configuration.SirstrapFontFamily;
            SirstrapTelemetry = configuration.SirstrapTelemetry;
            SirstrapTrayMode = configuration.SirstrapTrayMode;
            RobloxIncognito = configuration.RobloxIncognito;
            RobloxInstallationPath = configuration.RobloxInstallationPath;
            RobloxMultiInstance = configuration.RobloxMultiInstance;
            RobloxVersionSource = configuration.RobloxVersionSource;
            RobloxCdnUriOverride = configuration.RobloxCdnUriOverride;
            RunSirHurtEnabled = File.Exists(Path.Combine(sirHurtPath, "bootstrapper.exe"));
            SirHurtPath = sirHurtPath;
        }

        public void Set()
        {
            if (!string.Equals(_configuration.RobloxInstallationPath, RobloxInstallationPath, StringComparison.OrdinalIgnoreCase))
                _configuration.RobloxPreviousInstallationPath = _configuration.RobloxInstallationPath;

            _configuration.SirstrapAutoUpdate = SirstrapAutoUpdate;
            _configuration.SirstrapChannel = SirstrapChannel;
            _configuration.SirstrapFontFamily = SirstrapFontFamily;
            _configuration.SirstrapTelemetry = SirstrapTelemetry;
            _configuration.SirstrapTrayMode = SirstrapTrayMode;
            _configuration.RobloxIncognito = RobloxIncognito;
            _configuration.RobloxInstallationPath = RobloxInstallationPath;
            _configuration.RobloxMultiInstance = RobloxMultiInstance;
            _configuration.RobloxVersionSource = RobloxVersionSource;
            _configuration.RobloxCdnUriOverride = RobloxCdnService.NormalizeCdnUriOverride(RobloxCdnUriOverride);
            RobloxCdnUriOverride = _configuration.RobloxCdnUriOverride;

            _settingsService.SaveSettings();
            _settingsService.LoadSettings();

            _pathManager.PurgePreviousInstallationPath();
        }

        partial void OnRobloxMultiInstanceChanged(bool value)
        {
            if (!value
                && RobloxIncognito)
                RobloxIncognito = false;
        }
    }
}
