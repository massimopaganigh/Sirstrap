namespace Sirstrap.Core
{
    public static class SirstrapConfiguration
    {
        public static bool AutoUpdate { get; set; } = true;

        public static string ChannelName { get; set; } = "-beta";

        public static string FontFamily { get; set; } = "Minecraft";

        /// <summary>
        /// WIP
        /// </summary>
        public static bool Incognito { get; set; } = false;

        public static bool MultiInstance { get; set; } = true;

        public static bool RobloxApi { get; set; } = false;

        public static string RobloxCdnUri { get; set; } = "https://setup.rbxcdn.com";

        public static string SirHurtPath { get; set; } = string.Empty;
    }
}
