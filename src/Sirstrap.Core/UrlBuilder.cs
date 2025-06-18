namespace Sirstrap.Core
{
    /// <summary>
    /// Provides methods to construct standardized URLs for accessing Roblox
    /// deployment resources including manifests, binaries, and packages.
    /// </summary>
    public static class UrlBuilder
    {
        /// <summary>
        /// Gets the base host path for Roblox deployment resources from application settings.
        /// </summary>
        /// <returns>
        /// The configured host path for Roblox CDN.
        /// </returns>
        private static string GetHostPath()
        {
            return SettingsManager.GetSettings().RobloxCdnUrl;
        }

        /// <summary>
        /// Constructs a URL for downloading a specific binary file.
        /// </summary>
        /// <param name="configuration">Configuration containing version, channel, and blob directory information.</param>
        /// <param name="fileName">The name of the binary file to download.</param>
        /// <returns>
        /// A fully qualified URL to the specified binary file.
        /// </returns>
        /// <remarks>
        /// The URL is constructed by combining the base path, blob directory, version number, 
        /// and file name according to Roblox's CDN structure.
        /// </remarks>
        public static string GetBinaryUrl(Configuration configuration, string fileName)
        {
            return $"{GetBasePath(configuration)}{configuration.BlobDirectory}{configuration.VersionHash}-{fileName}";
        }

        /// <summary>
        /// Constructs a URL for downloading the package manifest file.
        /// </summary>
        /// <param name="configuration">Configuration containing version, channel, and blob directory information.</param>
        /// <returns>
        /// A fully qualified URL to the package manifest file for the specified version.
        /// </returns>
        /// <remarks>
        /// The manifest file follows a standard naming convention of "{version}-rbxPkgManifest.txt".
        /// </remarks>
        public static string GetManifestUrl(Configuration configuration)
        {
            return $"{GetBasePath(configuration)}{configuration.BlobDirectory}{configuration.VersionHash}-rbxPkgManifest.txt";
        }

        /// <summary>
        /// Constructs a URL for downloading a specific package file.
        /// </summary>
        /// <param name="configuration">Configuration containing version, channel, and blob directory information.</param>
        /// <param name="packageName">The name of the package file to download.</param>
        /// <returns>
        /// A fully qualified URL to the specified package file.
        /// </returns>
        /// <remarks>
        /// Package URLs follow the same pattern as binary URLs, but typically refer to ZIP archives
        /// containing components of the Roblox application.
        /// </remarks>
        public static string GetPackageUrl(Configuration configuration, string packageName)
        {
            return $"{GetBasePath(configuration)}{configuration.BlobDirectory}{configuration.VersionHash}-{packageName}";
        }

        /// <summary>
        /// Determines the base path for URLs based on the channel specified in the configuration.
        /// </summary>
        /// <param name="configuration">Configuration containing the channel information.</param>
        /// <returns>
        /// The base URL path, which varies depending on whether the channel is "LIVE" or another channel.
        /// </returns>
        /// <remarks>
        /// For the "LIVE" channel, the base host path is used directly.
        /// For other channels, the channel name is appended to the path.
        /// The host path is configurable via the application settings.
        /// </remarks>
        private static string GetBasePath(Configuration configuration)
        {
            string hostPath = GetHostPath();

            return configuration.ChannelName!.Equals("LIVE", StringComparison.OrdinalIgnoreCase) ? hostPath : $"{hostPath}/channel/{configuration.ChannelName}";
        }
    }
}