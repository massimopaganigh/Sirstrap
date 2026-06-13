namespace Sirstrap.Core.Tests.Deployment
{
    public class VersionApiTests
    {
        [Fact]
        public async Task RobloxClientVersionApi_ReturnsVersion_FromClientVersionUpload()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, """{"clientVersionUpload":"version-abc"}""");
            RobloxClientVersionApi api = new(client);

            Assert.Equal("version-abc", await api.GetVersionAsync());
        }

        [Fact]
        public async Task RobloxClientVersionApi_ReturnsEmpty_WhenFieldMissing()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, """{"other":"x"}""");
            RobloxClientVersionApi api = new(client);

            Assert.Equal(string.Empty, await api.GetVersionAsync());
        }

        [Fact]
        public async Task RobloxClientVersionApi_ReturnsEmpty_OnException()
        {
            HttpClient client = StubHttpMessageHandler.Client(_ => throw new HttpRequestException("boom"));
            RobloxClientVersionApi api = new(client);

            Assert.Equal(string.Empty, await api.GetVersionAsync());
        }

        [Fact]
        public async Task SirHurtVersionApi_ReturnsVersion_AndNotOutdated_ForRecentUpdate()
        {
            long recent = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, "[{\"SirHurt V5\":{\"roblox_version\":\"version-xyz\",\"last_update_unix\":" + recent + "}}]");
            SirHurtVersionApi api = new(client);

            var (version, isOutdated) = await api.GetVersionAsync();

            Assert.Equal("version-xyz", version);
            Assert.False(isOutdated);
        }

        [Fact]
        public async Task SirHurtVersionApi_MarksOutdated_WhenLastUpdateOlderThanTenDays()
        {
            long old = DateTimeOffset.UtcNow.AddDays(-15).ToUnixTimeSeconds();
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, "[{\"SirHurt V5\":{\"roblox_version\":\"version-xyz\",\"last_update_unix\":" + old + "}}]");
            SirHurtVersionApi api = new(client);

            var (version, isOutdated) = await api.GetVersionAsync();

            Assert.Equal("version-xyz", version);
            Assert.True(isOutdated);
        }

        [Fact]
        public async Task SirHurtVersionApi_ReturnsEmpty_ForNonArrayJson()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, """{"not":"array"}""");
            SirHurtVersionApi api = new(client);

            var (version, isOutdated) = await api.GetVersionAsync();

            Assert.Equal(string.Empty, version);
            Assert.False(isOutdated);
        }

        [Fact]
        public async Task SirHurtVersionApi_ReturnsEmpty_WhenSirHurtFieldMissing()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, """[{"Other":{}}]""");
            SirHurtVersionApi api = new(client);

            var (version, _) = await api.GetVersionAsync();

            Assert.Equal(string.Empty, version);
        }

        [Fact]
        public async Task SirHurtVersionApi_ReturnsEmpty_WhenVersionMissing()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, """[{"SirHurt V5":{"last_update_unix":0}}]""");
            SirHurtVersionApi api = new(client);

            var (version, _) = await api.GetVersionAsync();

            Assert.Equal(string.Empty, version);
        }

        [Fact]
        public async Task SirHurtVersionApi_ReturnsEmpty_OnException()
        {
            HttpClient client = StubHttpMessageHandler.Client(_ => throw new HttpRequestException("boom"));
            SirHurtVersionApi api = new(client);

            var (version, _) = await api.GetVersionAsync();

            Assert.Equal(string.Empty, version);
        }
    }
}
