namespace Sirstrap.Core.Models
{
    public class ConfigurationBase
    {
        public string BinaryType { get; set; } = "WindowsPlayer";

        public string BlobDirectory { get; set; } = "/";

        public string ChannelName { get; set; } = "LIVE";

        public string VersionHash { get; set; } = string.Empty;
    }
}
