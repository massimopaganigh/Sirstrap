namespace Sirstrap.Core.Tests.Cdn
{
    public class CdnProbeUriFactoryTests
    {
        private static Configuration NewConfiguration(string binaryType) => new()
        {
            BinaryType = binaryType,
            ChannelName = "LIVE",
            BlobDirectory = "/",
            VersionHash = "v1"
        };

        [Fact]
        public void Create_UsesManifestUri_ForWindowsBinary()
        {
            FakeRobloxUriFactory uriFactory = new();
            CdnProbeUriFactory factory = new(uriFactory);

            string result = factory.Create(NewConfiguration("WindowsPlayer"), "https://cdn.example.com");

            Assert.StartsWith("manifest:https://cdn.example.com", result);
        }

        [Fact]
        public void Create_UsesMacPlayerPackage_ForMacPlayerBinary()
        {
            FakeRobloxUriFactory uriFactory = new();
            CdnProbeUriFactory factory = new(uriFactory);

            string result = factory.Create(NewConfiguration("MacPlayer"), "https://cdn.example.com");

            Assert.Equal("package:https://cdn.example.com:RobloxPlayer.zip", result);
        }

        [Fact]
        public void Create_UsesStudioPackage_ForMacStudioBinary()
        {
            FakeRobloxUriFactory uriFactory = new();
            CdnProbeUriFactory factory = new(uriFactory);

            string result = factory.Create(NewConfiguration("MacStudio"), "https://cdn.example.com");

            Assert.Equal("package:https://cdn.example.com:RobloxStudioApp.zip", result);
        }

        [Fact]
        public void Create_Throws_WhenConfigurationNull()
        {
            CdnProbeUriFactory factory = new(new FakeRobloxUriFactory());

            Assert.Throws<ArgumentNullException>(() => factory.Create(null!, "https://cdn.example.com"));
        }
    }
}
