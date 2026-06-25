namespace Sirstrap.Core.Deployment
{
    public sealed class RobloxUriFactory(SirstrapConfiguration sirstrapConfiguration, ICdnUriNormalizer cdnUriNormalizer) : IRobloxUriFactory
    {
        public string GetManifestUri(Configuration configuration) => $"{GetBaseUri(configuration)}rbxPkgManifest.txt";

        public string GetManifestUri(Configuration configuration, string robloxCdnUri) => $"{GetBaseUri(configuration, robloxCdnUri)}rbxPkgManifest.txt";

        public string GetPackageUri(Configuration configuration, string package) => $"{GetBaseUri(configuration)}{package}";

        public string GetPackageUri(Configuration configuration, string package, string robloxCdnUri) => $"{GetBaseUri(configuration, robloxCdnUri)}{package}";

        private static string BuildBaseUri(Configuration configuration, string baseUri)
        {
            string rawBaseUri = configuration.ChannelName.Equals("LIVE", StringComparison.OrdinalIgnoreCase)
                ? baseUri
                : $"{baseUri}/channel/{configuration.ChannelName}";

            return $"{rawBaseUri}{configuration.BlobDirectory}{configuration.VersionHash}-";
        }

        private string GetBaseUri(Configuration configuration) => BuildBaseUri(configuration, sirstrapConfiguration.ResolvedRobloxCdnUri);

        private string GetBaseUri(Configuration configuration, string robloxCdnUri)
        {
            string normalized = cdnUriNormalizer.Normalize(robloxCdnUri);

            if (string.IsNullOrEmpty(normalized))
                normalized = RobloxCdnService.DefaultBaseUri;

            return BuildBaseUri(configuration, normalized);
        }
    }
}
