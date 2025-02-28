namespace Sirstrap.Core
{
    /// <summary>
    /// Provides methods to construct standardized URLs for accessing Roblox
    /// deployment resources including manifests, binaries, and packages.
    /// </summary>
    public static class UrlBuilder
    {
        /// <summary>
        /// The base host path for Roblox deployment resources.
        /// </summary>
        private const string HostPath = "https://setup-cfly.rbxcdn.com";

        /// <summary>
        /// Constructs a URL for downloading a specific binary file.
        /// </summary>
        /// <param name="downloadConfiguration">Configuration containing version, channel, and blob directory information.</param>
        /// <param name="fileName">The name of the binary file to download.</param>
        /// <returns>
        /// A fully qualified URL to the specified binary file.
        /// </returns>
        /// <remarks>
        /// The URL is constructed by combining the base path, blob directory, version number, 
        /// and file name according to Roblox's CDN structure.
        /// </remarks>
        public static string GetBinaryUrl(DownloadConfiguration downloadConfiguration, string fileName)
        {
            return $"{GetBasePath(downloadConfiguration)}{downloadConfiguration.BlobDir}{downloadConfiguration.Version}-{fileName}";
        }

        /// <summary>
        /// Constructs a URL for downloading the package manifest file.
        /// </summary>
        /// <param name="downloadConfiguration">Configuration containing version, channel, and blob directory information.</param>
        /// <returns>
        /// A fully qualified URL to the package manifest file for the specified version.
        /// </returns>
        /// <remarks>
        /// The manifest file follows a standard naming convention of "{version}-rbxPkgManifest.txt".
        /// </remarks>
        public static string GetManifestUrl(DownloadConfiguration downloadConfiguration)
        {
            return $"{GetBasePath(downloadConfiguration)}{downloadConfiguration.BlobDir}{downloadConfiguration.Version}-rbxPkgManifest.txt";
        }

        /// <summary>
        /// Constructs a URL for downloading a specific package file.
        /// </summary>
        /// <param name="downloadConfiguration">Configuration containing version, channel, and blob directory information.</param>
        /// <param name="packageName">The name of the package file to download.</param>
        /// <returns>
        /// A fully qualified URL to the specified package file.
        /// </returns>
        /// <remarks>
        /// Package URLs follow the same pattern as binary URLs, but typically refer to ZIP archives
        /// containing components of the Roblox application.
        /// </remarks>
        public static string GetPackageUrl(DownloadConfiguration downloadConfiguration, string packageName)
        {
            return $"{GetBasePath(downloadConfiguration)}{downloadConfiguration.BlobDir}{downloadConfiguration.Version}-{packageName}";
        }

        /// <summary>
        /// Determines the base path for URLs based on the channel specified in the configuration.
        /// </summary>
        /// <param name="downloadConfiguration">Configuration containing the channel information.</param>
        /// <returns>
        /// The base URL path, which varies depending on whether the channel is "LIVE" or another channel.
        /// </returns>
        /// <remarks>
        /// For the "LIVE" channel, the base host path is used directly.
        /// For other channels, the channel name is appended to the path.
        /// </remarks>
        private static string GetBasePath(DownloadConfiguration downloadConfiguration)
        {
            return downloadConfiguration.Channel.Equals("LIVE", StringComparison.OrdinalIgnoreCase) ? HostPath : $"{HostPath}/channel/{downloadConfiguration.Channel}";
        }
    }
}