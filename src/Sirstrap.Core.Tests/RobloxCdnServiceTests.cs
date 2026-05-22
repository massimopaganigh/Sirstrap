namespace Sirstrap.Core.Tests
{
    public class RobloxCdnServiceTests
    {
        [Fact]
        public void NormalizeCdnUriOverride_ReturnsEmpty_ForBlankOrInvalidValues()
        {
            Assert.Equal(string.Empty, RobloxCdnService.NormalizeCdnUriOverride(null));
            Assert.Equal(string.Empty, RobloxCdnService.NormalizeCdnUriOverride("   "));
            Assert.Equal(string.Empty, RobloxCdnService.NormalizeCdnUriOverride("setup.rbxcdn.com"));
        }

        [Fact]
        public void NormalizeCdnUriOverride_TrimsWhitespaceAndTrailingSlashes()
        {
            string normalized = RobloxCdnService.NormalizeCdnUriOverride("  https://setup-ak.rbxcdn.com///  ");

            Assert.Equal("https://setup-ak.rbxcdn.com", normalized);
        }

        [Fact]
        public void GetManifestUri_UsesProvidedCdnUri()
        {
            Configuration configuration = new()
            {
                ChannelName = "LIVE",
                BlobDirectory = "/",
                VersionHash = "version-hash"
            };

            string manifestUri = UriBuilder.GetManifestUri(configuration, " https://setup-aws.rbxcdn.com/ ");

            Assert.Equal("https://setup-aws.rbxcdn.com/version-hash-rbxPkgManifest.txt", manifestUri);
        }
    }
}
