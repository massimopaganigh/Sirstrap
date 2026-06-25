namespace Sirstrap.Core.Tests.Settings
{
    public class SettingsRegistryTests
    {
        private static SettingsRegistry NewRegistry(SirstrapConfiguration config, out RecordingPerformanceTelemetry telemetry)
        {
            telemetry = new RecordingPerformanceTelemetry();

            return new SettingsRegistry(config, new CdnUriNormalizer(), telemetry);
        }

        private static ISetting Find(SettingsRegistry registry, string key) => registry.Settings.First(s => s.Key == key);

        [Fact]
        public void Settings_ExposeAllExpectedKeys()
        {
            SettingsRegistry registry = NewRegistry(new SirstrapConfiguration(), out _);

            string[] keys = [.. registry.Settings.Select(s => s.Key)];

            Assert.Contains("AUTO_UPDATE", keys);
            Assert.Contains("CHANNEL_NAME", keys);
            Assert.Contains("FONT_FAMILY", keys);
            Assert.Contains("INCOGNITO", keys);
            Assert.Contains("INSTALLATION_PATH", keys);
            Assert.Contains("MULTI_INSTANCE", keys);
            Assert.Contains("PREVIOUS_INSTALLATION_PATH", keys);
            Assert.Contains("ROBLOX_API", keys);
            Assert.Contains("ROBLOX_CDN_URI_OVERRIDE", keys);
            Assert.Contains("ROBLOX_VERSION_OVERRIDE", keys);
            Assert.Contains("TELEMETRY", keys);
            Assert.Contains("TRAY_MODE", keys);
        }

        [Fact]
        public void Settings_IsCached()
        {
            SettingsRegistry registry = NewRegistry(new SirstrapConfiguration(), out _);

            Assert.Same(registry.Settings, registry.Settings);
            Assert.Same(registry.Migrations, registry.Migrations);
        }

        [Fact]
        public void AutoUpdate_ReadWrite_RoundTrips()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);
            ISetting setting = Find(registry, "AUTO_UPDATE");

            setting.Write("False");
            Assert.False(config.AutoUpdate);
            Assert.Equal("False", setting.Read());

            setting.Write("not-a-bool");
            Assert.False(config.AutoUpdate);
        }

        [Fact]
        public void FontFamily_MapsMinecraftToJetBrainsMono()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);
            ISetting setting = Find(registry, "FONT_FAMILY");

            setting.Write("Minecraft");
            Assert.Equal("JetBrains Mono", config.FontFamily);

            setting.Write("Consolas");
            Assert.Equal("Consolas", config.FontFamily);
        }

        [Fact]
        public void InstallationPath_EmptyResetsToDefault()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);
            ISetting setting = Find(registry, "INSTALLATION_PATH");

            setting.Write("   ");
            Assert.Equal(SirstrapConfiguration.GetDefaultInstallationPath(), config.InstallationPath);

            setting.Write(@"C:\Custom");
            Assert.Equal(@"C:\Custom", config.InstallationPath);
        }

        [Fact]
        public void CdnOverride_IsNormalizedOnWrite()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);
            ISetting setting = Find(registry, "ROBLOX_CDN_URI_OVERRIDE");

            setting.Write("  https://setup-aws.rbxcdn.com///  ");
            Assert.Equal("https://setup-aws.rbxcdn.com", config.RobloxCdnUriOverride);
        }

        [Fact]
        public void TrayMode_ParsesEnum()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);
            ISetting setting = Find(registry, "TRAY_MODE");

            setting.Write("onroblox");
            Assert.Equal(TrayMode.OnRoblox, config.TrayMode);

            setting.Write("garbage");
            Assert.Equal(TrayMode.OnRoblox, config.TrayMode);
        }

        [Fact]
        public void EmitMetric_RecordsCounter_ForSettingWithMetric()
        {
            SettingsRegistry registry = NewRegistry(new SirstrapConfiguration(), out var telemetry);

            Find(registry, "AUTO_UPDATE").EmitMetric();

            Assert.Contains(telemetry.Counters, c => c.Name == "settings.AutoUpdate");
        }

        [Fact]
        public void TelemetrySetting_HasNoMetricEmitter()
        {
            SettingsRegistry registry = NewRegistry(new SirstrapConfiguration(), out var telemetry);

            Find(registry, "TELEMETRY").EmitMetric();

            Assert.DoesNotContain(telemetry.Counters, c => c.Name.StartsWith("settings.Telemetry"));
        }

        [Fact]
        public void Migrations_NormalizeLegacyCdnUri()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);

            ISettingMigration migration = registry.Migrations[0];
            Assert.Equal("ROBLOX_CDN_URI_OVERRIDE", migration.TargetKey);

            migration.Apply("https://setup-aws.rbxcdn.com");
            Assert.Equal("https://setup-aws.rbxcdn.com", config.RobloxCdnUriOverride);
        }

        [Fact]
        public void Migrations_MapDefaultCdnUriToEmptyOverride()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);

            registry.Migrations[0].Apply(RobloxCdnService.DefaultBaseUri);

            Assert.Equal(string.Empty, config.RobloxCdnUriOverride);
        }
    }
}
