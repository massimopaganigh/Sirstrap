namespace Sirstrap.Core.Services
{
    public class ManifestService : IManifestService
    {
        public Manifest GetManifest(string? manifestContent)
        {
            if (string.IsNullOrEmpty(manifestContent))
                return new Manifest();

            var manifestRows = manifestContent.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

            return new Manifest
            {
                IsValid = manifestRows.Length > 0 && manifestRows[0].Trim().Equals("v0", StringComparison.OrdinalIgnoreCase),
                Packages = [.. manifestRows.Where(line => line.Contains('.') && line.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).Select(line => line.Trim())]
            };
        }
    }
}
