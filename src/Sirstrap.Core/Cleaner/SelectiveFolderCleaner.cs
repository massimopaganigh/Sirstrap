namespace Sirstrap.Core.Cleaner
{
    public sealed class SelectiveFolderCleaner(
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
                Log.Debug("[*] Skipping the folder {FolderPath} (not found).", folderPath);

                return;
            }

            Log.Information("[*] Cleaning the folder {FolderPath}...", folderPath);

            try
            {
                foreach (var filePath in fileSystem.GetFiles(folderPath))
                {
                    if (config.ExcludedFiles.Contains(Path.GetFileName(filePath)))
                    {
                        Log.Debug("[*] Keeping the excluded file {FilePath}.", filePath);

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
                Log.Warning(ex, "[!] The access to the folder {FolderPath} was denied.", folderPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to clean the folder {FolderPath}.", folderPath);
            }
        }

        private static bool IsRobloxStudioBuildFolder(string name) => !string.IsNullOrEmpty(name) && name.All(char.IsDigit);

        private void CleanSubDirectory(string subDir, string baseFolderPath)
        {
            string subDirName = Path.GetFileName(subDir);

            if (config.ExcludedSubFolders.Contains(subDirName) || IsRobloxStudioBuildFolder(subDirName))
            {
                Log.Debug("[*] Keeping the excluded folder {FolderPath}.", subDir);

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

        private void DeleteDirectoryIfEmpty(string path)
        {
            if (!fileSystem.DirectoryExists(path) || fileSystem.GetFileSystemEntries(path).Any())
                return;

            try
            {
                fileSystem.DeleteDirectory(path, recursive: false);

                Log.Information("[*] Deleted the empty folder {FolderPath}.", path);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to delete the empty folder {FolderPath}.", path);
            }
        }

        private void DeleteFile(string filePath, string baseFolderPath)
        {
            try
            {
                if (config.FilesRequiringConfirmation.Contains(Path.GetRelativePath(baseFolderPath, filePath)))
                {
                    Log.Information("[*] The file {FilePath} contains authentication or configuration data.", filePath);

                    if (!userInteraction.Confirm($"The file {filePath} is used for authentication or configuration. Delete it?"))
                    {
                        Log.Information("[*] Kept the protected file {FilePath} at the user's request.", filePath);

                        return;
                    }

                    fileSystem.DeleteFile(filePath);

                    Log.Information("[*] Deleted the protected file {FilePath} after user confirmation.", filePath);

                    return;
                }

                fileSystem.DeleteFile(filePath);

                Log.Debug("[*] Deleted the file {FilePath}.", filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to delete the file {FilePath}.", filePath);
            }
        }

        private void DeleteFolderWithConfirmation(string folderPath)
        {
            Log.Information("[*] The folder {FolderPath} contains configuration or authentication data.", folderPath);

            if (userInteraction.Confirm($"The folder {folderPath} contains configuration or authentication data. Delete it?"))
            {
                folderDeleter.DeleteFolder(folderPath);

                Log.Information("[*] Deleted the protected folder {FolderPath} after user confirmation.", folderPath);
            }
            else
                Log.Information("[*] Kept the protected folder {FolderPath} at the user's request.", folderPath);
        }
    }
}
