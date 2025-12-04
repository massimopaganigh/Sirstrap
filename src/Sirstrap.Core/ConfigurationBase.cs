namespace Sirstrap.Core
{
    public class ConfigurationBase
    {
        public string BinaryType { get; set; } = "WindowsPlayer";

        public string ChannelName { get; set; } = "LIVE";

        public string VersionHash { get; set; } = string.Empty;

        public string BlobDirectory { get; set; } = "/";
    }
}
