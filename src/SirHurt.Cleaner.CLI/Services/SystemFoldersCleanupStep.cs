namespace SirHurt.Cleaner.CLI.Services
{
    /// <summary>
    /// Removes system-wide installation folders (e.g. Program Files entries).
    /// </summary>
    public sealed class SystemFoldersCleanupStep(ILogger logger, IFolderDeleter folderDeleter, CleanerConfig config) : ICleanupStep
    {
        public string Name => "Clean system folders";

        public void Execute()
        {
            logger.Information("[*] Cleaning {FolderCount} system-wide folder(s)", config.SystemFolders.Count);

            foreach (var folderPath in config.SystemFolders)
                folderDeleter.DeleteFolder(folderPath);
        }
    }
}
