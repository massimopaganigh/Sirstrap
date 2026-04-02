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
        }

        public bool CleanTempFolders { get; set; } = true;

        public IReadOnlyList<string> ExcludedFiles { get; }

        public IReadOnlyList<string> ExcludedSubFolders { get; }

        public IReadOnlyList<string> FilesRequiringConfirmation { get; }

        public IReadOnlyList<string> FoldersRequiringConfirmation { get; }

        public IReadOnlyList<string> ProcessesToClose { get; }

        public IReadOnlyList<string> RegistryKeys { get; }

        public IReadOnlyList<string> SystemFolders { get; }
    }
}
