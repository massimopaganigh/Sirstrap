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
        public void NormalizeCdnUriOverride_PreservesDefaultBaseUri_AsIs()
        {
            string normalized = RobloxCdnService.NormalizeCdnUriOverride(RobloxCdnService.DefaultBaseUri);

            Assert.Equal(RobloxCdnService.DefaultBaseUri, normalized);
        }

        [Theory]
        [InlineData("ftp://setup.rbxcdn.com")]
        [InlineData("file:///etc/passwd")]
        [InlineData("javascript:alert(1)")]
        public void NormalizeCdnUriOverride_RejectsNonHttpSchemes(string input)
        {
            Assert.Equal(string.Empty, RobloxCdnService.NormalizeCdnUriOverride(input));
        }

        [Fact]
        public void NormalizeCdnUriOverride_StripsQueryAndFragment()
        {
            string normalized = RobloxCdnService.NormalizeCdnUriOverride("https://setup-aws.rbxcdn.com/path?a=1#frag");

            Assert.Equal("https://setup-aws.rbxcdn.com/path", normalized);
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

        [Fact]
        public void GetManifestUri_FallsBackToDefault_WhenOverrideInvalid()
        {
            Configuration configuration = new()
            {
                ChannelName = "LIVE",
                BlobDirectory = "/",
                VersionHash = "version-hash"
            };

            string manifestUri = UriBuilder.GetManifestUri(configuration, "not-a-valid-uri");

            Assert.Equal($"{RobloxCdnService.DefaultBaseUri}/version-hash-rbxPkgManifest.txt", manifestUri);
        }
    }
}
