namespace Sirstrap.UI.Models
{
    public partial class Settings : ModelBase
    {
        private readonly SirstrapConfiguration _configuration;
        private readonly IFastFlagService _fastFlagService;
        private readonly ISettingsService _settingsService;
        private readonly IPathManager _pathManager;

        [ObservableProperty]
        private Color _sirstrapAccentColorValue = Color.Parse("#454ee6");

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
        private ObservableCollection<FastFlagEntry> _robloxFastFlags = [];

        [ObservableProperty]
        private bool _robloxFastFlagsEnabled = true;

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

        public Settings(SirstrapConfiguration configuration, IFastFlagService fastFlagService, ISettingsService settingsService, IPathManager pathManager, ISirHurtService sirHurtService)
        {
            _configuration = configuration;
            _fastFlagService = fastFlagService;
            _settingsService = settingsService;
            _pathManager = pathManager;

            _settingsService.LoadSettings();

            var sirHurtPath = sirHurtService.GetSirHurtPath();

            if (Color.TryParse(configuration.SirstrapAccentColor, out var accentColor))
                SirstrapAccentColorValue = accentColor;

            SirstrapAutoUpdate = configuration.SirstrapAutoUpdate;
            SirstrapChannel = configuration.SirstrapChannel;
            SirstrapFontFamily = configuration.SirstrapFontFamily;
            SirstrapTelemetry = configuration.SirstrapTelemetry;
            SirstrapTrayMode = configuration.SirstrapTrayMode;
            RobloxFastFlags = new(fastFlagService.GetFlags().Select(flag => new FastFlagEntry { Name = flag.Key, Value = flag.Value }));
            RobloxFastFlagsEnabled = configuration.RobloxFastFlagsEnabled;
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

            _configuration.SirstrapAccentColor = $"#{SirstrapAccentColorValue.R:x2}{SirstrapAccentColorValue.G:x2}{SirstrapAccentColorValue.B:x2}";
            _configuration.SirstrapAutoUpdate = SirstrapAutoUpdate;
            _configuration.SirstrapChannel = SirstrapChannel;
            _configuration.SirstrapFontFamily = SirstrapFontFamily;
            _configuration.SirstrapTelemetry = SirstrapTelemetry;
            _configuration.SirstrapTrayMode = SirstrapTrayMode;
            _configuration.RobloxFastFlagsEnabled = RobloxFastFlagsEnabled;
            _configuration.RobloxIncognito = RobloxIncognito;

            Dictionary<string, string> flags = new(StringComparer.Ordinal);

            foreach (var entry in RobloxFastFlags.Where(entry => !string.IsNullOrWhiteSpace(entry.Name)))
                flags[entry.Name.Trim()] = entry.Value;

            _fastFlagService.SetFlags(flags);
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
