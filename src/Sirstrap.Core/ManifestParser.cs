namespace Sirstrap.Core
{
    public static class ManifestParser
    {
        private static List<string> GetPackages(string[] lines) => [.. lines.Where(line => line.Contains('.')
        && line.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).Select(line => line.Trim())];

        private static bool IsValidManifest(string[] lines) => lines.Length > 0
            && lines[0].Trim().Equals("v0", StringComparison.OrdinalIgnoreCase);

        public static Manifest Parse(string? manifestContext)
        {
            if (string.IsNullOrEmpty(manifestContext))
                return new Manifest();

            var lines = manifestContext.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

            return new Manifest
            {
                IsValid = IsValidManifest(lines),
                Packages = GetPackages(lines)
            };
        }
    }
}