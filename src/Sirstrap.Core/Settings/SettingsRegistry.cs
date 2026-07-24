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

            string MigrateVersionSource(string value)
            {
                if (!bool.TryParse(value, out var useRobloxApi))
                    return value;

                return useRobloxApi ? RobloxVersionSources.Roblox : RobloxVersionSources.SirHurt;
            }

            return
            [
                Setting.String("ROBLOX_CDN_URI_OVERRIDE",
                    () => configuration.RobloxCdnUriOverride,
                    value => configuration.RobloxCdnUriOverride = value,
                    Metric("RobloxCdnUriOverride", () => string.IsNullOrEmpty(configuration.RobloxCdnUriOverride) ? "Auto" : "Custom"),
                    legacyKeys: ["ROBLOX_CND_URI", "ROBLOX_CDN_URI"],
                    valueMigrator: NormalizeCdnUriOverride),
                Setting.Bool("ROBLOX_INCOGNITO",
                    () => configuration.RobloxIncognito,
                    value => configuration.RobloxIncognito = value,
                    Metric("RobloxIncognito", () => configuration.RobloxIncognito),
                    legacyKeys: ["INCOGNITO"]),
                Setting.String("ROBLOX_INSTALLATION_PATH",
                    () => configuration.RobloxInstallationPath,
                    value => configuration.RobloxInstallationPath = value,
                    legacyKeys: ["INSTALLATION_PATH"],
                    valueMigrator: value => string.IsNullOrWhiteSpace(value) ? SirstrapConfiguration.GetDefaultInstallationPath() : value),
                Setting.Bool("ROBLOX_MULTI_INSTANCE",
                    () => configuration.RobloxMultiInstance,
                    value => configuration.RobloxMultiInstance = value,
                    Metric("RobloxMultiInstance", () => configuration.RobloxMultiInstance),
                    legacyKeys: ["MULTI_INSTANCE"]),
                Setting.String("ROBLOX_VERSION_SOURCE",
                    () => configuration.RobloxVersionSource,
                    value => configuration.RobloxVersionSource = value,
                    Metric("RobloxVersionSource", () => configuration.RobloxVersionSource),
                    legacyKeys: ["ROBLOX_API"],
                    valueMigrator: MigrateVersionSource),
                Setting.String("SIRSTRAP_ACCENT_COLOR",
                    () => configuration.SirstrapAccentColor,
                    value => configuration.SirstrapAccentColor = value,
                    Metric("SirstrapAccentColor", () => configuration.SirstrapAccentColor)),
                Setting.Bool("SIRSTRAP_AUTO_UPDATE",
                    () => configuration.SirstrapAutoUpdate,
                    value => configuration.SirstrapAutoUpdate = value,
                    Metric("SirstrapAutoUpdate", () => configuration.SirstrapAutoUpdate),
                    legacyKeys: ["AUTO_UPDATE"]),
                Setting.String("SIRSTRAP_CHANNEL",
                    () => configuration.SirstrapChannel,
                    value => configuration.SirstrapChannel = value,
                    Metric("SirstrapChannel", () => configuration.SirstrapChannel),
                    legacyKeys: ["CHANNEL_NAME"]),
                Setting.String("SIRSTRAP_FONT_FAMILY",
                    () => configuration.SirstrapFontFamily,
                    value => configuration.SirstrapFontFamily = value,
                    Metric("SirstrapFontFamily", () => configuration.SirstrapFontFamily),
                    legacyKeys: ["FONT_FAMILY"],
                    valueMigrator: value => value.Equals("Minecraft", StringComparison.OrdinalIgnoreCase) ? "JetBrains Mono" : value),
                Setting.Bool("SIRSTRAP_TELEMETRY",
                    () => configuration.SirstrapTelemetry,
                    value => configuration.SirstrapTelemetry = value,
                    legacyKeys: ["TELEMETRY"]),
                Setting.Enum("SIRSTRAP_TRAY_MODE",
                    () => configuration.SirstrapTrayMode,
                    value => configuration.SirstrapTrayMode = value,
                    Metric("SirstrapTrayMode", () => configuration.SirstrapTrayMode.ToString()),
                    legacyKeys: ["TRAY_MODE"]),
                Setting.StateString("ROBLOX_PREVIOUS_INSTALLATION_PATH",
                    () => configuration.RobloxPreviousInstallationPath,
                    value => configuration.RobloxPreviousInstallationPath = value,
                    legacyKeys: ["PREVIOUS_INSTALLATION_PATH"]),
                Setting.Bool("CLEANER_ENABLED",
                    () => configuration.CleanerEnabled,
                    value => configuration.CleanerEnabled = value,
                    Metric("CleanerEnabled", () => configuration.CleanerEnabled)),
                Setting.Bool("CLEANER_FIRST_TIME_CONFIGURED",
                    () => configuration.CleanerFirstTimeConfigured,
                    value => configuration.CleanerFirstTimeConfigured = value),
                Setting.Bool("CLEANER_CLEAN_ON_LAUNCH",
                    () => configuration.CleanerCleanOnLaunch,
                    value => configuration.CleanerCleanOnLaunch = value,
                    Metric("CleanerCleanOnLaunch", () => configuration.CleanerCleanOnLaunch)),
                Setting.Bool("CLEANER_CLEAN_ON_EXIT",
                    () => configuration.CleanerCleanOnExit,
                    value => configuration.CleanerCleanOnExit = value,
                    Metric("CleanerCleanOnExit", () => configuration.CleanerCleanOnExit)),
                Setting.Bool("CLEANER_CLEAN_TEMP_FOLDERS",
                    () => configuration.CleanerCleanTempFolders,
                    value => configuration.CleanerCleanTempFolders = value,
                    Metric("CleanerCleanTempFolders", () => configuration.CleanerCleanTempFolders)),
                Setting.Bool("CLEANER_CLEAN_PROTECTED_FILES",
                    () => configuration.CleanerCleanProtectedFiles,
                    value => configuration.CleanerCleanProtectedFiles = value,
                    Metric("CleanerCleanProtectedFiles", () => configuration.CleanerCleanProtectedFiles))
            ];
        }
    }
}
