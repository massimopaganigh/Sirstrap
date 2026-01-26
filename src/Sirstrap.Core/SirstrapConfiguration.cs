namespace Sirstrap.Core
{
    public static class SirstrapConfiguration
    {
        public static bool AutoUpdate { get; set; } = true;

        public static string ChannelName { get; set; } = "-alpha";

        public static string FontFamily { get; set; } = "Minecraft";

        public static bool Incognito { get; set; } = false;

        public static bool MultiInstance { get; set; } = true;

        public static bool RobloxApi { get; set; } = false;

        public static string RobloxCdnUri { get; set; } = "https://setup.rbxcdn.com";

        public static string RobloxVersionOverride { get; set; } = string.Empty;

        public static string SirHurtPath => SirHurtService.GetSirHurtPath();
    }
}
