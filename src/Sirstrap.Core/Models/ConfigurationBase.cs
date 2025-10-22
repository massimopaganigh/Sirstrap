namespace Sirstrap.Core.Models
{
    public class RobloxDownloadConfigurationBase
    {
        public string BinaryType { get; set; } = "WindowsPlayer";

        public string BlobDirectory { get; set; } = "/";

        public string ChannelName { get; set; } = "LIVE";

        public string VersionHash { get; set; } = string.Empty;
    }
}
