namespace SirHurt.Cleaner.CLI.Services
{
    /// <summary>
    /// Cleans a folder's contents while skipping excluded entries and asking the user
    /// before deleting configuration or authentication files and folders.
    /// </summary>
    public sealed class SelectiveFolderCleaner(
        ILogger logger,
        IFileSystem fileSystem,
        IUserInteraction userInteraction,
        IFolderDeleter folderDeleter,
        CleanerConfig config) : ISelectiveFolderCleaner
    {
        private readonly HashSet<string> _protectedFileDirectories = config.FilesRequiringConfirmation
            .Select(f => Path.GetDirectoryName(f) ?? string.Empty)
            .Where(d => d.Length > 0)
            .ToHashSet(StringComparer.Ordinal);

        public void CleanFolderContents(string folderPath)
        {
            if (!fileSystem.DirectoryExists(folderPath))
            {
                logger.Debug("[*] Skipping folder (not found): {FolderPath}", folderPath);
                return;
            }

            logger.Information("[*] Cleaning folder: {FolderPath}", folderPath);

            try
            {
                foreach (var filePath in fileSystem.GetFiles(folderPath))
                {
                    if (config.ExcludedFiles.Contains(Path.GetFileName(filePath)))
                    {
                        logger.Debug("[*] Keeping excluded file: {FilePath}", filePath);
                        continue;
                    }

                    DeleteFile(filePath, folderPath);
                }

                foreach (var subDir in fileSystem.GetDirectories(folderPath))
                    CleanSubDirectory(subDir, folderPath);

                DeleteDirectoryIfEmpty(folderPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warning(ex, "[!] Access denied while cleaning folder: {FolderPath}", folderPath);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[!] Unexpected error while cleaning folder: {FolderPath}", folderPath);
            }
        }

        private void CleanSubDirectory(string subDir, string baseFolderPath)
        {
            string subDirName = Path.GetFileName(subDir);

            if (config.ExcludedSubFolders.Contains(subDirName) || IsRobloxStudioBuildFolder(subDirName))
            {
                logger.Debug("[*] Keeping excluded folder: {FolderPath}", subDir);
                return;
            }

            if (config.FoldersRequiringConfirmation.Contains(Path.GetRelativePath(baseFolderPath, subDir)))
            {
                DeleteFolderWithConfirmation(subDir);
                return;
            }

            if (!_protectedFileDirectories.Contains(subDirName))
            {
                folderDeleter.DeleteFolder(subDir);
                return;
            }

            foreach (var filePath in fileSystem.GetFiles(subDir))
                DeleteFile(filePath, baseFolderPath);

            DeleteDirectoryIfEmpty(subDir);
        }

        private void DeleteFolderWithConfirmation(string folderPath)
        {
            logger.Information("[*] Folder contains configuration or authentication data: {FolderPath}", folderPath);

            if (userInteraction.Confirm($"The folder {folderPath} contains configuration or authentication data. Delete it?"))
            {
                folderDeleter.DeleteFolder(folderPath);
                logger.Information("[*] Deleted protected folder after user confirmation: {FolderPath}", folderPath);
            }
            else
            {
                logger.Information("[*] Kept protected folder at user's request: {FolderPath}", folderPath);
            }
        }

        private void DeleteFile(string filePath, string baseFolderPath)
        {
            try
            {
                if (config.FilesRequiringConfirmation.Contains(Path.GetRelativePath(baseFolderPath, filePath)))
                {
                    logger.Information("[*] File contains authentication or configuration data: {FilePath}", filePath);

                    if (!userInteraction.Confirm($"The file {filePath} is used for authentication or configuration. Delete it?"))
                    {
                        logger.Information("[*] Kept protected file at user's request: {FilePath}", filePath);
                        return;
                    }

                    fileSystem.DeleteFile(filePath);
                    logger.Information("[*] Deleted protected file after user confirmation: {FilePath}", filePath);
                    return;
                }

                fileSystem.DeleteFile(filePath);
                logger.Debug("[*] Deleted file: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[!] Could not delete file: {FilePath}", filePath);
            }
        }

        private void DeleteDirectoryIfEmpty(string path)
        {
            if (!fileSystem.DirectoryExists(path) || fileSystem.GetFileSystemEntries(path).Any())
                return;

            try
            {
                fileSystem.DeleteDirectory(path, recursive: false);
                logger.Information("[*] Deleted empty folder: {FolderPath}", path);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[!] Could not delete empty folder: {FolderPath}", path);
            }
        }

        /// <summary>
        /// Returns true if the folder name consists entirely of digits.
        /// Roblox Studio stores build-version folders with purely numeric names (e.g. "5082874847").
        /// </summary>
        private static bool IsRobloxStudioBuildFolder(string name) =>
            !string.IsNullOrEmpty(name) && name.All(char.IsDigit);
    }
}
