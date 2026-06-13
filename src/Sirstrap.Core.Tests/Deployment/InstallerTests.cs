namespace Sirstrap.Core.Tests.Deployment
{
    public class InstallerTests
    {
        [Fact]
        public void Install_ExtractsArchive_ToExtractionPath_AndDeletesArchive()
        {
            using TempDirectory temp = new();
            FakePathManager pathManager = new(temp.Path);
            Configuration configuration = new() { BinaryType = "WindowsPlayer", VersionHash = "v1" };

            string outputPath = pathManager.GetOutputPath(configuration);
            File.WriteAllBytes(outputPath, ZipTestHelper.CreateZip(("RobloxPlayerBeta.exe", "exe"), ("sub/file.txt", "data")));

            new Installer(pathManager, NullPerformanceTelemetry.Instance).Install(configuration);

            string extraction = pathManager.GetExtractionPath("v1");

            Assert.True(File.Exists(Path.Combine(extraction, "RobloxPlayerBeta.exe")));
            Assert.Equal("data", File.ReadAllText(Path.Combine(extraction, "sub", "file.txt")));
            Assert.False(File.Exists(outputPath));
        }

        [Fact]
        public void Install_Throws_WhenArchiveMissing()
        {
            using TempDirectory temp = new();
            FakePathManager pathManager = new(temp.Path);
            Configuration configuration = new() { BinaryType = "WindowsPlayer", VersionHash = "missing" };

            Assert.Throws<InvalidOperationException>(() => new Installer(pathManager, NullPerformanceTelemetry.Instance).Install(configuration));
        }
    }
}
