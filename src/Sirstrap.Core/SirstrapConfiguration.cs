namespace Sirstrap.Core
{
    public static class SirstrapConfiguration
    {
        public static string GetDefaultInstallationPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Versions");

        public static bool AutoUpdate { get; set; } = true;

        public static string ChannelName { get; set; } = "-beta";

        public static string FontFamily { get; set; } = "JetBrains Mono";

        public static bool Incognito { get; set; } = false;

        public static string InstallationPath { get; set; } = GetDefaultInstallationPath();

        public static bool MultiInstance { get; set; } = true;

        public static string PreviousInstallationPath { get; set; } = string.Empty;

        public static bool RobloxApi { get; set; } = false;

        public static string ResolvedRobloxCdnUri { get; set; } = RobloxCdnService.DefaultBaseUri;

        // Ordered fastest-first; downloads walk this list when a file is missing from the selected CDN.
        public static IReadOnlyList<string> ResolvedRobloxCdnUris { get; set; } = [RobloxCdnService.DefaultBaseUri];

        public static string RobloxCdnUriOverride { get; set; } = string.Empty;

        public static string RobloxVersionOverride { get; set; } = string.Empty;

        public static string SirHurtPath => SirHurtService.GetSirHurtPath();

        public static bool Telemetry { get; set; } = true;

        public static TrayMode TrayMode { get; set; } = TrayMode.None;
    }
}
