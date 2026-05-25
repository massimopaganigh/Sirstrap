namespace Sirstrap.Core.Tests
{
    public class SirstrapConfigurationServiceTests
    {
        [Fact]
        public void LoadSettings_MigratesLegacyDefaultCdnUri_ToEmptyOverride()
        {
            string settingsFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.ini");
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                File.WriteAllText(settingsFilePath, "[SETTINGS]\nROBLOX_CND_URI=https://setup.rbxcdn.com\n");

                SirstrapConfiguration.RobloxCdnUriOverride = "https://custom.example.com";

                SirstrapConfigurationService.LoadSettings(settingsFilePath);

                Assert.Equal(string.Empty, SirstrapConfiguration.RobloxCdnUriOverride);
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;

                if (File.Exists(settingsFilePath))
                    File.Delete(settingsFilePath);
            }
        }

        [Fact]
        public void LoadSettings_MigratesLegacyCustomCdnUri_ToOverride()
        {
            string settingsFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.ini");
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                File.WriteAllText(settingsFilePath, "[SETTINGS]\nROBLOX_CDN_URI=https://setup-aws.rbxcdn.com\n");

                SirstrapConfiguration.RobloxCdnUriOverride = string.Empty;

                SirstrapConfigurationService.LoadSettings(settingsFilePath);

                Assert.Equal("https://setup-aws.rbxcdn.com", SirstrapConfiguration.RobloxCdnUriOverride);
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;

                if (File.Exists(settingsFilePath))
                    File.Delete(settingsFilePath);
            }
        }

        [Fact]
        public void LoadSettings_DoesNotMigrateLegacyKey_WhenOverrideKeyAlreadyPresent()
        {
            string settingsFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.ini");
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                File.WriteAllText(settingsFilePath, "[SETTINGS]\nROBLOX_CND_URI=https://setup-ak.rbxcdn.com\nROBLOX_CDN_URI_OVERRIDE=https://setup-aws.rbxcdn.com\n");

                SirstrapConfigurationService.LoadSettings(settingsFilePath);

                Assert.Equal("https://setup-aws.rbxcdn.com", SirstrapConfiguration.RobloxCdnUriOverride);
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;

                if (File.Exists(settingsFilePath))
                    File.Delete(settingsFilePath);
            }
        }

        [Fact]
        public void SaveSettings_RemovesLegacyCdnUriKeys_AndWritesOverrideKey()
        {
            string settingsFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.ini");
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                File.WriteAllText(settingsFilePath, "[SETTINGS]\nROBLOX_CND_URI=https://setup.rbxcdn.com\n");

                SirstrapConfiguration.RobloxCdnUriOverride = string.Empty;

                SirstrapConfigurationService.SaveSettings(settingsFilePath);

                string savedSettings = File.ReadAllText(settingsFilePath);

                Assert.DoesNotContain("ROBLOX_CND_URI", savedSettings);
                Assert.Contains("ROBLOX_CDN_URI_OVERRIDE=", savedSettings);
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;

                if (File.Exists(settingsFilePath))
                    File.Delete(settingsFilePath);
            }
        }

        [Fact]
        public void SaveSettings_RemovesBothLegacyKeys_WhenBothPresent()
        {
            string settingsFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.ini");
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                File.WriteAllText(settingsFilePath, "[SETTINGS]\nROBLOX_CND_URI=https://a.example.com\nROBLOX_CDN_URI=https://b.example.com\n");

                SirstrapConfiguration.RobloxCdnUriOverride = string.Empty;

                SirstrapConfigurationService.SaveSettings(settingsFilePath);

                string savedSettings = File.ReadAllText(settingsFilePath);

                Assert.DoesNotContain("ROBLOX_CND_URI=", savedSettings);
                Assert.DoesNotContain("ROBLOX_CDN_URI=", savedSettings);
                Assert.Contains("ROBLOX_CDN_URI_OVERRIDE=", savedSettings);
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;

                if (File.Exists(settingsFilePath))
                    File.Delete(settingsFilePath);
            }
        }
    }
}
