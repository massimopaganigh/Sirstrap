namespace Sirstrap.Core.Tests.Cleaner
{
    public class CleanerConfigTests
    {
        [Fact]
        public void Defaults_ArePopulated()
        {
            CleanerConfig config = new();

            Assert.True(config.CleanTempFolders);
            Assert.Equal(Path.Combine("AppData", "Local", "Temp"), config.UserTempPath);
            Assert.Contains("RobloxPlayerBeta", config.ProcessesToClose);
            Assert.Contains(@"Software\Roblox", config.RegistryKeys);
            Assert.Contains(@"SOFTWARE\Roblox Corporation", config.LocalMachineRegistryKeys);
            Assert.NotEmpty(config.SystemFolders);
            Assert.NotEmpty(config.UserFolders);
            Assert.Contains("GlobalBasicSettings_13_Studio.xml", config.ExcludedFiles);
            Assert.Contains("RobloxStudio", config.ExcludedSubFolders);
            Assert.Contains("Sirstrap.ini", config.FilesRequiringConfirmation);
            Assert.Empty(config.FoldersRequiringConfirmation);
        }

        [Fact]
        public void CleanProtectedFiles_DefaultsToFalse_AndIsSettable()
        {
            CleanerConfig config = new();
            Assert.False(config.CleanProtectedFiles);

            config.CleanProtectedFiles = true;
            Assert.True(config.CleanProtectedFiles);
        }
    }
}
