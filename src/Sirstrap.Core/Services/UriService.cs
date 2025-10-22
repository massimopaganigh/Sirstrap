namespace Sirstrap.Core.Services
{
    public class UriService(Configuration configuration) : IUriService
    {
        private string GetBaseUri()
        {
            var robloxCdnUrl = SirstrapConfiguration.RobloxCdnUri;
            var rawBaseUri = configuration.ChannelName.Equals("LIVE", StringComparison.OrdinalIgnoreCase) ? robloxCdnUrl : $"{robloxCdnUrl}/channel/{configuration.ChannelName}";

            return $"{rawBaseUri}{configuration.BlobDirectory}{configuration.VersionHash}-";
        }

        public string GetManifestUri() => $"{GetBaseUri()}rbxPkgManifest.txt";

        public string GetPackageUri(string package) => $"{GetBaseUri()}{package}";
    }
}
