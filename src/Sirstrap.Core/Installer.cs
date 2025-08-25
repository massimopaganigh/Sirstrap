namespace Sirstrap.Core
{
    public static class Installer
    {
        public static void Install(Configuration configuration)
        {
            try
            {
                string targetDirectory = PathManager.GetExtractionPath(configuration.VersionHash);
                string archivePath = configuration.GetOutputPath();

                targetDirectory.BetterDirectoryDelete();

                try
                {
                    using ZipArchive archive = ZipFile.OpenRead(archivePath);

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string entryPath = Path.GetFullPath(Path.Combine(targetDirectory, entry.FullName));

                        Path.GetDirectoryName(entryPath)?.BetterDirectoryCreate();

                        if (!string.IsNullOrEmpty(entry.Name))
                            entry.ExtractToFile(entryPath, true);
                    }
                }
                finally
                {
                    archivePath.BetterFileDelete();
                }

                Log.Information("[*] Archive successfully extracted to: {0}", targetDirectory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Installation error: {0}", ex.Message);

                throw;
            }
        }
    }
}