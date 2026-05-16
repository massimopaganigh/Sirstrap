namespace SirHurt.Cleaner.CLI
{
    public class CleanerConfig
    {
        public CleanerConfig()
        {
            ExcludedFiles = ["GlobalBasicSettings_13_Studio.xml"];
            ExcludedSubFolders =
            [
                "DefaultInstances",
                "OTAPlugins",
                "RobloxStudio",
                "RobloxStudioInstaller"
            ];
            FilesRequiringConfirmation =
            [
                Path.Combine("sirhui", "sirhurta.dat"), // SirHurt username
                Path.Combine("sirhui", "sirhurtp.dat"),         // password

                "Sirstrap.ini" // Sirstrap related shit
            ];
            FoldersRequiringConfirmation =
            [
                //"Versions"
            ];
            ProcessesToClose =
            [
                "RobloxPlayerBeta",
                "RobloxStudioBeta",
                "SirHurtUI",
                "Sirstrap",
                "RobloxCrashHandler",
                "RobloxPlayerInstaller",
                "RobloxStudioInstaller"
            ];
            RegistryKeys =
            [
                @"Software\Asshurt",
                @"Software\Roblox",
                @"Software\Roblox Corporation"
            ];
            LocalMachineRegistryKeys =
            [
                @"SOFTWARE\Roblox Corporation",
                @"SOFTWARE\WOW6432Node\Roblox Corporation"
            ];
            SystemFolders =
            [
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "rsTrust"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Roblox")
            ];
            UserFolders =
            [
                Path.Combine("AppData", "Local", "Roblox"),
                Path.Combine("AppData", "LocalLow", "Roblox"),
                Path.Combine("AppData", "Roaming", "Roblox"),
                Path.Combine("AppData", "Local", "Roblox Corporation"),
                Path.Combine("AppData", "Roaming", "sirhurt"),
                Path.Combine("AppData", "Local", "Sirstrap")
            ];
        }

        public bool CleanTempFolders { get; set; } = true;

        public IReadOnlyList<string> ExcludedFiles { get; }

        public IReadOnlyList<string> ExcludedSubFolders { get; }

        public IReadOnlyList<string> FilesRequiringConfirmation { get; }

        public IReadOnlyList<string> FoldersRequiringConfirmation { get; }

        public IReadOnlyList<string> ProcessesToClose { get; }

        public IReadOnlyList<string> RegistryKeys { get; }

        public IReadOnlyList<string> LocalMachineRegistryKeys { get; }

        public IReadOnlyList<string> SystemFolders { get; }

        public IReadOnlyList<string> UserFolders { get; }
    }
}
