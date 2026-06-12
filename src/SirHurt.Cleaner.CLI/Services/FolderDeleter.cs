namespace SirHurt.Cleaner.CLI.Services
{
    public sealed class FolderDeleter(ILogger logger, IFileSystem fileSystem) : IFolderDeleter
    {
        private void TryForceDelete(string path)
        {
            try
            {
                foreach (var filePath in fileSystem.GetFilesRecursive(path)) fileSystem.ClearReadOnlyAttribute(filePath);
                fileSystem.DeleteDirectory(path, recursive: true);
                logger.Information("[*] Deleted folder after clearing read-only attributes: {FolderPath}", path);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[!] Could not delete folder: {FolderPath}", path);
            }
        }

        public void DeleteFolder(string path)
        {
            if (!fileSystem.DirectoryExists(path))
            {
                logger.Debug("[*] Skipping folder (not found): {FolderPath}", path);
                return;
            }
            try
            {
                logger.Information("[*] Deleting folder: {FolderPath}", path);
                fileSystem.DeleteDirectory(path, recursive: true);
                logger.Information("[*] Deleted folder: {FolderPath}", path);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                logger.Warning(ex, "[!] Folder is locked or read-only, retrying after clearing attributes: {FolderPath}", path);
                TryForceDelete(path);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[!] Could not delete folder: {FolderPath}", path);
            }
        }
    }
}
