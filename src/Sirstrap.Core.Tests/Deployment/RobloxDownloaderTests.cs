namespace Sirstrap.Core.Tests.Deployment
{
    public class RobloxDownloaderTests
    {
        private sealed record Harness(
            RobloxDownloader Downloader,
            FakeSirstrapUpdateService Update,
            FakeRobloxVersionService Version,
            FakePackageManager Packages,
            FakeCdnResolver Cdn,
            FakeInstaller Installer,
            FakeRobloxLauncher Launcher,
            FakePathManager Paths);

        private static Harness NewHarness(string root, string resolvedVersion = "v1", bool launchResult = true)
        {
            FakeSirstrapUpdateService update = new();
            FakeRobloxVersionService version = new(resolvedVersion);
            FakePackageManager packages = new();
            FakeCdnResolver cdn = new();
            FakeInstaller installer = new();
            FakeRobloxLauncher launcher = new(launchResult);
            FakePathManager paths = new(root);

            RobloxDownloader downloader = new(update, version, packages, cdn, installer, launcher, paths, NullPerformanceTelemetry.Instance);

            return new Harness(downloader, update, version, packages, cdn, installer, launcher, paths);
        }

        [Fact]
        public async Task ExecuteAsync_FreshWindowsInstall_DownloadsInstallsAndLaunches()
        {
            using TempDirectory temp = new();
            Harness h = NewHarness(temp.Path);

            await h.Downloader.ExecuteAsync(["--version-hash", "v1"], SirstrapType.CLI);

            Assert.Equal(1, h.Update.UpdateCalls);
            Assert.Equal(1, h.Cdn.Calls);
            Assert.True(h.Paths.CacheCleared);
            Assert.Equal(1, h.Packages.WindowsCalls);
            Assert.Equal(1, h.Installer.Calls);
            Assert.True(h.Launcher.Calls >= 1);
        }

        [Fact]
        public async Task ExecuteAsync_ResolvesVersion_WhenNotProvided()
        {
            using TempDirectory temp = new();
            Harness h = NewHarness(temp.Path, resolvedVersion: "resolved");

            await h.Downloader.ExecuteAsync([], SirstrapType.CLI);

            Assert.Equal(1, h.Version.Calls);
            Assert.Equal(1, h.Packages.WindowsCalls);
        }

        [Fact]
        public async Task ExecuteAsync_Aborts_WhenVersionResolutionFails()
        {
            using TempDirectory temp = new();
            Harness h = NewHarness(temp.Path, resolvedVersion: string.Empty);

            await h.Downloader.ExecuteAsync([], SirstrapType.CLI);

            Assert.Equal(1, h.Version.Calls);
            Assert.Equal(0, h.Packages.WindowsCalls);
            Assert.Equal(0, h.Cdn.Calls);
        }

        [Fact]
        public async Task ExecuteAsync_CacheHit_LaunchesWithoutDownloading()
        {
            using TempDirectory temp = new();
            Harness h = NewHarness(temp.Path, launchResult: true);

            string extraction = h.Paths.GetExtractionPath("v1");
            Directory.CreateDirectory(extraction);
            await File.WriteAllTextAsync(Path.Combine(extraction, "RobloxPlayerBeta.exe"), "exe", TestContext.Current.CancellationToken);

            await h.Downloader.ExecuteAsync(["--version-hash", "v1"], SirstrapType.CLI);

            Assert.Equal(1, h.Launcher.Calls);
            Assert.Equal(0, h.Packages.WindowsCalls);
            Assert.Equal(0, h.Cdn.Calls);
        }

        [Fact]
        public async Task ExecuteAsync_MacBinary_DownloadsMacArchive_WithoutInstallOrLaunch()
        {
            using TempDirectory temp = new();
            Harness h = NewHarness(temp.Path);

            await h.Downloader.ExecuteAsync(["--binary-type", "MacPlayer", "--version-hash", "v1"], SirstrapType.UI);

            Assert.Equal(1, h.Packages.MacCalls);
            Assert.Equal(0, h.Packages.WindowsCalls);
            Assert.Equal(0, h.Installer.Calls);
            Assert.Equal(0, h.Launcher.Calls);
        }
    }
}
