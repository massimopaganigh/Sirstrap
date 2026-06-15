namespace Sirstrap.Core.Cleaner.Steps
{
    public sealed class SystemFoldersCleanupStep(IFolderDeleter folderDeleter, CleanerConfig config) : ICleanupStep
    {
        public void Execute()
        {
            Log.Information("[*] Cleaning {FolderCount} system-wide folder(s)...", config.SystemFolders.Count);

            foreach (var folderPath in config.SystemFolders)
                folderDeleter.DeleteFolder(folderPath);
        }

        public string Name => "Clean system folders";
    }
}
