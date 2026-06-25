namespace Sirstrap.Core.Common
{
    public sealed class PathManager(SirstrapConfiguration sirstrapConfiguration, ISettingsService settingsService) : IPathManager
    {
        public void ClearCacheDirectory()
        {
            try
            {
                var cacheDirectory = GetCacheDirectory();

                foreach (var file in Directory.GetFiles(cacheDirectory))
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex, "[!] Failed to delete the cached file {FilePath}.", file);
                    }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to clear the cache directory.");
            }
        }

        public string GetCacheDirectory()
        {
            string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Cache");

            Directory.CreateDirectory(cacheDirectory);

            return cacheDirectory;
        }

        public string GetExtractionPath(string versionHash) => Path.Combine(sirstrapConfiguration.InstallationPath, versionHash);

        public string GetLogsPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Logs");

        public string GetOutputPath(Configuration configuration) => Path.Combine(GetCacheDirectory(), $"{configuration.VersionHash}.zip");

        public void PurgeOldLogs(int maxFiles = 100)
        {
            try
            {
                var logsDirectory = GetLogsPath();

                if (!Directory.Exists(logsDirectory))
                    return;

                var files = new DirectoryInfo(logsDirectory).GetFiles().OrderBy(f => f.LastWriteTimeUtc).ToList();
                var toDelete = files.Count - maxFiles;

                for (var i = 0; i < toDelete; i++)
                {
                    try
                    {
                        files[i].Delete();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "[!] Failed to delete the log file {LogFileName}.", files[i].Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to purge the old logs.");
            }
        }

        public void PurgePreviousInstallationPath()
        {
            try
            {
                var previousPath = sirstrapConfiguration.PreviousInstallationPath;

                if (string.IsNullOrWhiteSpace(previousPath)
                    || !Directory.Exists(previousPath))
                    return;

                if (string.Equals(previousPath, sirstrapConfiguration.InstallationPath, StringComparison.OrdinalIgnoreCase))
                    return;

                Log.Information("[*] Purging the previous installation path {InstallationPath}...", previousPath);
                FileSystemOperations.DeleteDirectory(previousPath);

                sirstrapConfiguration.PreviousInstallationPath = string.Empty;

                settingsService.SaveSettings();
                Log.Information("[*] Purged the previous installation path.");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to purge the previous installation path.");
            }
        }
    }
}
