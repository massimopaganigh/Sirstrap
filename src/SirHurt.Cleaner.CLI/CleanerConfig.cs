namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Cleanup configuration: the folders, registry keys and processes targeted by the cleaner,
    /// plus the entries that must be skipped or confirmed by the user before deletion.
    /// </summary>
    public sealed class CleanerConfig
    {
        public bool CleanTempFolders { get; init; } = true;

        /// <summary>
        /// Temp folder location relative to a user profile root.
        /// </summary>
        public string UserTempPath { get; } = Path.Combine("AppData", "Local", "Temp");

        public IReadOnlyList<string> ExcludedFiles { get; } =
        [
            "GlobalBasicSettings_13_Studio.xml"
        ];

        public IReadOnlyList<string> ExcludedSubFolders { get; } =
        [
            "DefaultInstances",
            "OTAPlugins",
            "RobloxStudio",
            "RobloxStudioInstaller"
        ];

        public IReadOnlyList<string> FilesRequiringConfirmation { get; } =
        [
            Path.Combine("sirhui", "sirhurta.dat"), // SirHurt username
            Path.Combine("sirhui", "sirhurtp.dat"), // SirHurt password

            "Sirstrap.ini" // Sirstrap configuration
        ];

        public IReadOnlyList<string> FoldersRequiringConfirmation { get; } =
        [
            //"Versions"
        ];

        public IReadOnlyList<string> LocalMachineRegistryKeys { get; } =
        [
            @"SOFTWARE\Roblox Corporation",
            @"SOFTWARE\WOW6432Node\Roblox Corporation"
        ];

        public IReadOnlyList<string> ProcessesToClose { get; } =
        [
            "RobloxPlayerBeta",
            "RobloxStudioBeta",
            "SirHurtUI",
            "Sirstrap",
            "RobloxCrashHandler",
            "RobloxPlayerInstaller",
            "RobloxStudioInstaller"
        ];

        public IReadOnlyList<string> RegistryKeys { get; } =
        [
            @"Software\Asshurt",
            @"Software\Roblox",
            @"Software\Roblox Corporation"
        ];

        public IReadOnlyList<string> SystemFolders { get; } =
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "rsTrust"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Roblox")
        ];

        public IReadOnlyList<string> UserFolders { get; } =
        [
            Path.Combine("AppData", "Local", "Roblox"),
            Path.Combine("AppData", "LocalLow", "Roblox"),
            Path.Combine("AppData", "Roaming", "Roblox"),
            Path.Combine("AppData", "Local", "Roblox Corporation"),
            Path.Combine("AppData", "Roaming", "sirhurt"),
            Path.Combine("AppData", "Local", "Sirstrap")
        ];
    }
}
