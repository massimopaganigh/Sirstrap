namespace Sirstrap.Core.Tests.Settings
{
    public class SettingsRegistryTests
    {
        private static SettingsRegistry NewRegistry(SirstrapConfiguration config, out RecordingPerformanceTelemetry telemetry)
        {
            telemetry = new RecordingPerformanceTelemetry();

            return new SettingsRegistry(config, new CdnUriNormalizer(), telemetry);
        }

        private static SettingDefinition Find(SettingsRegistry registry, string key) => registry.Settings.First(s => s.Key == key);

        private static void Apply(SettingDefinition definition, string rawValue) => definition.Setter(definition.ValueMigrator?.Invoke(rawValue) ?? rawValue);

        [Fact]
        public void Settings_ExposeAllExpectedKeys()
        {
            SettingsRegistry registry = NewRegistry(new SirstrapConfiguration(), out _);

            string[] keys = [.. registry.Settings.Select(s => s.Key)];

            Assert.Contains("ROBLOX_CDN_URI_OVERRIDE", keys);
            Assert.Contains("ROBLOX_FAST_FLAGS_ENABLED", keys);
            Assert.Contains("ROBLOX_INCOGNITO", keys);
            Assert.Contains("ROBLOX_INSTALLATION_PATH", keys);
            Assert.Contains("ROBLOX_MULTI_INSTANCE", keys);
            Assert.Contains("ROBLOX_PREVIOUS_INSTALLATION_PATH", keys);
            Assert.Contains("ROBLOX_VERSION_SOURCE", keys);
            Assert.Contains("SIRSTRAP_AUTO_UPDATE", keys);
            Assert.Contains("SIRSTRAP_CHANNEL", keys);
            Assert.Contains("SIRSTRAP_FONT_FAMILY", keys);
            Assert.Contains("SIRSTRAP_TELEMETRY", keys);
            Assert.Contains("SIRSTRAP_TRAY_MODE", keys);
        }

        [Fact]
        public void Settings_IsCached()
        {
            SettingsRegistry registry = NewRegistry(new SirstrapConfiguration(), out _);

            Assert.Same(registry.Settings, registry.Settings);
        }

        [Fact]
        public void PreviousInstallationPath_LivesInStateSection()
        {
            SettingsRegistry registry = NewRegistry(new SirstrapConfiguration(), out _);

            Assert.Equal(SettingsSection.State, Find(registry, "ROBLOX_PREVIOUS_INSTALLATION_PATH").Section);
        }

        [Fact]
        public void AutoUpdate_ReadWrite_RoundTrips()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);
            SettingDefinition setting = Find(registry, "SIRSTRAP_AUTO_UPDATE");

            Apply(setting, "False");
            Assert.False(config.SirstrapAutoUpdate);
            Assert.Equal("False", setting.Getter());

            Apply(setting, "not-a-bool");
            Assert.False(config.SirstrapAutoUpdate);
        }

        [Fact]
        public void FontFamily_MapsMinecraftToJetBrainsMono()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);
            SettingDefinition setting = Find(registry, "SIRSTRAP_FONT_FAMILY");

            Apply(setting, "Minecraft");
            Assert.Equal("JetBrains Mono", config.SirstrapFontFamily);

            Apply(setting, "Consolas");
            Assert.Equal("Consolas", config.SirstrapFontFamily);
        }

        [Fact]
        public void InstallationPath_EmptyResetsToDefault()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);
            SettingDefinition setting = Find(registry, "ROBLOX_INSTALLATION_PATH");

            Apply(setting, "   ");
            Assert.Equal(SirstrapConfiguration.GetDefaultInstallationPath(), config.RobloxInstallationPath);

            Apply(setting, @"C:\Custom");
            Assert.Equal(@"C:\Custom", config.RobloxInstallationPath);
        }

        [Fact]
        public void CdnOverride_IsNormalizedOnWrite()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);
            SettingDefinition setting = Find(registry, "ROBLOX_CDN_URI_OVERRIDE");

            Apply(setting, "  https://setup-aws.rbxcdn.com///  ");
            Assert.Equal("https://setup-aws.rbxcdn.com", config.RobloxCdnUriOverride);
        }

        [Fact]
        public void CdnOverride_MapsDefaultUriToEmpty()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);
            SettingDefinition setting = Find(registry, "ROBLOX_CDN_URI_OVERRIDE");

            Apply(setting, RobloxCdnService.DefaultBaseUri);
            Assert.Equal(string.Empty, config.RobloxCdnUriOverride);
        }

        [Fact]
        public void CdnOverride_ExposesLegacyAliases()
        {
            SettingsRegistry registry = NewRegistry(new SirstrapConfiguration(), out _);

            IReadOnlyList<string> legacyKeys = Find(registry, "ROBLOX_CDN_URI_OVERRIDE").LegacyKeys;

            Assert.Contains("ROBLOX_CND_URI", legacyKeys);
            Assert.Contains("ROBLOX_CDN_URI", legacyKeys);
        }

        [Fact]
        public void VersionSource_MigratesLegacyRobloxApiBool()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);
            SettingDefinition setting = Find(registry, "ROBLOX_VERSION_SOURCE");

            Assert.Contains("ROBLOX_API", setting.LegacyKeys);

            Apply(setting, "True");
            Assert.Equal(RobloxVersionSources.Roblox, config.RobloxVersionSource);

            Apply(setting, "False");
            Assert.Equal(RobloxVersionSources.SirHurt, config.RobloxVersionSource);

            Apply(setting, RobloxVersionSources.ExecutorPrefix + "Wave");
            Assert.Equal(RobloxVersionSources.ExecutorPrefix + "Wave", config.RobloxVersionSource);
        }

        [Fact]
        public void TrayMode_ParsesEnum()
        {
            SirstrapConfiguration config = new();
            SettingsRegistry registry = NewRegistry(config, out _);
            SettingDefinition setting = Find(registry, "SIRSTRAP_TRAY_MODE");

            Apply(setting, "onroblox");
            Assert.Equal(TrayMode.OnRoblox, config.SirstrapTrayMode);

            Apply(setting, "garbage");
            Assert.Equal(TrayMode.OnRoblox, config.SirstrapTrayMode);
        }

        [Fact]
        public void MetricEmitter_RecordsCounter_ForSettingWithMetric()
        {
            SettingsRegistry registry = NewRegistry(new SirstrapConfiguration(), out var telemetry);

            Find(registry, "SIRSTRAP_AUTO_UPDATE").MetricEmitter!();

            Assert.Contains(telemetry.Counters, c => c.Name == "settings.SirstrapAutoUpdate");
        }

        [Fact]
        public void TelemetrySetting_HasNoMetricEmitter()
        {
            SettingsRegistry registry = NewRegistry(new SirstrapConfiguration(), out _);

            Assert.Null(Find(registry, "SIRSTRAP_TELEMETRY").MetricEmitter);
        }
    }
}
