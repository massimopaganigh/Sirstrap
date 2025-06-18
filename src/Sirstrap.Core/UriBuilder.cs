namespace Sirstrap.Core
{
    public static class UriBuilder
    {
        private static string GetBaseUri(Configuration configuration)
        {
            string robloxCdnUrl = AppSettingsManager.GetSettings().RobloxCdnUrl;

            string rawBaseUri = configuration.ChannelName.Equals("LIVE", StringComparison.OrdinalIgnoreCase)
                ? robloxCdnUrl
                : $"{robloxCdnUrl}/channel/{configuration.ChannelName}";

            return $"{rawBaseUri}{configuration.BlobDirectory}{configuration.VersionHash}-";
        }

        public static string GetManifestUri(Configuration configuration) => $"{GetBaseUri(configuration)}rbxPkgManifest.txt";

        public static string GetPackageUri(Configuration configuration, string package) => $"{GetBaseUri(configuration)}{package}";
    }
}