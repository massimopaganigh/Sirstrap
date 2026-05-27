namespace Sirstrap.Core
{
    public static class Installer
    {
        public static void Install(Configuration configuration)
        {
            using ITelemetryScope scope = Telemetry.Performance.Measure("install");

            try
            {
                string targetDirectory = PathManager.GetExtractionPath(configuration.VersionHash);
                string archivePath = configuration.GetOutputPath();

                targetDirectory.BetterDirectoryDelete();

                int entryCount = 0;

                try
                {
                    using ZipArchive archive = ZipFile.OpenRead(archivePath);

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string entryPath = Path.GetFullPath(Path.Combine(targetDirectory, entry.FullName));

                        Path.GetDirectoryName(entryPath)?.BetterDirectoryCreate();

                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            entry.ExtractToFile(entryPath, true);

                            entryCount++;
                        }
                    }
                }
                finally
                {
                    archivePath.BetterFileDelete();
                }

                scope.SetTag("entryCount", entryCount.ToString());

                Telemetry.Performance.RecordCounter("install.entries", new Dictionary<string, object>
                {
                    ["entries"] = entryCount,
                    ["binaryType"] = configuration.BinaryType
                });

                Log.Information("[*] Archive successfully extracted to: {0}", targetDirectory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Installation error: {0}", ex.Message);

                scope.MarkFailed();

                throw new InvalidOperationException($"Installation error: {ex.Message}", ex);
            }
        }
    }
}
