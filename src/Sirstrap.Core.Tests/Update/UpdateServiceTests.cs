namespace Sirstrap.Core.Tests.Update
{
    public class UpdateServiceTests
    {
        private static SirstrapUpdateService NewService(SirstrapConfiguration config, ISirstrapVersion version, HttpClient releasesClient, out RecordingPerformanceTelemetry telemetry)
        {
            telemetry = new RecordingPerformanceTelemetry();

            return new SirstrapUpdateService(new GitHubReleaseClient(releasesClient), new UpdateApplier(new HttpClient()), config, version, telemetry);
        }

        private static HttpClient ReleasesClient(string json) => StubHttpMessageHandler.Client(HttpStatusCode.OK, json);

        [Fact]
        public async Task UpdateAsync_IsDisabled_WhenAutoUpdateOff()
        {
            SirstrapConfiguration config = new() { SirstrapAutoUpdate = false };
            SirstrapUpdateService service = NewService(config, new FakeSirstrapVersion(new Version("1.0.0.0")), ReleasesClient("[]"), out var telemetry);

            await service.UpdateAsync(SirstrapType.CLI, []);

            Assert.Contains(telemetry.Counters, c => c.Name == "update.check.outcome" && Equals(c.Tags?["outcome"], "Disabled"));
        }

        [Fact]
        public async Task UpdateAsync_IsUpToDate_WhenLatestNotNewer()
        {
            SirstrapConfiguration config = new() { SirstrapAutoUpdate = true, SirstrapChannel = "-beta" };
            FakeSirstrapVersion version = new(new Version("9.9.9.9"), "-beta");
            HttpClient releases = ReleasesClient("""[{"tag_name":"v0.0.1.0-beta","draft":false,"body":"old"}]""");
            SirstrapUpdateService service = NewService(config, version, releases, out var telemetry);

            await service.UpdateAsync(SirstrapType.CLI, []);

            Assert.Contains(telemetry.Counters, c => c.Name == "update.check.outcome" && Equals(c.Tags?["outcome"], "UpToDate"));
        }

        [Fact]
        public async Task UpdateAsync_IsUpToDate_WhenReleaseLookupFails()
        {
            SirstrapConfiguration config = new() { SirstrapAutoUpdate = true, SirstrapChannel = "-beta" };
            FakeSirstrapVersion version = new(new Version("1.0.0.0"), "-beta");
            SirstrapUpdateService service = NewService(config, version, ReleasesClient("[]"), out var telemetry);

            await service.UpdateAsync(SirstrapType.CLI, []);

            Assert.Contains(telemetry.Counters, c => c.Name == "update.check.outcome" && Equals(c.Tags?["outcome"], "UpToDate"));
        }

        [Fact]
        public async Task UpdateAsync_Fails_WhenUpdateNeededButAssetMissing()
        {
            SirstrapConfiguration config = new() { SirstrapAutoUpdate = true, SirstrapChannel = "-beta" };
            FakeSirstrapVersion version = new(new Version("1.0.0.0"), "-beta");
            HttpClient releases = ReleasesClient("""[{"tag_name":"v2.0.0.0-beta","draft":false,"body":"newer","assets":[]}]""");
            SirstrapUpdateService service = NewService(config, version, releases, out var telemetry);

            await service.UpdateAsync(SirstrapType.CLI, []);

            Assert.Contains(telemetry.Counters, c => c.Name == "update.check.outcome" && Equals(c.Tags?["outcome"], "Failed"));
        }

        [Fact]
        public async Task GetLatestChangelogAsync_ReturnsBody_ForMatchingChannel()
        {
            SirstrapConfiguration config = new() { SirstrapChannel = "-beta" };
            FakeSirstrapVersion version = new(new Version("1.0.0.0"), "-beta");
            HttpClient releases = ReleasesClient("""[{"tag_name":"v2.0.0.0-beta","draft":false,"body":"the changelog"}]""");
            SirstrapUpdateService service = NewService(config, version, releases, out _);

            Assert.Equal("the changelog", await service.GetLatestChangelogAsync());
        }

        [Fact]
        public async Task GetLatestChangelogAsync_ReturnsEmpty_WhenNoMatchingRelease()
        {
            SirstrapConfiguration config = new() { SirstrapChannel = "-beta" };
            FakeSirstrapVersion version = new(new Version("1.0.0.0"), "-beta");
            HttpClient releases = ReleasesClient("""[{"tag_name":"v2.0.0.0-alpha","draft":false,"body":"other channel"}]""");
            SirstrapUpdateService service = NewService(config, version, releases, out _);

            Assert.Equal(string.Empty, await service.GetLatestChangelogAsync());
        }

        [Fact]
        public async Task GetLatestChangelogAsync_IgnoresDraftReleases()
        {
            SirstrapConfiguration config = new() { SirstrapChannel = "-beta" };
            FakeSirstrapVersion version = new(new Version("1.0.0.0"), "-beta");
            HttpClient releases = ReleasesClient("""[{"tag_name":"v3.0.0.0-beta","draft":true,"body":"draft body"}]""");
            SirstrapUpdateService service = NewService(config, version, releases, out _);

            Assert.Equal(string.Empty, await service.GetLatestChangelogAsync());
        }

        [Fact]
        public void UpdateOutcome_HasExpectedMembers()
        {
            Assert.Equal(["Disabled", "UpToDate", "Updated", "Failed"], Enum.GetNames<UpdateOutcome>());
        }
    }
}
