namespace Sirstrap.Core.Settings
{
    public sealed class SettingsRegistry : ISettingsRegistry
    {
        private readonly Lazy<IReadOnlyList<SettingDefinition>> _settings;

        public SettingsRegistry(SirstrapConfiguration configuration, ICdnUriNormalizer cdnUriNormalizer, IPerformanceTelemetry performanceTelemetry)
        {
            _settings = new(() => BuildSettings(configuration, cdnUriNormalizer, performanceTelemetry));
        }

        public IReadOnlyList<SettingDefinition> Settings => _settings.Value;

        private static IReadOnlyList<SettingDefinition> BuildSettings(SirstrapConfiguration configuration, ICdnUriNormalizer cdnUriNormalizer, IPerformanceTelemetry performanceTelemetry)
        {
            Action Metric(string name, Func<object> getValue)
                => () => performanceTelemetry.RecordCounter($"settings.{name}", new Dictionary<string, object> { ["value"] = getValue() });

            string NormalizeCdnUriOverride(string value)
            {
                string normalized = cdnUriNormalizer.Normalize(value);

                return normalized.Equals(RobloxCdnService.DefaultBaseUri, StringComparison.OrdinalIgnoreCase) ? string.Empty : normalized;
            }

            return
            [
                Setting.Bool("AUTO_UPDATE",
                    () => configuration.AutoUpdate,
                    value => configuration.AutoUpdate = value,
                    Metric("AutoUpdate", () => configuration.AutoUpdate)),
                Setting.String("CHANNEL_NAME",
                    () => configuration.ChannelName,
                    value => configuration.ChannelName = value,
                    Metric("ChannelName", () => configuration.ChannelName)),
                Setting.String("FONT_FAMILY",
                    () => configuration.FontFamily,
                    value => configuration.FontFamily = value,
                    Metric("FontFamily", () => configuration.FontFamily),
                    valueMigrator: value => value.Equals("Minecraft", StringComparison.OrdinalIgnoreCase) ? "JetBrains Mono" : value),
                Setting.Bool("INCOGNITO",
                    () => configuration.Incognito,
                    value => configuration.Incognito = value,
                    Metric("Incognito", () => configuration.Incognito)),
                Setting.String("INSTALLATION_PATH",
                    () => configuration.InstallationPath,
                    value => configuration.InstallationPath = value,
                    valueMigrator: value => string.IsNullOrWhiteSpace(value) ? SirstrapConfiguration.GetDefaultInstallationPath() : value),
                Setting.Bool("MULTI_INSTANCE",
                    () => configuration.MultiInstance,
                    value => configuration.MultiInstance = value,
                    Metric("MultiInstance", () => configuration.MultiInstance)),
                Setting.Bool("ROBLOX_API",
                    () => configuration.RobloxApi,
                    value => configuration.RobloxApi = value,
                    Metric("RobloxApi", () => configuration.RobloxApi)),
                Setting.String("ROBLOX_CDN_URI_OVERRIDE",
                    () => configuration.RobloxCdnUriOverride,
                    value => configuration.RobloxCdnUriOverride = value,
                    Metric("RobloxCdnUriOverride", () => string.IsNullOrEmpty(configuration.RobloxCdnUriOverride) ? "Auto" : "Custom"),
                    legacyKeys: ["ROBLOX_CND_URI", "ROBLOX_CDN_URI"],
                    valueMigrator: NormalizeCdnUriOverride),
                Setting.String("ROBLOX_VERSION_OVERRIDE",
                    () => configuration.RobloxVersionOverride,
                    value => configuration.RobloxVersionOverride = value,
                    Metric("RobloxVersionOverride", () => string.IsNullOrEmpty(configuration.RobloxVersionOverride) ? "None" : "Custom")),
                Setting.Bool("TELEMETRY",
                    () => configuration.Telemetry,
                    value => configuration.Telemetry = value),
                Setting.Enum("TRAY_MODE",
                    () => configuration.TrayMode,
                    value => configuration.TrayMode = value,
                    Metric("TrayMode", () => configuration.TrayMode.ToString())),
                Setting.StateString("PREVIOUS_INSTALLATION_PATH",
                    () => configuration.PreviousInstallationPath,
                    value => configuration.PreviousInstallationPath = value)
            ];
        }
    }
}
