namespace Sirstrap.Core.Tests.Cleaner
{
    public class CleanupStepsTests
    {
        private readonly CleanerConfig _config = new();

        [Fact]
        public void ProcessCloser_NoRunningProcesses_DoesNotConfirmOrKill()
        {
            FakeProcessManager processes = new();
            FakeUserInteraction interaction = new();
            ProcessCloser step = new(processes, interaction, _config);

            step.Execute();

            Assert.Empty(interaction.Messages);
            Assert.Empty(processes.Killed);
            Assert.Equal("Close running applications", step.Name);
        }

        [Fact]
        public void ProcessCloser_KillsRunningProcesses_WhenConfirmed()
        {
            FakeProcessManager processes = new("RobloxPlayerBeta");
            ProcessCloser step = new(processes, new FakeUserInteraction(confirmResult: true), _config);

            step.Execute();

            Assert.Contains("RobloxPlayerBeta", processes.Killed);
        }

        [Fact]
        public void ProcessCloser_DoesNotKill_WhenDeclined()
        {
            FakeProcessManager processes = new("RobloxPlayerBeta");
            ProcessCloser step = new(processes, new FakeUserInteraction(confirmResult: false), _config);

            step.Execute();

            Assert.Empty(processes.Killed);
        }

        [Fact]
        public void SystemFoldersCleanupStep_DeletesConfiguredSystemFolders()
        {
            FakeFolderDeleter deleter = new();
            SystemFoldersCleanupStep step = new(deleter, _config);

            step.Execute();

            Assert.Equal(_config.SystemFolders.OrderBy(x => x), deleter.Deleted.OrderBy(x => x));
            Assert.Equal("Clean system folders", step.Name);
        }

        [Fact]
        public void UserFoldersCleanupStep_CleansCurrentAndOtherProfiles()
        {
            FakeSelectiveFolderCleaner cleaner = new();
            FakeUserProfileProvider profiles = new(@"C:\Users\Other");
            UserFoldersCleanupStep step = new(cleaner, profiles, _config);

            step.Execute();

            string currentProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            Assert.Contains(Path.GetFullPath(Path.Combine(currentProfile, _config.UserFolders[0])), cleaner.Cleaned);
            Assert.Contains(Path.GetFullPath(Path.Combine(@"C:\Users\Other", _config.UserFolders[0])), cleaner.Cleaned);
            Assert.Equal("Clean user folders", step.Name);
        }

        [Fact]
        public void UserFoldersCleanupStep_ContinuesAfterProfileException()
        {
            ThrowingSelectiveFolderCleaner cleaner = new();
            FakeUserProfileProvider profiles = new(@"C:\Users\Other");
            UserFoldersCleanupStep step = new(cleaner, profiles, _config);

            Assert.Null(Record.Exception(step.Execute));
            Assert.True(cleaner.Calls > 0);
        }

        private sealed class ThrowingSelectiveFolderCleaner : ISelectiveFolderCleaner
        {
            public int Calls { get; private set; }

            public void CleanFolderContents(string folderPath)
            {
                Calls++;

                throw new UnauthorizedAccessException(folderPath);
            }
        }

        [Fact]
        public void RegistryCleanupStep_CleansUserAllUsersAndMachineKeys()
        {
            FakeCleanupRegistryManager registry = new();
            RegistryCleanupStep step = new(registry, _config);

            step.Execute();

            Assert.Equal(_config.RegistryKeys, registry.CurrentUser);
            Assert.Equal(_config.RegistryKeys, registry.AllUsers);
            Assert.Equal(_config.LocalMachineRegistryKeys, registry.LocalMachine);
            Assert.Equal("Clean registry", step.Name);
        }

        [Fact]
        public void TempFolderCleaner_SkipsCleanup_WhenDisabled()
        {
            FakeFileSystem fs = new();
            FakeFolderDeleter deleter = new();
            CleanerConfig config = new() { CleanTempFolders = false };
            TempFolderCleaner step = new(fs, deleter, new FakeUserProfileProvider(), config);

            step.Execute();

            Assert.Empty(fs.DeletedFiles);
            Assert.Empty(deleter.Deleted);
            Assert.Equal("Clean temporary folders", step.Name);
        }

        [Fact]
        public void TempFolderCleaner_CleansCurrentSystemAndOtherUserTempFolders_WhenEnabled()
        {
            string otherProfile = @"C:\Users\Other";
            string otherTemp = Path.Combine(otherProfile, _config.UserTempPath);
            string currentTemp = Path.GetTempPath();
            string systemTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");

            FakeFileSystem fs = new();
            foreach (var temp in new[] { otherTemp, currentTemp, systemTemp })
            {
                fs.AddDirectory(temp);
                fs.AddFile(Path.Combine(temp, "leftover.tmp"));
                fs.AddDirectory(Path.Combine(temp, "cache"));
            }

            FakeFolderDeleter deleter = new();
            TempFolderCleaner step = new(fs, deleter, new FakeUserProfileProvider(otherProfile), _config);

            step.Execute();

            Assert.Contains(Path.Combine(otherTemp, "leftover.tmp"), fs.DeletedFiles);
            Assert.Contains(Path.Combine(currentTemp, "leftover.tmp"), fs.DeletedFiles);
            Assert.Contains(Path.Combine(systemTemp, "cache"), deleter.Deleted);
        }

        [Fact]
        public void TempFolderCleaner_WarnsAndSkips_WhenTempFoldersMissing()
        {
            FakeFolderDeleter deleter = new();
            TempFolderCleaner step = new(new FakeFileSystem(), deleter, new FakeUserProfileProvider(@"C:\Users\Ghost"), _config);

            Assert.Null(Record.Exception(step.Execute));
            Assert.Empty(deleter.Deleted);
        }
    }
}
