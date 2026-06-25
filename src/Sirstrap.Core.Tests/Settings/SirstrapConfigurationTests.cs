namespace Sirstrap.Core.Tests.Settings
{
    public class SirstrapConfigurationTests
    {
        [Fact]
        public void Defaults_AreApplied()
        {
            SirstrapConfiguration config = new();

            Assert.True(config.AutoUpdate);
            Assert.Equal("-beta", config.ChannelName);
            Assert.Equal("JetBrains Mono", config.FontFamily);
            Assert.False(config.Incognito);
            Assert.True(config.MultiInstance);
            Assert.False(config.RobloxApi);
            Assert.Equal(RobloxCdnService.DefaultBaseUri, config.ResolvedRobloxCdnUri);
            Assert.Equal(string.Empty, config.RobloxCdnUriOverride);
            Assert.Equal(string.Empty, config.RobloxVersionOverride);
            Assert.True(config.Telemetry);
            Assert.Equal(TrayMode.None, config.TrayMode);
            Assert.Equal(string.Empty, config.PreviousInstallationPath);
        }

        [Fact]
        public void GetDefaultInstallationPath_PointsUnderLocalAppDataSirstrap()
        {
            string path = SirstrapConfiguration.GetDefaultInstallationPath();

            Assert.EndsWith(Path.Combine("Sirstrap", "Versions"), path);
            Assert.Equal(path, new SirstrapConfiguration().InstallationPath);
        }

        [Fact]
        public void Properties_AreMutable()
        {
            SirstrapConfiguration config = new()
            {
                AutoUpdate = false,
                ChannelName = "zlive",
                Incognito = true,
                TrayMode = TrayMode.OnRoblox
            };

            Assert.False(config.AutoUpdate);
            Assert.Equal("zlive", config.ChannelName);
            Assert.True(config.Incognito);
            Assert.Equal(TrayMode.OnRoblox, config.TrayMode);
        }

        [Fact]
        public void TrayMode_HasExpectedMembers()
        {
            Assert.Equal(["None", "OnLaunch", "OnRoblox"], Enum.GetNames<TrayMode>());
        }
    }
}
