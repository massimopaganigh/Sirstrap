namespace Sirstrap.Core.Deployment
{
    public class Configuration
    {
        public string BinaryType { get; set; } = "WindowsPlayer";

        public string BlobDirectory { get; set; } = "/";

        public string ChannelName { get; set; } = "LIVE";

        public string LaunchUri { get; set; } = string.Empty;

        public string VersionHash { get; set; } = string.Empty;

        public bool IsMacBinary() => BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) || BinaryType.Equals("MacStudio", StringComparison.OrdinalIgnoreCase);

        public bool IsMacPlayer() => BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase);

        public bool IsWindowsPlayer() => BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase);
    }
}
