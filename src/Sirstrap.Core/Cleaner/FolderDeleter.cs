namespace Sirstrap.Core.Cleaner
{
    public sealed class FolderDeleter(IFileSystem fileSystem) : IFolderDeleter
    {
        public void DeleteFolder(string path)
        {
            if (!fileSystem.DirectoryExists(path))
            {
                Log.Debug("[*] Skipping the folder {FolderPath} (not found).", path);

                return;
            }

            try
            {
                Log.Information("[*] Deleting the folder {FolderPath}...", path);

                fileSystem.DeleteDirectory(path, recursive: true);

                Log.Information("[*] Deleted the folder {FolderPath}.", path);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                Log.Warning(ex, "[!] The folder {FolderPath} is locked or read-only, retrying after clearing the attributes...", path);

                TryForceDelete(path);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to delete the folder {FolderPath}.", path);
            }
        }

        private void TryForceDelete(string path)
        {
            try
            {
                foreach (var filePath in fileSystem.GetFilesRecursive(path))
                    fileSystem.ClearReadOnlyAttribute(filePath);

                fileSystem.DeleteDirectory(path, recursive: true);

                Log.Information("[*] Deleted the folder {FolderPath} after clearing the read-only attributes.", path);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to delete the folder {FolderPath}.", path);
            }
        }
    }
}
