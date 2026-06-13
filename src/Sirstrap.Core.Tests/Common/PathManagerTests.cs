namespace Sirstrap.Core.Tests.Common
{
    public class PathManagerTests
    {
        private static PathManager NewPathManager(SirstrapConfiguration config, out FakeSettingsService settings)
        {
            settings = new FakeSettingsService();

            return new PathManager(config, settings);
        }

        [Fact]
        public void GetExtractionPath_CombinesInstallationPathAndVersion()
        {
            SirstrapConfiguration config = new() { InstallationPath = @"C:\Install" };
            PathManager pathManager = NewPathManager(config, out _);

            Assert.Equal(Path.Combine(@"C:\Install", "v1"), pathManager.GetExtractionPath("v1"));
        }

        [Fact]
        public void GetOutputPath_IsZipUnderCacheDirectory()
        {
            PathManager pathManager = NewPathManager(new SirstrapConfiguration(), out _);
            Configuration configuration = new() { VersionHash = "v9" };

            string output = pathManager.GetOutputPath(configuration);

            Assert.Equal(pathManager.GetCacheDirectory(), Path.GetDirectoryName(output));
            Assert.Equal("v9.zip", Path.GetFileName(output));
        }

        [Fact]
        public void GetLogsPath_AndCacheDirectory_PointUnderSirstrap()
        {
            PathManager pathManager = NewPathManager(new SirstrapConfiguration(), out _);

            Assert.EndsWith(Path.Combine("Sirstrap", "Logs"), pathManager.GetLogsPath());
            Assert.EndsWith(Path.Combine("Sirstrap", "Cache"), pathManager.GetCacheDirectory());
            Assert.True(Directory.Exists(pathManager.GetCacheDirectory()));
        }

        [Fact]
        public void ClearCacheDirectory_RemovesCachedFiles()
        {
            PathManager pathManager = NewPathManager(new SirstrapConfiguration(), out _);
            string marker = Path.Combine(pathManager.GetCacheDirectory(), $"sirstrap-test-{Guid.NewGuid():N}.zip");
            File.WriteAllText(marker, "cached");

            pathManager.ClearCacheDirectory();

            Assert.False(File.Exists(marker));
        }

        [Fact]
        public void PurgeOldLogs_DoesNotThrow()
        {
            PathManager pathManager = NewPathManager(new SirstrapConfiguration(), out _);

            Assert.Null(Record.Exception(() => pathManager.PurgeOldLogs()));
        }

        [Fact]
        public void PurgePreviousInstallationPath_DeletesPreviousDirectory_AndSavesSettings()
        {
            using TempDirectory temp = new();
            string previous = temp.Combine("previous");
            Directory.CreateDirectory(previous);

            SirstrapConfiguration config = new()
            {
                InstallationPath = temp.Combine("current"),
                PreviousInstallationPath = previous
            };
            PathManager pathManager = NewPathManager(config, out var settings);

            pathManager.PurgePreviousInstallationPath();

            Assert.False(Directory.Exists(previous));
            Assert.Equal(string.Empty, config.PreviousInstallationPath);
            Assert.Equal(1, settings.SaveCalls);
        }

        [Fact]
        public void PurgePreviousInstallationPath_DoesNothing_WhenPreviousEqualsCurrent()
        {
            using TempDirectory temp = new();
            string shared = temp.Combine("shared");
            Directory.CreateDirectory(shared);

            SirstrapConfiguration config = new()
            {
                InstallationPath = shared,
                PreviousInstallationPath = shared
            };
            PathManager pathManager = NewPathManager(config, out var settings);

            pathManager.PurgePreviousInstallationPath();

            Assert.True(Directory.Exists(shared));
            Assert.Equal(0, settings.SaveCalls);
        }

        [Fact]
        public void PurgePreviousInstallationPath_DoesNothing_WhenPreviousMissing()
        {
            SirstrapConfiguration config = new() { PreviousInstallationPath = string.Empty };
            PathManager pathManager = NewPathManager(config, out var settings);

            pathManager.PurgePreviousInstallationPath();

            Assert.Equal(0, settings.SaveCalls);
        }
    }
}
