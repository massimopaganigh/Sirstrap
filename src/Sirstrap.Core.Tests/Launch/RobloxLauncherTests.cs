namespace Sirstrap.Core.Tests.Launch
{
    public class RobloxLauncherTests
    {
        [Fact]
        public void Launch_ReturnsFalse_WhenExecutableMissing()
        {
            using TempDirectory temp = new();
            SirstrapConfiguration config = new() { RobloxInstallationPath = temp.Path };
            RobloxLauncher launcher = new(
                config,
                new FakePathManager(temp.Path),
                new FakeSingletonManager(),
                new FakeIncognitoManager(),
                new FakeRobloxProcessService(),
                NullPerformanceTelemetry.Instance,
                new FakeFFlagManager());

            Configuration configuration = new() { BinaryType = "WindowsPlayer", VersionHash = "v1" };

            Assert.False(launcher.Launch(configuration));
        }
    }
}
