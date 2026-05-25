namespace Sirstrap.Core
{
    public static class SirstrapSettingsRegistry
    {
        private static readonly Lazy<IReadOnlyList<ISetting>> _settings = new(BuildSettings);
        private static readonly Lazy<IReadOnlyList<ISettingMigration>> _migrations = new(BuildMigrations);

        public static IReadOnlyList<ISetting> Settings => _settings.Value;

        public static IReadOnlyList<ISettingMigration> Migrations => _migrations.Value;

        private static IReadOnlyList<ISetting> BuildSettings() =>
        [
            new Setting("AUTO_UPDATE",
                () => SirstrapConfiguration.AutoUpdate.ToString(),
                value => { if (bool.TryParse(value, out var v)) SirstrapConfiguration.AutoUpdate = v; },
                Metric("AutoUpdate", () => SirstrapConfiguration.AutoUpdate)),
            new Setting("CHANNEL_NAME",
                () => SirstrapConfiguration.ChannelName,
                value => SirstrapConfiguration.ChannelName = value,
                Metric("ChannelName", () => SirstrapConfiguration.ChannelName)),
            new Setting("FONT_FAMILY",
                () => SirstrapConfiguration.FontFamily,
                value => SirstrapConfiguration.FontFamily = value.Equals("Minecraft", StringComparison.OrdinalIgnoreCase) ? "JetBrains Mono" : value,
                Metric("FontFamily", () => SirstrapConfiguration.FontFamily)),
            new Setting("INCOGNITO",
                () => SirstrapConfiguration.Incognito.ToString(),
                value => { if (bool.TryParse(value, out var v)) SirstrapConfiguration.Incognito = v; },
                Metric("Incognito", () => SirstrapConfiguration.Incognito)),
            new Setting("INSTALLATION_PATH",
                () => SirstrapConfiguration.InstallationPath,
                value => SirstrapConfiguration.InstallationPath = string.IsNullOrWhiteSpace(value) ? SirstrapConfiguration.GetDefaultInstallationPath() : value),
            new Setting("MULTI_INSTANCE",
                () => SirstrapConfiguration.MultiInstance.ToString(),
                value => { if (bool.TryParse(value, out var v)) SirstrapConfiguration.MultiInstance = v; },
                Metric("MultiInstance", () => SirstrapConfiguration.MultiInstance)),
            new Setting("PREVIOUS_INSTALLATION_PATH",
                () => SirstrapConfiguration.PreviousInstallationPath,
                value => SirstrapConfiguration.PreviousInstallationPath = value),
            new Setting("ROBLOX_API",
                () => SirstrapConfiguration.RobloxApi.ToString(),
                value => { if (bool.TryParse(value, out var v)) SirstrapConfiguration.RobloxApi = v; },
                Metric("RobloxApi", () => SirstrapConfiguration.RobloxApi)),
            new Setting("ROBLOX_CDN_URI_OVERRIDE",
                () => SirstrapConfiguration.RobloxCdnUriOverride,
                value => SirstrapConfiguration.RobloxCdnUriOverride = RobloxCdnService.NormalizeCdnUriOverride(value),
                Metric("RobloxCdnUriOverride", () => string.IsNullOrEmpty(SirstrapConfiguration.RobloxCdnUriOverride) ? "Auto" : "Custom")),
            new Setting("ROBLOX_VERSION_OVERRIDE",
                () => SirstrapConfiguration.RobloxVersionOverride,
                value => SirstrapConfiguration.RobloxVersionOverride = value,
                Metric("RobloxVersionOverride", () => string.IsNullOrEmpty(SirstrapConfiguration.RobloxVersionOverride) ? "None" : "Custom")),
            new Setting("TELEMETRY",
                () => SirstrapConfiguration.Telemetry.ToString(),
                value => { if (bool.TryParse(value, out var v)) SirstrapConfiguration.Telemetry = v; }),
            new Setting("TRAY_MODE",
                () => SirstrapConfiguration.TrayMode.ToString(),
                value => { if (Enum.TryParse<TrayMode>(value, true, out var v)) SirstrapConfiguration.TrayMode = v; },
                Metric("TrayMode", () => SirstrapConfiguration.TrayMode.ToString()))
        ];

        private static IReadOnlyList<ISettingMigration> BuildMigrations() =>
        [
            new SettingMigration("ROBLOX_CND_URI", "ROBLOX_CDN_URI_OVERRIDE", MigrateCdnUriOverride),
            new SettingMigration("ROBLOX_CDN_URI", "ROBLOX_CDN_URI_OVERRIDE", MigrateCdnUriOverride)
        ];

        private static void MigrateCdnUriOverride(string legacyValue)
        {
            string normalized = RobloxCdnService.NormalizeCdnUriOverride(legacyValue);

            if (normalized.Equals(RobloxCdnService.DefaultBaseUri, StringComparison.OrdinalIgnoreCase))
                normalized = string.Empty;

            SirstrapConfiguration.RobloxCdnUriOverride = normalized;
        }

        private static Action Metric(string name, Func<object> getValue)
            => () => SentrySdk.Metrics.EmitCounter($"settings.{name}", 1, new Dictionary<string, object> { ["value"] = getValue() });
    }
}
