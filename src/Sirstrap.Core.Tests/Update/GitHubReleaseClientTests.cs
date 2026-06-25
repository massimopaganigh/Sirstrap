namespace Sirstrap.Core.Tests.Update
{
    public class GitHubReleaseClientTests
    {
        [Fact]
        public async Task GetReleasesAsync_ParsesReleaseArray()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, """[{"tag_name":"v1.0.0.0-beta","draft":false,"body":"a"},{"tag_name":"v2.0.0.0-beta","draft":true,"body":"b"}]""");
            GitHubReleaseClient releaseClient = new(client);

            var releases = await releaseClient.GetReleasesAsync();

            Assert.Equal(2, releases.Count);
            Assert.Equal("v1.0.0.0-beta", releases[0].TagName);
            Assert.True(releases[1].IsDraft);
        }

        [Fact]
        public async Task GetReleasesAsync_ReturnsEmpty_OnException()
        {
            HttpClient client = StubHttpMessageHandler.Client(_ => throw new HttpRequestException("down"));
            GitHubReleaseClient releaseClient = new(client);

            Assert.Empty(await releaseClient.GetReleasesAsync());
        }

        [Fact]
        public async Task GetReleasesAsync_ReturnsEmpty_OnMalformedJson()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, "not-json");
            GitHubReleaseClient releaseClient = new(client);

            Assert.Empty(await releaseClient.GetReleasesAsync());
        }
    }
}
