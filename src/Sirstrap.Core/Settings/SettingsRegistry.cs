namespace Sirstrap.Core.Settings
{
    public sealed class SettingsRegistry : ISettingsRegistry
    {
        private readonly Lazy<IReadOnlyList<ISettingMigration>> _migrations;
        private readonly Lazy<IReadOnlyList<ISetting>> _settings;

        public SettingsRegistry(SirstrapConfiguration configuration, ICdnUriNormalizer cdnUriNormalizer, IPerformanceTelemetry performanceTelemetry)
        {
            _settings = new(() => BuildSettings(configuration, cdnUriNormalizer, performanceTelemetry));
            _migrations = new(() => BuildMigrations(configuration, cdnUriNormalizer));
        }

        public IReadOnlyList<ISettingMigration> Migrations => _migrations.Value;

        public IReadOnlyList<ISetting> Settings => _settings.Value;

        private static IReadOnlyList<ISettingMigration> BuildMigrations(SirstrapConfiguration configuration, ICdnUriNormalizer cdnUriNormalizer)
        {
            void MigrateCdnUriOverride(string legacyValue)
            {
                string normalized = cdnUriNormalizer.Normalize(legacyValue);

                if (normalized.Equals(RobloxCdnService.DefaultBaseUri, StringComparison.OrdinalIgnoreCase))
                    normalized = string.Empty;

                configuration.RobloxCdnUriOverride = normalized;
            }

            return
            [
                new SettingMigration("ROBLOX_CND_URI", "ROBLOX_CDN_URI_OVERRIDE", MigrateCdnUriOverride),
                new SettingMigration("ROBLOX_CDN_URI", "ROBLOX_CDN_URI_OVERRIDE", MigrateCdnUriOverride)
            ];
        }

        private static IReadOnlyList<ISetting> BuildSettings(SirstrapConfiguration configuration, ICdnUriNormalizer cdnUriNormalizer, IPerformanceTelemetry performanceTelemetry)
        {
            Action Metric(string name, Func<object> getValue)
                => () => performanceTelemetry.RecordCounter($"settings.{name}", new Dictionary<string, object> { ["value"] = getValue() });

            return
            [
                new Setting("AUTO_UPDATE",
                    () => configuration.AutoUpdate.ToString(),
                    value => { if (bool.TryParse(value, out var v)) configuration.AutoUpdate = v; },
                    Metric("AutoUpdate", () => configuration.AutoUpdate)),
                new Setting("CHANNEL_NAME",
                    () => configuration.ChannelName,
                    value => configuration.ChannelName = value,
                    Metric("ChannelName", () => configuration.ChannelName)),
                new Setting("FONT_FAMILY",
                    () => configuration.FontFamily,
                    value => configuration.FontFamily = value.Equals("Minecraft", StringComparison.OrdinalIgnoreCase) ? "JetBrains Mono" : value,
                    Metric("FontFamily", () => configuration.FontFamily)),
                new Setting("INCOGNITO",
                    () => configuration.Incognito.ToString(),
                    value => { if (bool.TryParse(value, out var v)) configuration.Incognito = v; },
                    Metric("Incognito", () => configuration.Incognito)),
                new Setting("INSTALLATION_PATH",
                    () => configuration.InstallationPath,
                    value => configuration.InstallationPath = string.IsNullOrWhiteSpace(value) ? SirstrapConfiguration.GetDefaultInstallationPath() : value),
                new Setting("MULTI_INSTANCE",
                    () => configuration.MultiInstance.ToString(),
                    value => { if (bool.TryParse(value, out var v)) configuration.MultiInstance = v; },
                    Metric("MultiInstance", () => configuration.MultiInstance)),
                new Setting("PREVIOUS_INSTALLATION_PATH",
                    () => configuration.PreviousInstallationPath,
                    value => configuration.PreviousInstallationPath = value),
                new Setting("ROBLOX_API",
                    () => configuration.RobloxApi.ToString(),
                    value => { if (bool.TryParse(value, out var v)) configuration.RobloxApi = v; },
                    Metric("RobloxApi", () => configuration.RobloxApi)),
                new Setting("ROBLOX_CDN_URI_OVERRIDE",
                    () => configuration.RobloxCdnUriOverride,
                    value => configuration.RobloxCdnUriOverride = cdnUriNormalizer.Normalize(value),
                    Metric("RobloxCdnUriOverride", () => string.IsNullOrEmpty(configuration.RobloxCdnUriOverride) ? "Auto" : "Custom")),
                new Setting("ROBLOX_VERSION_OVERRIDE",
                    () => configuration.RobloxVersionOverride,
                    value => configuration.RobloxVersionOverride = value,
                    Metric("RobloxVersionOverride", () => string.IsNullOrEmpty(configuration.RobloxVersionOverride) ? "None" : "Custom")),
                new Setting("TELEMETRY",
                    () => configuration.Telemetry.ToString(),
                    value => { if (bool.TryParse(value, out var v)) configuration.Telemetry = v; }),
                new Setting("TRAY_MODE",
                    () => configuration.TrayMode.ToString(),
                    value => { if (Enum.TryParse<TrayMode>(value, true, out var v)) configuration.TrayMode = v; },
                    Metric("TrayMode", () => configuration.TrayMode.ToString()))
            ];
        }
    }
}
