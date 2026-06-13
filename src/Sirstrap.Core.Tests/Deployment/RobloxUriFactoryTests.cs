namespace Sirstrap.Core.Tests.Deployment
{
    public class RobloxUriFactoryTests
    {
        private static RobloxUriFactory NewFactory(SirstrapConfiguration? config = null)
            => new(config ?? new SirstrapConfiguration(), new CdnUriNormalizer());

        private static Configuration NewConfiguration(string channel = "LIVE") => new()
        {
            ChannelName = channel,
            BlobDirectory = "/",
            VersionHash = "version-hash"
        };

        [Fact]
        public void GetManifestUri_UsesResolvedCdnUri_ForLiveChannel()
        {
            SirstrapConfiguration config = new() { ResolvedRobloxCdnUri = "https://cdn.example.com" };

            string uri = NewFactory(config).GetManifestUri(NewConfiguration());

            Assert.Equal("https://cdn.example.com/version-hash-rbxPkgManifest.txt", uri);
        }

        [Fact]
        public void GetManifestUri_InsertsChannelSegment_ForNonLiveChannel()
        {
            SirstrapConfiguration config = new() { ResolvedRobloxCdnUri = "https://cdn.example.com" };

            string uri = NewFactory(config).GetManifestUri(NewConfiguration(channel: "zcanary"));

            Assert.Equal("https://cdn.example.com/channel/zcanary/version-hash-rbxPkgManifest.txt", uri);
        }

        [Fact]
        public void GetManifestUri_UsesProvidedCdnUri()
        {
            string uri = NewFactory().GetManifestUri(NewConfiguration(), " https://setup-aws.rbxcdn.com/ ");

            Assert.Equal("https://setup-aws.rbxcdn.com/version-hash-rbxPkgManifest.txt", uri);
        }

        [Fact]
        public void GetManifestUri_FallsBackToDefault_WhenProvidedCdnInvalid()
        {
            string uri = NewFactory().GetManifestUri(NewConfiguration(), "not-a-valid-uri");

            Assert.Equal($"{RobloxCdnService.DefaultBaseUri}/version-hash-rbxPkgManifest.txt", uri);
        }

        [Fact]
        public void GetPackageUri_AppendsPackageName()
        {
            SirstrapConfiguration config = new() { ResolvedRobloxCdnUri = "https://cdn.example.com" };

            string uri = NewFactory(config).GetPackageUri(NewConfiguration(), "RobloxApp.zip");

            Assert.Equal("https://cdn.example.com/version-hash-RobloxApp.zip", uri);
        }

        [Fact]
        public void GetPackageUri_WithProvidedCdn_AppendsPackageName()
        {
            string uri = NewFactory().GetPackageUri(NewConfiguration(), "RobloxApp.zip", "https://setup-aws.rbxcdn.com");

            Assert.Equal("https://setup-aws.rbxcdn.com/version-hash-RobloxApp.zip", uri);
        }
    }
}
