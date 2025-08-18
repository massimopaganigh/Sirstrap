namespace Sirstrap.Core
{
    /// <summary>
    /// Provides functionality to install the Roblox application by extracting the 
    /// downloaded ZIP archive to the appropriate version-specific directory.
    /// </summary>
    public static class ApplicationInstaller
    {
        /// <summary>
        /// Installs the application by preparing the target directory, extracting the downloaded 
        /// ZIP archive, and cleaning up temporary files.
        /// </summary>
        /// <param name="configuration">Configuration containing version information 
        /// and file paths needed for installation.</param>
        /// <exception cref="Exception">Rethrows any exceptions that occur during the installation process.</exception>
        /// <remarks>
        /// The installation process includes:
        /// - Clearing any existing installation in the target directory
        /// - Extracting all files from the ZIP archive to the version-specific directory
        /// - Deleting the temporary ZIP file after successful extraction
        /// </remarks>
        public static void Install(Configuration configuration)
        {
            var targetPath = PathManager.GetExtractionPath(configuration.VersionHash);
            var zipPath = configuration.GetOutputPath();
                      
            try
            {
                PrepareInstallDirectory(targetPath);

                try
                {
                    using var archive = ZipFile.OpenRead(zipPath);

                    foreach (var entry in archive.Entries)
                    {
                        var destinationPath = Path.GetFullPath(Path.Combine(targetPath, entry.FullName));

                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? string.Empty);

                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            entry.ExtractToFile(destinationPath, true);
                        }
                    }
                }
                finally
                {
                    DeleteFileWithRetry(zipPath);
                }

                Log.Information("[*] Archive successfully extracted to: {0}", targetPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Installation error: {0}", ex.Message);

                throw;
            }
        }

        /// <summary>
        /// Prepares the installation directory by attempting to remove any existing files.
        /// </summary>
        /// <param name="targetPath">The version-specific directory path where the application will be installed.</param>
        /// <remarks>
        /// First attempts to delete the parent directory recursively. If that fails due to access restrictions,
        /// falls back to deleting individual files to clean up as much as possible before installation.
        /// </remarks>
        private static void PrepareInstallDirectory(string targetPath)
        {
            var parentDir = Directory.GetParent(targetPath)?.FullName;

            if (Directory.Exists(parentDir))
            {
                try
                {
                    Directory.Delete(parentDir, recursive: true);
                }
                catch (UnauthorizedAccessException)
                {
                    foreach (var file in Directory.GetFiles(parentDir, "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (IOException) { /* Continue with other files */ }
                    }
                }
            }

            Directory.CreateDirectory(targetPath);
        }

        /// <summary>
        /// Attempts to delete a file with multiple retries in case of IO contention.
        /// </summary>
        /// <param name="filePath">The path of the file to delete.</param>
        /// <param name="maxAttempts">The maximum number of deletion attempts before giving up.</param>
        /// <remarks>
        /// Implements an exponential backoff strategy between retry attempts.
        /// Silently returns if the file doesn't exist or is successfully deleted.
        /// </remarks>
        private static void DeleteFileWithRetry(string filePath, int maxAttempts = 3)
        {
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    return;
                }
                catch (IOException) when (attempt < maxAttempts)
                {
                    Thread.Sleep(100 * attempt);
                }
            }
        }
    }
}