namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Configuration settings for system cleaning operations
    /// </summary>
    public class CleanerConfig
    {
        public CleanerConfig()
        {
            FilesRequiringConfirmation =
            [
                Path.Combine("sirhui", "sirhurta.dat"),
                Path.Combine("sirhui", "sirhurtp.dat"),
                "Sirstrap.ini"
            ];

            FoldersRequiringConfirmation =
            [
                "Versions"
            ];

            ProcessesToClose =
            [
                "RobloxPlayerBeta",
                "SirHurtUI",
                "Sirstrap"
            ];

            RegistryKeys =
            [
                @"Software\Asshurt"
            ];

            SystemFolders =
            [
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "rsTrust")
            ];

            ExcludedSubFolders =
            [
                "DefaultInstances",
                "OTAPlugins",
                "RobloxStudio",
                "RobloxStudioInstaller"
            ];

            ExcludedFiles =
            [
                "GlobalBasicSettings_13_Studio.xml"
            ];
        }

        /// <summary>
        /// Whether to clean temporary folders
        /// </summary>
        public bool CleanTempFolders { get; set; } = true;

        /// <summary>
        /// Files that require confirmation before deletion, relative to their base folders
        /// </summary>
        public IReadOnlyList<string> FilesRequiringConfirmation { get; }

        /// <summary>
        /// Folders that require confirmation before deletion, relative to their base folders
        /// </summary>
        public IReadOnlyList<string> FoldersRequiringConfirmation { get; }

        /// <summary>
        /// Process names that must be closed before cleaning
        /// </summary>
        public IReadOnlyList<string> ProcessesToClose { get; }

        /// <summary>
        /// Registry keys to delete
        /// </summary>
        public IReadOnlyList<string> RegistryKeys { get; }

        /// <summary>
        /// System folder paths to delete
        /// </summary>
        public IReadOnlyList<string> SystemFolders { get; }

        /// <summary>
        /// Subfolder names to skip during folder-contents cleanup (e.g. Roblox Studio-specific folders)
        /// </summary>
        public IReadOnlyList<string> ExcludedSubFolders { get; }

        /// <summary>
        /// File names to skip during folder-contents cleanup (e.g. Roblox Studio-specific files)
        /// </summary>
        public IReadOnlyList<string> ExcludedFiles { get; }
    }
}
