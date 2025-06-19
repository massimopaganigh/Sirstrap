namespace Sirstrap.Core
{
    public static class SirstrapConfiguration
    {
        public static string ChannelName { get; set; } = "-beta";

        public static bool MultiInstance { get; set; }

        public static bool RobloxApi { get; set; }

        public static string RobloxCdnUri { get; set; } = "https://setup.rbxcdn.com";

        /// <summary>
        /// WIP
        /// </summary>
        public static bool Incognito { get; set; }
    }
}