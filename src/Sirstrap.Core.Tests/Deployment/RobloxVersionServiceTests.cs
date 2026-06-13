namespace Sirstrap.Core.Tests.Deployment
{
    public class RobloxVersionServiceTests
    {
        private static RobloxVersionService NewService(SirstrapConfiguration config, HttpClient robloxClient, HttpClient sirHurtClient, out RecordingPerformanceTelemetry telemetry)
        {
            telemetry = new RecordingPerformanceTelemetry();

            return new RobloxVersionService(new RobloxClientVersionApi(robloxClient), new SirHurtVersionApi(sirHurtClient), config, telemetry);
        }

        private static HttpClient RobloxClient(string version) => StubHttpMessageHandler.Client(HttpStatusCode.OK, "{\"clientVersionUpload\":\"" + version + "\"}");

        private static HttpClient SirHurtClient(string version, int ageDays)
        {
            long unix = DateTimeOffset.UtcNow.AddDays(-ageDays).ToUnixTimeSeconds();

            return StubHttpMessageHandler.Client(HttpStatusCode.OK, "[{\"SirHurt V5\":{\"roblox_version\":\"" + version + "\",\"last_update_unix\":" + unix + "}}]");
        }

        [Fact]
        public async Task GetLatestVersionAsync_ReturnsOverride_WhenSet()
        {
            SirstrapConfiguration config = new() { RobloxVersionOverride = "override-version" };
            RobloxVersionService service = NewService(config, RobloxClient("roblox"), SirHurtClient("sirhurt", 1), out var telemetry);

            Assert.Equal("override-version", await service.GetLatestVersionAsync());
            Assert.Contains(telemetry.Scopes, s => s.Operation == "version.resolve");
        }

        [Fact]
        public async Task GetLatestVersionAsync_UsesRobloxApi_WhenEnabled()
        {
            SirstrapConfiguration config = new() { RobloxApi = true };
            RobloxVersionService service = NewService(config, RobloxClient("roblox-version"), SirHurtClient("sirhurt", 1), out _);

            Assert.Equal("roblox-version", await service.GetLatestVersionAsync());
        }

        [Fact]
        public async Task GetLatestVersionAsync_MarksFailed_WhenRobloxApiReturnsEmpty()
        {
            SirstrapConfiguration config = new() { RobloxApi = true };
            RobloxVersionService service = NewService(config, RobloxClient(string.Empty), SirHurtClient("sirhurt", 1), out var telemetry);

            Assert.Equal(string.Empty, await service.GetLatestVersionAsync());
            Assert.True(telemetry.Scopes[0].Failed);
        }

        [Fact]
        public async Task GetLatestVersionAsync_UsesSirHurt_WhenApiDisabledAndRecent()
        {
            SirstrapConfiguration config = new() { RobloxApi = false };
            RobloxVersionService service = NewService(config, RobloxClient("roblox"), SirHurtClient("sirhurt-version", 1), out _);

            Assert.Equal("sirhurt-version", await service.GetLatestVersionAsync());
        }

        [Fact]
        public async Task GetLatestVersionAsync_FallsBackToRobloxApi_WhenSirHurtEmpty()
        {
            SirstrapConfiguration config = new() { RobloxApi = false };
            HttpClient emptySirHurt = StubHttpMessageHandler.Client(HttpStatusCode.OK, """{"not":"array"}""");
            RobloxVersionService service = NewService(config, RobloxClient("roblox-fallback"), emptySirHurt, out _);

            Assert.Equal("roblox-fallback", await service.GetLatestVersionAsync());
        }

        [Fact]
        public async Task GetLatestVersionAsync_FallsBackToRobloxApi_WhenSirHurtOutdated()
        {
            SirstrapConfiguration config = new() { RobloxApi = false };
            RobloxVersionService service = NewService(config, RobloxClient("roblox-newer"), SirHurtClient("sirhurt-old", 20), out _);

            Assert.Equal("roblox-newer", await service.GetLatestVersionAsync());
        }

        [Fact]
        public async Task GetLatestVersionAsync_UsesOutdatedSirHurt_WhenRobloxApiAlsoEmpty()
        {
            SirstrapConfiguration config = new() { RobloxApi = false };
            RobloxVersionService service = NewService(config, RobloxClient(string.Empty), SirHurtClient("sirhurt-old", 20), out _);

            Assert.Equal("sirhurt-old", await service.GetLatestVersionAsync());
        }
    }
}
