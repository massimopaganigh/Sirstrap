namespace Sirstrap.Core
{
    public static class PathManager
    {
        public static string GetExtractionPath(string versionHash) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Versions", versionHash);

        public static string GetLogsPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Logs");

        public static void PurgeOldLogs(int maxFiles = 100)
        {
            try
            {
                var logsDirectory = GetLogsPath();

                if (!Directory.Exists(logsDirectory))
                    return;

                var files = new DirectoryInfo(logsDirectory)
                    .GetFiles()
                    .OrderBy(f => f.LastWriteTimeUtc)
                    .ToList();

                var toDelete = files.Count - maxFiles;

                for (var i = 0; i < toDelete; i++)
                    files[i].Delete();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, nameof(PurgeOldLogs));
            }
        }
    }
}
