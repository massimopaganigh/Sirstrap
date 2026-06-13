namespace Sirstrap.Core.Tests.Update
{
    public class GitHubReleaseTests
    {
        private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

        [Fact]
        public void FromJson_ParsesAllFields()
        {
            GitHubRelease release = GitHubRelease.FromJson(Parse("""
            {
                "tag_name": "v1.0.0.0-beta",
                "draft": true,
                "body": "changelog",
                "assets": [
                    { "name": "Sirstrap.CLI.zip", "browser_download_url": "https://example.com/cli.zip" },
                    { "name": "Sirstrap.UI.zip", "browser_download_url": "https://example.com/ui.zip" }
                ]
            }
            """));

            Assert.Equal("v1.0.0.0-beta", release.TagName);
            Assert.True(release.IsDraft);
            Assert.Equal("changelog", release.Body);
            Assert.Equal(2, release.Assets.Count);
        }

        [Fact]
        public void FromJson_UsesDefaults_WhenFieldsMissing()
        {
            GitHubRelease release = GitHubRelease.FromJson(Parse("{}"));

            Assert.Equal(string.Empty, release.TagName);
            Assert.False(release.IsDraft);
            Assert.Equal(string.Empty, release.Body);
            Assert.Empty(release.Assets);
        }

        [Fact]
        public void FromJson_SkipsAssetsWithoutNames()
        {
            GitHubRelease release = GitHubRelease.FromJson(Parse("""
            { "assets": [ { "browser_download_url": "https://x" }, { "name": "", "browser_download_url": "https://y" }, { "name": "ok.zip" } ] }
            """));

            Assert.Equal("ok.zip", Assert.Single(release.Assets).Name);
            Assert.Equal(string.Empty, release.Assets[0].DownloadUri);
        }

        [Fact]
        public void FindAssetDownloadUri_IsCaseInsensitive_AndReturnsEmptyWhenMissing()
        {
            GitHubRelease release = GitHubRelease.FromJson(Parse("""
            { "assets": [ { "name": "Sirstrap.CLI.zip", "browser_download_url": "https://example.com/cli.zip" } ] }
            """));

            Assert.Equal("https://example.com/cli.zip", release.FindAssetDownloadUri("sirstrap.cli.zip"));
            Assert.Equal(string.Empty, release.FindAssetDownloadUri("missing.zip"));
        }
    }
}
