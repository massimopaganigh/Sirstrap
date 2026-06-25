namespace Sirstrap.Core.Tests.Deployment
{
    public class PackageManagerTests
    {
        private static PackageManager NewManager(HttpClient client, IPathManager pathManager, SirstrapConfiguration? sirstrapConfiguration = null)
        {
            sirstrapConfiguration ??= new SirstrapConfiguration();

            return new(client, new RobloxUriFactory(sirstrapConfiguration, new CdnUriNormalizer()), pathManager, NullPerformanceTelemetry.Instance, sirstrapConfiguration);
        }

        [Fact]
        public async Task DownloadWindowsArchiveAsync_BuildsZip_WithAppSettingsAndPackages()
        {
            using TempDirectory temp = new();
            FakePathManager pathManager = new(temp.Path);

            HttpClient client = StubHttpMessageHandler.Client(request =>
            {
                string uri = request.RequestUri!.ToString();

                if (uri.EndsWith("rbxPkgManifest.txt", StringComparison.Ordinal))
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("v0\nCustomExtra.zip\n") };

                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("app-bytes")) };
            });

            Configuration configuration = new() { BinaryType = "WindowsPlayer", VersionHash = "v1" };

            await NewManager(client, pathManager).DownloadWindowsArchiveAsync(configuration);

            var entries = ZipTestHelper.ReadZip(pathManager.GetOutputPath(configuration));

            Assert.Contains("AppSettings.xml", entries.Keys);
            Assert.Equal("app-bytes", entries["CustomExtra.zip"]);
        }

        [Fact]
        public async Task DownloadWindowsArchiveAsync_DoesNothing_WhenManifestInvalid()
        {
            using TempDirectory temp = new();
            FakePathManager pathManager = new(temp.Path);

            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, "garbage-manifest");
            Configuration configuration = new() { BinaryType = "WindowsPlayer", VersionHash = "v1" };

            await NewManager(client, pathManager).DownloadWindowsArchiveAsync(configuration);

            Assert.False(File.Exists(pathManager.GetOutputPath(configuration)));
        }

        [Fact]
        public async Task DownloadMacArchiveAsync_WritesArchiveBytes()
        {
            using TempDirectory temp = new();
            FakePathManager pathManager = new(temp.Path);

            HttpClient client = StubHttpMessageHandler.Client(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("mac-bytes"))
            });

            Configuration configuration = new() { BinaryType = "MacPlayer", VersionHash = "v1" };

            await NewManager(client, pathManager).DownloadMacArchiveAsync(configuration);

            Assert.Equal("mac-bytes", await File.ReadAllTextAsync(pathManager.GetOutputPath(configuration), TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task DownloadMacArchiveAsync_Throws_WhenDownloadFails()
        {
            using TempDirectory temp = new();
            FakePathManager pathManager = new(temp.Path);

            HttpClient client = StubHttpMessageHandler.Client(_ => throw new HttpRequestException("down"));
            Configuration configuration = new() { BinaryType = "MacPlayer", VersionHash = "v1" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => NewManager(client, pathManager).DownloadMacArchiveAsync(configuration));
        }
    }
}
