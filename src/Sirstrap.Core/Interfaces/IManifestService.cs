namespace Sirstrap.Core.Interfaces
{
    public interface IManifestService
    {
        public Manifest GetManifest(string? manifestContent);
    }
}
