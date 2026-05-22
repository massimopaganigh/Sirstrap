namespace Sirstrap.Core
{
    public static class UriBuilder
    {
        private static string GetBaseUri(Configuration configuration, string robloxCdnUri)
        {
            string robloxCdnUrl = RobloxCdnService.NormalizeCdnUriOverride(robloxCdnUri);

            if (string.IsNullOrEmpty(robloxCdnUrl))
                robloxCdnUrl = RobloxCdnService.DefaultBaseUri;

            string rawBaseUri = configuration.ChannelName.Equals("LIVE", StringComparison.OrdinalIgnoreCase)
                ? robloxCdnUrl
                : $"{robloxCdnUrl}/channel/{configuration.ChannelName}";

            return $"{rawBaseUri}{configuration.BlobDirectory}{configuration.VersionHash}-";
        }

        private static string GetBaseUri(Configuration configuration) => GetBaseUri(configuration, SirstrapConfiguration.ResolvedRobloxCdnUri);

        public static string GetManifestUri(Configuration configuration) => $"{GetBaseUri(configuration)}rbxPkgManifest.txt";

        public static string GetManifestUri(Configuration configuration, string robloxCdnUri) => $"{GetBaseUri(configuration, robloxCdnUri)}rbxPkgManifest.txt";

        public static string GetPackageUri(Configuration configuration, string package) => $"{GetBaseUri(configuration)}{package}";

        public static string GetPackageUri(Configuration configuration, string package, string robloxCdnUri) => $"{GetBaseUri(configuration, robloxCdnUri)}{package}";
    }
}
