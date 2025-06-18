namespace Sirstrap.Core
{
    public class AppSettings
    {
        public string RobloxCdnUrl { get; set; } = "https://setup.rbxcdn.com";

        public string SirstrapUpdateChannel { get; set; } = "-beta";

        public bool SafeMode { get; set; }

        public bool MultiInstance { get; set; }

        public bool IncognitoMode { get; set; }
    }
}