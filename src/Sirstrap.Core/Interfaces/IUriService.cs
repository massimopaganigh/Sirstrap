namespace Sirstrap.Core.Interfaces
{
    public interface IUriService
    {
        public string GetManifestUri();

        public string GetPackageUri(string package);
    }
}
