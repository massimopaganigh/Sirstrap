namespace Sirstrap.Core.Cdn
{
    public sealed class CdnProbeUriFactory(IRobloxUriFactory robloxUriFactory) : ICdnProbeUriFactory
    {
        public string Create(Configuration configuration, string baseUri)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            if (configuration.IsMacBinary())
            {
                var package = configuration.IsMacPlayer() ? "RobloxPlayer.zip" : "RobloxStudioApp.zip";

                return robloxUriFactory.GetPackageUri(configuration, package, baseUri);
            }

            return robloxUriFactory.GetManifestUri(configuration, baseUri);
        }
    }
}
