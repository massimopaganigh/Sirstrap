namespace Sirstrap.Core
{
    public static class PathManager
    {
        public static string GetExtractionPath(string versionHash) => Path.Combine(SirstrapConfiguration.InstallationPath, versionHash);

        public static string GetLogsPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Logs");

        public static void PurgePreviousInstallationPath()
        {
            try
            {
                var previousPath = SirstrapConfiguration.PreviousInstallationPath;

                if (string.IsNullOrWhiteSpace(previousPath)
                    || !Directory.Exists(previousPath))
                    return;

                if (string.Equals(previousPath, SirstrapConfiguration.InstallationPath, StringComparison.OrdinalIgnoreCase))
                    return;

                Log.Information("[*] Purging previous installation path: {0}...", previousPath);

                previousPath.BetterDirectoryDelete();

                SirstrapConfiguration.PreviousInstallationPath = string.Empty;
                SirstrapConfigurationService.SaveSettings();

                Log.Information("[*] Previous installation path purged successfully.");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, nameof(PurgePreviousInstallationPath));
            }
        }

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
                {
                    try
                    {
                        files[i].Delete();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to delete log file: {0}", files[i].Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, nameof(PurgeOldLogs));
            }
        }
    }
}
