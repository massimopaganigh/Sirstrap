namespace Sirstrap.Core.Deployment
{
    public interface IRobloxUriFactory
    {
        string GetManifestUri(Configuration configuration);

        string GetManifestUri(Configuration configuration, string robloxCdnUri);

        string GetPackageUri(Configuration configuration, string package);

        string GetPackageUri(Configuration configuration, string package, string robloxCdnUri);
    }
}
