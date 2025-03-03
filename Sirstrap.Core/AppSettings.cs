namespace Sirstrap.Core
{
    /// <summary>
    /// Stores application-wide settings for the Sirstrap application.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the base host path for Roblox deployment resources.
        /// </summary>
        /// <value>
        /// The URL used as the base for all Roblox CDN requests.
        /// Defaults to "https://setup-cfly.rbxcdn.com".
        /// </value>
        public string RobloxCdnUrl { get; set; } = "https://setup-cfly.rbxcdn.com";
    }
}