using Sirstrap.Core.Models;

namespace Sirstrap.Core
{
    public static class UriBuilder
    {
        private static string GetBaseUri(RobloxDownloadConfiguration configuration)
        {
            string robloxCdnUrl = SirstrapConfiguration.RobloxCdnUri;

            string rawBaseUri = configuration.ChannelName.Equals("LIVE", StringComparison.OrdinalIgnoreCase)
                ? robloxCdnUrl
                : $"{robloxCdnUrl}/channel/{configuration.ChannelName}";

            return $"{rawBaseUri}{configuration.BlobDirectory}{configuration.VersionHash}-";
        }

        public static string GetManifestUri(RobloxDownloadConfiguration configuration) => $"{GetBaseUri(configuration)}rbxPkgManifest.txt";

        public static string GetPackageUri(RobloxDownloadConfiguration configuration, string package) => $"{GetBaseUri(configuration)}{package}";
    }
}