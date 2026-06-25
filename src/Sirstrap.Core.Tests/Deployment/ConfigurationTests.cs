namespace Sirstrap.Core.Tests.Deployment
{
    public class ConfigurationTests
    {
        [Fact]
        public void Defaults_AreWindowsPlayerLive()
        {
            Configuration configuration = new();

            Assert.Equal("WindowsPlayer", configuration.BinaryType);
            Assert.Equal("/", configuration.BlobDirectory);
            Assert.Equal("LIVE", configuration.ChannelName);
            Assert.Equal(string.Empty, configuration.LaunchUri);
            Assert.Equal(string.Empty, configuration.VersionHash);
            Assert.True(configuration.IsWindowsPlayer());
            Assert.False(configuration.IsMacBinary());
            Assert.False(configuration.IsMacPlayer());
        }

        [Theory]
        [InlineData("MacPlayer", true, true)]
        [InlineData("MacStudio", true, false)]
        [InlineData("WindowsPlayer", false, false)]
        [InlineData("WindowsStudio64", false, false)]
        public void BinaryTypePredicates_ReflectBinaryType(string binaryType, bool isMac, bool isMacPlayer)
        {
            Configuration configuration = new() { BinaryType = binaryType };

            Assert.Equal(isMac, configuration.IsMacBinary());
            Assert.Equal(isMacPlayer, configuration.IsMacPlayer());
        }

        [Fact]
        public void IsWindowsPlayer_IsCaseInsensitive()
        {
            Assert.True(new Configuration { BinaryType = "windowsplayer" }.IsWindowsPlayer());
        }
    }
}
