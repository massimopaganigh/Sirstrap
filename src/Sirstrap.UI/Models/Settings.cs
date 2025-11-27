namespace Sirstrap.UI.Models
{
    public partial class Settings : ModelBase
    {
        [ObservableProperty]
        private bool _autoUpdate = true;

        [ObservableProperty]
        private string _channelName = string.Empty;

        [ObservableProperty]
        private string _fontFamily = "Minecraft";

        [ObservableProperty]
        private bool _incognito = false;

        [ObservableProperty]
        private bool _multiInstance = true;

        [ObservableProperty]
        private bool _robloxApi = false;

        [ObservableProperty]
        private string _robloxCdnUri = "https://setup.rbxcdn.com";

        [ObservableProperty]
        private string _robloxVersionOverride = string.Empty;

        [ObservableProperty]
        private string _sirHurtPath = string.Empty;

        public Settings()
        {
            SirstrapConfigurationService.LoadSettings();

            AutoUpdate = SirstrapConfiguration.AutoUpdate;
            ChannelName = SirstrapConfiguration.ChannelName;
            FontFamily = SirstrapConfiguration.FontFamily;
            Incognito = SirstrapConfiguration.Incognito;
            MultiInstance = SirstrapConfiguration.MultiInstance;
            RobloxApi = SirstrapConfiguration.RobloxApi;
            RobloxCdnUri = SirstrapConfiguration.RobloxCdnUri;
            RobloxVersionOverride = SirstrapConfiguration.RobloxVersionOverride;
            SirHurtPath = SirstrapConfiguration.SirHurtPath;
        }

        partial void OnMultiInstanceChanged(bool value)
        {
            if (!value
                && Incognito)
                Incognito = false;
        }

        public void Set()
        {
            SirstrapConfiguration.AutoUpdate = AutoUpdate;
            SirstrapConfiguration.ChannelName = ChannelName;
            SirstrapConfiguration.FontFamily = FontFamily;
            SirstrapConfiguration.Incognito = Incognito;
            SirstrapConfiguration.MultiInstance = MultiInstance;
            SirstrapConfiguration.RobloxApi = RobloxApi;
            SirstrapConfiguration.RobloxCdnUri = RobloxCdnUri;
            SirstrapConfiguration.RobloxVersionOverride = RobloxVersionOverride;
            SirstrapConfiguration.SirHurtPath = SirHurtPath;

            SirstrapConfigurationService.SaveSettings();
            SirstrapConfigurationService.LoadSettings();
        }
    }
}
