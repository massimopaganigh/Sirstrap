namespace Sirstrap.Core.Models
{
    public static class Directories
    {
        public static string CacheDirectory => Path.Combine(SirstrapDirectory, "Cache");

        public static string SirstrapDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap");

        public static string VersionsDirectory => Path.Combine(SirstrapDirectory, "Versions");
    }
}
