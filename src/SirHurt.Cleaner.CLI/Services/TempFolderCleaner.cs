namespace SirHurt.Cleaner.CLI.Services
{
    /// <summary>
    /// Cleans temporary folders for the current user, all other users, and the system.
    /// </summary>
    public sealed class TempFolderCleaner(
        ILogger logger,
        IFileSystem fileSystem,
        IFolderDeleter folderDeleter,
        IUserProfileProvider userProfileProvider,
        CleanerConfig config) : ICleanupStep
    {
        public string Name => "Clean temporary folders";

        public void Execute()
        {
            CleanCurrentUserTempFolder();
            CleanOtherUsersTempFolders();
            CleanSystemTempFolder();
        }

        private void CleanCurrentUserTempFolder()
        {
            string tempPath = Path.GetTempPath();

            logger.Information("[*] Cleaning temporary files for current user: {TempPath}", tempPath);
            CleanTempFolder(tempPath);
        }

        private void CleanOtherUsersTempFolders()
        {
            logger.Information("[*] Scanning other user profiles for temporary files");

            foreach (var userProfile in userProfileProvider.GetOtherUserProfileDirectories())
            {
                string tempPath = Path.Combine(userProfile, config.UserTempPath);

                if (!fileSystem.DirectoryExists(tempPath))
                    continue;

                logger.Information("[*] Cleaning temporary files for user {Username}: {TempPath}", Path.GetFileName(userProfile), tempPath);
                CleanTempFolder(tempPath);
            }
        }

        private void CleanSystemTempFolder()
        {
            string systemTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");

            logger.Information("[*] Cleaning system temporary folder: {TempPath}", systemTempPath);
            CleanTempFolder(systemTempPath);
        }

        private void CleanTempFolder(string tempPath)
        {
            try
            {
                if (!fileSystem.DirectoryExists(tempPath))
                {
                    logger.Warning("[!] Temporary folder not found: {TempPath}", tempPath);
                    return;
                }

                foreach (var filePath in fileSystem.GetFiles(tempPath))
                    TryDeleteFile(filePath);

                foreach (var directory in fileSystem.GetDirectories(tempPath))
                    folderDeleter.DeleteFolder(directory);

                int remainingEntries = fileSystem.GetFileSystemEntries(tempPath).Count();

                if (remainingEntries == 0)
                    logger.Information("[*] Temporary folder fully cleaned: {TempPath}", tempPath);
                else
                    logger.Information("[*] Temporary folder partially cleaned, {RemainingCount} entries are in use: {TempPath}", remainingEntries, tempPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warning(ex, "[!] Access denied to temporary folder: {TempPath}", tempPath);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[!] Unexpected error while cleaning temporary folder: {TempPath}", tempPath);
            }
        }

        private void TryDeleteFile(string filePath)
        {
            try
            {
                fileSystem.DeleteFile(filePath);
            }
            catch (Exception ex)
            {
                logger.Debug(ex, "[*] Skipping temporary file (probably in use): {FilePath}", filePath);
            }
        }
    }
}
