namespace Sirstrap.Core.Deployment
{
    public sealed class Installer(IPathManager pathManager, IPerformanceTelemetry performanceTelemetry) : IInstaller
    {
        public void Install(Configuration configuration)
        {
            using var scope = performanceTelemetry.Measure("install");

            try
            {
                var targetDirectory = pathManager.GetExtractionPath(configuration.VersionHash);
                var archivePath = pathManager.GetOutputPath(configuration);

                FileSystemOperations.DeleteDirectory(targetDirectory);

                var entryCount = 0;

                try
                {
                    using var archive = ZipFile.OpenRead(archivePath);

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        var entryPath = Path.GetFullPath(Path.Combine(targetDirectory, entry.FullName));
                        var entryDirectory = Path.GetDirectoryName(entryPath);

                        if (entryDirectory != null)
                            FileSystemOperations.CreateDirectory(entryDirectory);

                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            entry.ExtractToFile(entryPath, true);

                            entryCount++;
                        }
                    }
                }
                finally
                {
                    FileSystemOperations.DeleteFile(archivePath);
                }

                scope.SetTag("entryCount", entryCount.ToString());

                performanceTelemetry.RecordCounter("install.entries", new Dictionary<string, object>
                {
                    ["entries"] = entryCount,
                    ["binaryType"] = configuration.BinaryType
                });

                Log.Information("[*] Extracted the archive to {TargetDirectory}.", targetDirectory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to install the archive.");
                scope.MarkFailed();
                throw new InvalidOperationException($"Installation error: {ex.Message}", ex);
            }
        }
    }
}
