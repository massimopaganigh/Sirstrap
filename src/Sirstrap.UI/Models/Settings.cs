namespace Sirstrap.UI.Models
{
    public partial class Settings : ModelBase
    {
        [ObservableProperty]
        private bool _autoUpdate = true;

        [ObservableProperty]
        private string _channelName = string.Empty;

        [ObservableProperty]
        private bool _incognito;

        [ObservableProperty]
        private bool _multiInstance = true;

        [ObservableProperty]
        private bool _robloxApi;

        [ObservableProperty]
        private string _robloxCdnUri = "https://setup.rbxcdn.com";

        public Settings()
        {
            AutoUpdate = SirstrapConfiguration.AutoUpdate;
            ChannelName = SirstrapConfiguration.ChannelName;
            Incognito = SirstrapConfiguration.Incognito;
            MultiInstance = SirstrapConfiguration.MultiInstance;
            RobloxApi = SirstrapConfiguration.RobloxApi;
            RobloxCdnUri = SirstrapConfiguration.RobloxCdnUri;
        }

        public void Set()
        {
            SirstrapConfiguration.AutoUpdate = AutoUpdate;
            SirstrapConfiguration.ChannelName = ChannelName;
            SirstrapConfiguration.Incognito = Incognito;
            SirstrapConfiguration.MultiInstance = MultiInstance;
            SirstrapConfiguration.RobloxApi = RobloxApi;
            SirstrapConfiguration.RobloxCdnUri = RobloxCdnUri;

            SirstrapConfigurationService.SaveSettings();
            SirstrapConfigurationService.LoadSettings();
        }
    }
}
