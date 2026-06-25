namespace Sirstrap.Core.Tests.Support
{
    public sealed class FakePathManager(string root) : IPathManager
    {
        public string Root { get; } = root;

        public bool CacheCleared { get; private set; }

        public bool OldLogsPurged { get; private set; }

        public bool PreviousInstallationPurged { get; private set; }

        public void ClearCacheDirectory() => CacheCleared = true;

        public string GetCacheDirectory()
        {
            string path = Path.Combine(Root, "Cache");

            Directory.CreateDirectory(path);

            return path;
        }

        public string GetExtractionPath(string versionHash) => Path.Combine(Root, "Versions", versionHash);

        public string GetLogsPath() => Path.Combine(Root, "Logs");

        public string GetOutputPath(Configuration configuration) => Path.Combine(GetCacheDirectory(), $"{configuration.VersionHash}.zip");

        public void PurgeOldLogs(int maxFiles = 100) => OldLogsPurged = true;

        public void PurgePreviousInstallationPath() => PreviousInstallationPurged = true;
    }
}
