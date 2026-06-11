namespace Sirstrap.Core
{
    public sealed class CdnProbeUriFactory : ICdnProbeUriFactory
    {
        public string Create(Configuration configuration, string baseUri)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            if (configuration.IsMacBinary())
            {
                string package = configuration.BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase)
                    ? "RobloxPlayer.zip"
                    : "RobloxStudioApp.zip";

                return UriBuilder.GetPackageUri(configuration, package, baseUri);
            }

            return UriBuilder.GetManifestUri(configuration, baseUri);
        }
    }
}
