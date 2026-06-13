namespace Sirstrap.Core.Tests.Deployment
{
    public class ConfigurationServiceTests
    {
        [Fact]
        public void ParseConfiguration_Throws_OnNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => ConfigurationService.ParseConfiguration(null!));
        }

        [Fact]
        public void ParseConfiguration_ParsesOptionsAndLaunchUri()
        {
            var parsed = ConfigurationService.ParseConfiguration(["roblox-player://launch", "--channel-name", "ztest", "--binary-type", "WindowsPlayer"]);

            Assert.Equal("roblox-player://launch", parsed["launch-uri"]);
            Assert.Equal("ztest", parsed["channel-name"]);
            Assert.Equal("WindowsPlayer", parsed["binary-type"]);
        }

        [Fact]
        public void ParseConfiguration_IgnoresOptionWithoutValue()
        {
            var parsed = ConfigurationService.ParseConfiguration(["--channel-name", "--binary-type", "WindowsStudio64"]);

            Assert.False(parsed.ContainsKey("channel-name"));
            Assert.Equal("WindowsStudio64", parsed["binary-type"]);
        }

        [Fact]
        public void ParseConfiguration_DoesNotTreatLaterPositionalAsLaunchUri()
        {
            var parsed = ConfigurationService.ParseConfiguration(["--channel-name", "zlive", "trailing"]);

            Assert.Equal("zlive", parsed["channel-name"]);
            Assert.False(parsed.ContainsKey("launch-uri"));
        }

        [Fact]
        public void CreateConfigurationFromArguments_UsesDefaults_WhenEmpty()
        {
            Configuration configuration = ConfigurationService.CreateConfigurationFromArguments([]);

            Assert.Equal("WindowsPlayer", configuration.BinaryType);
            Assert.Equal("LIVE", configuration.ChannelName);
            Assert.Equal("/", configuration.BlobDirectory);
            Assert.Equal(string.Empty, configuration.VersionHash);
            Assert.Equal(string.Empty, configuration.LaunchUri);
        }

        [Fact]
        public void CreateConfigurationFromArguments_UsesMacBlobDirectory_ForMacBinary()
        {
            Configuration configuration = ConfigurationService.CreateConfigurationFromArguments(new(StringComparer.OrdinalIgnoreCase)
            {
                ["binary-type"] = "MacPlayer"
            });

            Assert.Equal("/mac/", configuration.BlobDirectory);
        }

        [Theory]
        [InlineData("custom", "/custom/")]
        [InlineData("/custom", "/custom/")]
        [InlineData("custom/", "/custom/")]
        [InlineData("/custom/", "/custom/")]
        public void CreateConfigurationFromArguments_NormalizesBlobDirectory(string input, string expected)
        {
            Configuration configuration = ConfigurationService.CreateConfigurationFromArguments(new(StringComparer.OrdinalIgnoreCase)
            {
                ["blob-directory"] = input
            });

            Assert.Equal(expected, configuration.BlobDirectory);
        }

        [Fact]
        public void CreateConfigurationFromArguments_Throws_OnUnsupportedBinaryType()
        {
            Assert.Throws<ArgumentException>(() => ConfigurationService.CreateConfigurationFromArguments(new(StringComparer.OrdinalIgnoreCase)
            {
                ["binary-type"] = "PlayStation"
            }));
        }
    }
}
