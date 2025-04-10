namespace Sirstrap.Core
{
    /// <summary>
    /// Stores application-wide settings for the Sirstrap application that control deployment,
    /// update behavior, and version retrieval preferences.
    /// </summary>
    /// <remarks>
    /// This class defines the configuration parameters that can be customized by users
    /// through the settings.ini file. It provides default values for all settings
    /// which are used when no custom configuration is present.
    /// </remarks>
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the base host path for Roblox deployment resources.
        /// </summary>
        /// <value>
        /// The URL used as the base for all Roblox CDN requests.
        /// Defaults to "https://setup.rbxcdn.com".
        /// </value>
        /// <remarks>
        /// This URL is used for constructing various deployment resource URLs,
        /// including manifest files, package files, and binaries.
        /// </remarks>
        public string RobloxCdnUrl { get; set; } = "https://setup.rbxcdn.com";

        /// <summary>
        /// Gets or sets the update channel for Sirstrap.
        /// </summary>
        /// <value>
        /// The update channel suffix, typically "-beta" for beta releases.
        /// </value>
        /// <remarks>
        /// The update channel determines which GitHub release branch is used
        /// when checking for and downloading Sirstrap updates.
        /// </remarks>
        public string SirstrapUpdateChannel { get; set; } = "-beta";

        /// <summary>
        /// Gets or sets a value indicating whether Safe Mode is enabled for version retrieval.
        /// </summary>
        /// <value>
        /// <c>true</c> to use only the official Roblox API for version information;
        /// <c>false</c> to try SirHurt API first with fallback to Roblox API.
        /// Defaults to <c>true</c>.
        /// </value>
        /// <remarks>
        /// This setting affects how Sirstrap determines the latest Roblox version:
        /// 
        /// When enabled (default):
        /// - Only the official Roblox API is used to retrieve version information
        /// - Provides maximum compatibility and security
        /// 
        /// When disabled:
        /// - The SirHurt API is tried first, which may provide more up-to-date versions for exploits
        /// - Falls back to the official Roblox API if SirHurt API fails
        /// </remarks>
        public bool SafeMode { get; set; } = true;
    }
}