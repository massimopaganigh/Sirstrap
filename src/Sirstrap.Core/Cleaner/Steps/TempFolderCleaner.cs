namespace Sirstrap.Core.Cleaner.Steps
{
    public sealed class TempFolderCleaner(IFileSystem fileSystem, IFolderDeleter folderDeleter, IUserProfileProvider userProfileProvider, CleanerConfig config) : ICleanupStep
    {
        public void Execute()
        {
            if (!config.CleanTempFolders)
            {
                Log.Information("[*] The temporary folder cleanup is disabled, skipping.");

                return;
            }

            CleanCurrentUserTempFolder();
            CleanOtherUsersTempFolders();
            CleanSystemTempFolder();
        }

        public string Name => "Clean temporary folders";

        #region PRIVATE METHODS
        private void CleanCurrentUserTempFolder()
        {
            var tempPath = Path.GetTempPath();

            Log.Information("[*] Cleaning the temporary files for the current user: {TempPath}...", tempPath);
            CleanTempFolder(tempPath);
        }

        private void CleanOtherUsersTempFolders()
        {
            Log.Information("[*] Scanning the other user profiles for temporary files...");

            foreach (var userProfile in userProfileProvider.GetOtherUserProfileDirectories())
            {
                var tempPath = Path.Combine(userProfile, config.UserTempPath);

                if (!fileSystem.DirectoryExists(tempPath))
                    continue;

                Log.Information("[*] Cleaning the temporary files for the user {Username}: {TempPath}...", Path.GetFileName(userProfile), tempPath);
                CleanTempFolder(tempPath);
            }
        }

        private void CleanSystemTempFolder()
        {
            var systemTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");

            Log.Information("[*] Cleaning the system temporary folder: {TempPath}...", systemTempPath);
            CleanTempFolder(systemTempPath);
        }

        private void CleanTempFolder(string tempPath)
        {
            try
            {
                if (!fileSystem.DirectoryExists(tempPath))
                {
                    Log.Warning("[!] The temporary folder {TempPath} was not found.", tempPath);

                    return;
                }

                foreach (var filePath in fileSystem.GetFiles(tempPath))
                    TryDeleteFile(filePath);

                foreach (var directory in fileSystem.GetDirectories(tempPath))
                    folderDeleter.DeleteFolder(directory);

                var remainingEntries = fileSystem.GetFileSystemEntries(tempPath).Count();

                if (remainingEntries == 0)
                    Log.Information("[*] Fully cleaned the temporary folder {TempPath}.", tempPath);
                else
                    Log.Information("[*] Partially cleaned the temporary folder {TempPath}, {RemainingCount} entries are in use.", tempPath, remainingEntries);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning(ex, "[!] The access to the temporary folder {TempPath} was denied.", tempPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to clean the temporary folder {TempPath}.", tempPath);
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
                Log.Debug(ex, "[*] Skipping the temporary file {FilePath} (probably in use).", filePath);
            }
        }
        #endregion
    }
}
