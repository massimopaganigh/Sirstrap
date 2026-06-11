namespace Sirstrap.Core
{
    public static class UriBuilder
    {
        private static string BuildBaseUri(Configuration configuration, string baseUri)
        {
            string rawBaseUri = configuration.ChannelName.Equals("LIVE", StringComparison.OrdinalIgnoreCase)
                ? baseUri
                : $"{baseUri}/channel/{configuration.ChannelName}";

            return $"{rawBaseUri}{configuration.BlobDirectory}{configuration.VersionHash}-";
        }

        private static string GetBaseUri(Configuration configuration) => BuildBaseUri(configuration, SirstrapConfiguration.ResolvedRobloxCdnUri);

        private static string GetBaseUri(Configuration configuration, string robloxCdnUri)
        {
            string normalized = RobloxCdnService.NormalizeCdnUriOverride(robloxCdnUri);

            if (string.IsNullOrEmpty(normalized))
                normalized = RobloxCdnService.DefaultBaseUri;

            return BuildBaseUri(configuration, normalized);
        }

        public static string GetManifestUri(Configuration configuration) => $"{GetBaseUri(configuration)}rbxPkgManifest.txt";

        public static string GetManifestUri(Configuration configuration, string robloxCdnUri) => $"{GetBaseUri(configuration, robloxCdnUri)}rbxPkgManifest.txt";

        public static string GetPackageUri(Configuration configuration, string package) => $"{GetBaseUri(configuration)}{package}";

        public static string GetPackageUri(Configuration configuration, string package, string robloxCdnUri) => $"{GetBaseUri(configuration, robloxCdnUri)}{package}";
    }
}
