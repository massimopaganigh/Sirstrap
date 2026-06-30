namespace Sirstrap.Core.Tests.Deployment
{
    public class RobloxVersionServiceTests
    {
        private static RobloxVersionService NewService(SirstrapConfiguration config, HttpClient robloxClient, HttpClient sirHurtClient, out RecordingPerformanceTelemetry telemetry, IWeaoService? weaoService = null)
        {
            telemetry = new RecordingPerformanceTelemetry();

            return new RobloxVersionService(new RobloxClientVersionApi(robloxClient), new SirHurtVersionApi(sirHurtClient), weaoService ?? new FakeWeaoService(), config, telemetry);
        }

        private static HttpClient RobloxClient(string version) => StubHttpMessageHandler.Client(HttpStatusCode.OK, "{\"clientVersionUpload\":\"" + version + "\"}");

        private static HttpClient SirHurtClient(string version, int ageDays)
        {
            long unix = DateTimeOffset.UtcNow.AddDays(-ageDays).ToUnixTimeSeconds();

            return StubHttpMessageHandler.Client(HttpStatusCode.OK, "[{\"SirHurt V5\":{\"roblox_version\":\"" + version + "\",\"last_update_unix\":" + unix + "}}]");
        }

        [Fact]
        public async Task GetLatestVersionAsync_ReturnsPinnedVersion_WhenSourceIsVersion()
        {
            SirstrapConfiguration config = new() { RobloxVersionSource = RobloxVersionSources.VersionPrefix + "pinned-version" };
            RobloxVersionService service = NewService(config, RobloxClient("roblox"), SirHurtClient("sirhurt", 1), out var telemetry);

            Assert.Equal("pinned-version", await service.GetLatestVersionAsync());
            Assert.Contains(telemetry.Scopes, s => s.Operation == "version.resolve");
        }

        [Fact]
        public async Task GetLatestVersionAsync_UsesRobloxApi_WhenSourceIsRoblox()
        {
            SirstrapConfiguration config = new() { RobloxVersionSource = RobloxVersionSources.Roblox };
            RobloxVersionService service = NewService(config, RobloxClient("roblox-version"), SirHurtClient("sirhurt", 1), out _);

            Assert.Equal("roblox-version", await service.GetLatestVersionAsync());
        }

        [Fact]
        public async Task GetLatestVersionAsync_MarksFailed_WhenRobloxApiReturnsEmpty()
        {
            SirstrapConfiguration config = new() { RobloxVersionSource = RobloxVersionSources.Roblox };
            RobloxVersionService service = NewService(config, RobloxClient(string.Empty), SirHurtClient("sirhurt", 1), out var telemetry);

            Assert.Equal(string.Empty, await service.GetLatestVersionAsync());
            Assert.True(telemetry.Scopes[0].Failed);
        }

        [Fact]
        public async Task GetLatestVersionAsync_UsesSirHurt_WhenSourceIsSirHurtAndRecent()
        {
            SirstrapConfiguration config = new() { RobloxVersionSource = RobloxVersionSources.SirHurt };
            RobloxVersionService service = NewService(config, RobloxClient("roblox"), SirHurtClient("sirhurt-version", 1), out _);

            Assert.Equal("sirhurt-version", await service.GetLatestVersionAsync());
        }

        [Fact]
        public async Task GetLatestVersionAsync_FallsBackToRobloxApi_WhenSirHurtEmpty()
        {
            SirstrapConfiguration config = new() { RobloxVersionSource = RobloxVersionSources.SirHurt };
            HttpClient emptySirHurt = StubHttpMessageHandler.Client(HttpStatusCode.OK, """{"not":"array"}""");
            RobloxVersionService service = NewService(config, RobloxClient("roblox-fallback"), emptySirHurt, out _);

            Assert.Equal("roblox-fallback", await service.GetLatestVersionAsync());
        }

        [Fact]
        public async Task GetLatestVersionAsync_FallsBackToRobloxApi_WhenSirHurtOutdated()
        {
            SirstrapConfiguration config = new() { RobloxVersionSource = RobloxVersionSources.SirHurt };
            RobloxVersionService service = NewService(config, RobloxClient("roblox-newer"), SirHurtClient("sirhurt-old", 20), out _);

            Assert.Equal("roblox-newer", await service.GetLatestVersionAsync());
        }

        [Fact]
        public async Task GetLatestVersionAsync_UsesOutdatedSirHurt_WhenRobloxApiAlsoEmpty()
        {
            SirstrapConfiguration config = new() { RobloxVersionSource = RobloxVersionSources.SirHurt };
            RobloxVersionService service = NewService(config, RobloxClient(string.Empty), SirHurtClient("sirhurt-old", 20), out _);

            Assert.Equal("sirhurt-old", await service.GetLatestVersionAsync());
        }

        [Fact]
        public async Task GetLatestVersionAsync_UsesWeao_WhenSourceIsWeao()
        {
            SirstrapConfiguration config = new() { RobloxVersionSource = RobloxVersionSources.Weao };
            FakeWeaoService weao = new() { CurrentWindowsVersion = "weao-version" };
            RobloxVersionService service = NewService(config, RobloxClient("roblox"), SirHurtClient("sirhurt", 1), out _, weao);

            Assert.Equal("weao-version", await service.GetLatestVersionAsync());
        }

        [Fact]
        public async Task GetLatestVersionAsync_UsesExecutorVersion_WhenSourceIsExecutor()
        {
            SirstrapConfiguration config = new() { RobloxVersionSource = RobloxVersionSources.ExecutorPrefix + "Wave" };
            FakeWeaoService weao = new() { ExecutorVersion = "executor-version" };
            RobloxVersionService service = NewService(config, RobloxClient("roblox"), SirHurtClient("sirhurt", 1), out _, weao);

            Assert.Equal("executor-version", await service.GetLatestVersionAsync());
        }

        [Fact]
        public async Task GetLatestVersionAsync_FallsBackToRobloxApi_WhenWeaoUnavailable()
        {
            SirstrapConfiguration config = new() { RobloxVersionSource = RobloxVersionSources.Weao };
            RobloxVersionService service = NewService(config, RobloxClient("roblox-fallback"), SirHurtClient("sirhurt", 1), out _, new FakeWeaoService());

            Assert.Equal("roblox-fallback", await service.GetLatestVersionAsync());
        }
    }
}
