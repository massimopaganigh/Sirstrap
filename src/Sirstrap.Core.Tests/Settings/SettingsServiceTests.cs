namespace Sirstrap.Core.Tests.Settings
{
    public class SettingsServiceTests
    {
        private static (SettingsService Service, SirstrapConfiguration Configuration, RecordingPerformanceTelemetry Telemetry) NewService()
        {
            SirstrapConfiguration configuration = new();
            RecordingPerformanceTelemetry telemetry = new();
            SettingsRegistry registry = new(configuration, new CdnUriNormalizer(), telemetry);

            return (new SettingsService(registry), configuration, telemetry);
        }

        [Fact]
        public void LoadSettings_CreatesFile_WhenMissing()
        {
            using TempDirectory temp = new();
            string path = temp.Combine("Sirstrap.ini");
            var (service, _, _) = NewService();

            service.LoadSettings(path);

            Assert.True(File.Exists(path));
            string content = File.ReadAllText(path);
            Assert.Contains("[SETTINGS]", content);
            Assert.Contains("ROBLOX_CDN_URI_OVERRIDE=", content);
        }

        [Fact]
        public void LoadSettings_AppliesValuesFromFile()
        {
            using TempDirectory temp = new();
            string path = temp.WriteFile("Sirstrap.ini", "[SETTINGS]\nAUTO_UPDATE=False\nCHANNEL_NAME=zlive\n");
            var (service, config, _) = NewService();

            service.LoadSettings(path);

            Assert.False(config.AutoUpdate);
            Assert.Equal("zlive", config.ChannelName);
        }

        [Fact]
        public void LoadSettings_MigratesLegacyDefaultCdnUri_ToEmptyOverride()
        {
            using TempDirectory temp = new();
            string path = temp.WriteFile("Sirstrap.ini", "[SETTINGS]\nROBLOX_CND_URI=https://setup.rbxcdn.com\n");
            var (service, config, _) = NewService();
            config.RobloxCdnUriOverride = "https://custom.example.com";

            service.LoadSettings(path);

            Assert.Equal(string.Empty, config.RobloxCdnUriOverride);
        }

        [Fact]
        public void LoadSettings_MigratesLegacyCustomCdnUri_ToOverride()
        {
            using TempDirectory temp = new();
            string path = temp.WriteFile("Sirstrap.ini", "[SETTINGS]\nROBLOX_CDN_URI=https://setup-aws.rbxcdn.com\n");
            var (service, config, _) = NewService();

            service.LoadSettings(path);

            Assert.Equal("https://setup-aws.rbxcdn.com", config.RobloxCdnUriOverride);
        }

        [Fact]
        public void LoadSettings_DoesNotMigrate_WhenTargetKeyPresent()
        {
            using TempDirectory temp = new();
            string path = temp.WriteFile("Sirstrap.ini", "[SETTINGS]\nROBLOX_CND_URI=https://legacy.example.com\nROBLOX_CDN_URI_OVERRIDE=https://kept.example.com\n");
            var (service, config, _) = NewService();

            service.LoadSettings(path);

            Assert.Equal("https://kept.example.com", config.RobloxCdnUriOverride);
        }

        [Fact]
        public void LoadSettings_DoesNotThrow_OnUnreadableFile()
        {
            var (service, _, _) = NewService();

            Assert.Null(Record.Exception(() => service.LoadSettings(Path.Combine("Z:\\does", "not", "exist", "file.ini"))));
        }

        [Fact]
        public void SaveSettings_RemovesLegacyKeys_AndKeepsOverride()
        {
            using TempDirectory temp = new();
            string path = temp.WriteFile("Sirstrap.ini", "[SETTINGS]\nROBLOX_CND_URI=https://a.example.com\nROBLOX_CDN_URI=https://b.example.com\n");
            var (service, _, _) = NewService();

            service.SaveSettings(path);

            string content = File.ReadAllText(path);
            Assert.DoesNotContain("ROBLOX_CND_URI=", content);
            Assert.DoesNotContain("ROBLOX_CDN_URI=", content);
            Assert.Contains("ROBLOX_CDN_URI_OVERRIDE=", content);
        }

        [Fact]
        public void SaveSettings_AddsMissingSettingsSection_WhenAbsent()
        {
            using TempDirectory temp = new();
            string path = temp.WriteFile("Sirstrap.ini", "; just a comment\n");
            var (service, _, _) = NewService();

            service.SaveSettings(path);

            string content = File.ReadAllText(path);
            Assert.Contains("[SETTINGS]", content);
            Assert.Contains("AUTO_UPDATE=", content);
        }

        [Fact]
        public void SaveSettings_AppendsMissingKeys_ToExistingSection()
        {
            using TempDirectory temp = new();
            string path = temp.WriteFile("Sirstrap.ini", "[SETTINGS]\nAUTO_UPDATE=True\n");
            var (service, _, _) = NewService();

            service.SaveSettings(path);

            string content = File.ReadAllText(path);
            Assert.Contains("CHANNEL_NAME=", content);
            Assert.Contains("TRAY_MODE=", content);
        }

        [Fact]
        public void SaveSettings_InsertsMissingKeys_WhenSettingsSectionFollowedByAnotherSection()
        {
            using TempDirectory temp = new();
            string path = temp.WriteFile("Sirstrap.ini", "[SETTINGS]\nAUTO_UPDATE=True\n[OTHER]\nFOO=1\n");
            var (service, _, _) = NewService();

            service.SaveSettings(path);

            string[] lines = File.ReadAllLines(path);
            int settingsIndex = Array.FindIndex(lines, l => l.Trim() == "[SETTINGS]");
            int otherIndex = Array.FindIndex(lines, l => l.Trim() == "[OTHER]");

            Assert.True(settingsIndex >= 0 && otherIndex > settingsIndex);
            Assert.Contains(lines, l => l.StartsWith("CHANNEL_NAME="));
            Assert.Contains("FOO=1", lines);
        }

        [Fact]
        public void SaveSettings_RemovesLegacyKey_InsideSettingsSection()
        {
            using TempDirectory temp = new();
            string path = temp.WriteFile("Sirstrap.ini", "[SETTINGS]\nROBLOX_CND_URI=https://legacy.example.com\nAUTO_UPDATE=True\n");
            var (service, _, _) = NewService();

            service.SaveSettings(path);

            Assert.DoesNotContain("ROBLOX_CND_URI", File.ReadAllText(path));
        }

        [Fact]
        public void EmitSettingsMetrics_RecordsCountersForSettingsWithMetrics()
        {
            var (service, _, telemetry) = NewService();

            service.EmitSettingsMetrics();

            Assert.Contains(telemetry.Counters, c => c.Name == "settings.AutoUpdate");
            Assert.Contains(telemetry.Counters, c => c.Name == "settings.TrayMode");
        }
    }
}
