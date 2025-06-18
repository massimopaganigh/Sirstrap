namespace Sirstrap.Core
{
    public static class PathManager
    {
        public static string GetExtractionPath(string versionHash) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Versions", versionHash);
    }
}