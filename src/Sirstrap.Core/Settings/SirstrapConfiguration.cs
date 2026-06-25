namespace Sirstrap.Core.Settings
{
    public class SirstrapConfiguration
    {
        public static string GetDefaultInstallationPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Versions");

        public bool AutoUpdate { get; set; } = true;

        public string ChannelName { get; set; } = "-beta";

        public string FontFamily { get; set; } = "JetBrains Mono";

        public bool Incognito { get; set; }

        public string InstallationPath { get; set; } = GetDefaultInstallationPath();

        public bool MultiInstance { get; set; } = true;

        public string PreviousInstallationPath { get; set; } = string.Empty;

        public string ResolvedRobloxCdnUri { get; set; } = RobloxCdnService.DefaultBaseUri;

        public IReadOnlyList<string> ResolvedRobloxCdnUris { get; set; } = [RobloxCdnService.DefaultBaseUri];

        public bool RobloxApi { get; set; }

        public string RobloxCdnUriOverride { get; set; } = string.Empty;

        public string RobloxVersionOverride { get; set; } = string.Empty;

        public bool Telemetry { get; set; } = true;

        public TrayMode TrayMode { get; set; } = TrayMode.None;
    }
}
