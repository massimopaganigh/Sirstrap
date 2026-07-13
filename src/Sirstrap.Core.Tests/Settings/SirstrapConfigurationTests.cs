namespace Sirstrap.Core.Tests.Settings
{
    public class SirstrapConfigurationTests
    {
        [Fact]
        public void Defaults_AreApplied()
        {
            SirstrapConfiguration config = new();

            Assert.True(config.SirstrapAutoUpdate);
            Assert.Equal("-beta", config.SirstrapChannel);
            Assert.Equal("JetBrains Mono", config.SirstrapFontFamily);
            Assert.True(config.RobloxFastFlagsEnabled);
            Assert.False(config.RobloxIncognito);
            Assert.True(config.RobloxMultiInstance);
            Assert.Equal(RobloxVersionSources.SirHurt, config.RobloxVersionSource);
            Assert.Equal(RobloxCdnService.DefaultBaseUri, config.ResolvedRobloxCdnUri);
            Assert.Equal(string.Empty, config.RobloxCdnUriOverride);
            Assert.True(config.SirstrapTelemetry);
            Assert.Equal(TrayMode.None, config.SirstrapTrayMode);
            Assert.Equal(string.Empty, config.RobloxPreviousInstallationPath);
        }

        [Fact]
        public void GetDefaultInstallationPath_PointsUnderLocalAppDataSirstrap()
        {
            string path = SirstrapConfiguration.GetDefaultInstallationPath();

            Assert.EndsWith(Path.Combine("Sirstrap", "Versions"), path);
            Assert.Equal(path, new SirstrapConfiguration().RobloxInstallationPath);
        }

        [Fact]
        public void Properties_AreMutable()
        {
            SirstrapConfiguration config = new()
            {
                SirstrapAutoUpdate = false,
                SirstrapChannel = "zlive",
                RobloxIncognito = true,
                SirstrapTrayMode = TrayMode.OnRoblox
            };

            Assert.False(config.SirstrapAutoUpdate);
            Assert.Equal("zlive", config.SirstrapChannel);
            Assert.True(config.RobloxIncognito);
            Assert.Equal(TrayMode.OnRoblox, config.SirstrapTrayMode);
        }

        [Fact]
        public void TrayMode_HasExpectedMembers()
        {
            Assert.Equal(["None", "OnLaunch", "OnRoblox"], Enum.GetNames<TrayMode>());
        }
    }
}
