namespace Sirstrap.Core
{
    public static class SirstrapConfigurationService
    {
        private static readonly HashSet<string> _legacySettingsKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "ROBLOX_CND_URI",
            "ROBLOX_CDN_URI"
        };

        private static List<string> GetMissingKeys(HashSet<string> foundKeys)
        {
            var settingsDefinitions = GetSettingsDefinitions();

            return settingsDefinitions
                .Where(setting => !foundKeys.Contains(setting.Key))
                .Select(setting => $"{setting.Key}={setting.Value.Getter()}")
                .ToList();
        }

        private static Dictionary<string, (Func<string> Getter, Action<string> Setter)> GetSettingsDefinitions() => new()
        {
            ["AUTO_UPDATE"] = (
                () => SirstrapConfiguration.AutoUpdate.ToString(),
                value => { if (bool.TryParse(value, out var v)) SirstrapConfiguration.AutoUpdate = v; }
            ),
            ["CHANNEL_NAME"] = (
                () => SirstrapConfiguration.ChannelName,
                value => SirstrapConfiguration.ChannelName = value
            ),
            ["FONT_FAMILY"] = (
                () => SirstrapConfiguration.FontFamily,
                value => SirstrapConfiguration.FontFamily = value.Equals("Minecraft", StringComparison.OrdinalIgnoreCase) ? "JetBrains Mono" : value
            ),
            ["INCOGNITO"] = (
                () => SirstrapConfiguration.Incognito.ToString(),
                value => { if (bool.TryParse(value, out var v)) SirstrapConfiguration.Incognito = v; }
            ),
            ["INSTALLATION_PATH"] = (
                () => SirstrapConfiguration.InstallationPath,
                value => SirstrapConfiguration.InstallationPath = string.IsNullOrWhiteSpace(value) ? SirstrapConfiguration.GetDefaultInstallationPath() : value
            ),
            ["MULTI_INSTANCE"] = (
                () => SirstrapConfiguration.MultiInstance.ToString(),
                value => { if (bool.TryParse(value, out var v)) SirstrapConfiguration.MultiInstance = v; }
            ),
            ["PREVIOUS_INSTALLATION_PATH"] = (
                () => SirstrapConfiguration.PreviousInstallationPath,
                value => SirstrapConfiguration.PreviousInstallationPath = value
            ),
            ["ROBLOX_API"] = (
                () => SirstrapConfiguration.RobloxApi.ToString(),
                value => { if (bool.TryParse(value, out var v)) SirstrapConfiguration.RobloxApi = v; }
            ),
            ["ROBLOX_CDN_URI_OVERRIDE"] = (
                () => SirstrapConfiguration.RobloxCdnUriOverride,
                value => SirstrapConfiguration.RobloxCdnUriOverride = RobloxCdnService.NormalizeCdnUriOverride(value)
            ),
            ["ROBLOX_VERSION_OVERRIDE"] = (
                () => SirstrapConfiguration.RobloxVersionOverride,
                value => SirstrapConfiguration.RobloxVersionOverride = value
            ),
            ["TELEMETRY"] = (
                () => SirstrapConfiguration.Telemetry.ToString(),
                value => { if (bool.TryParse(value, out var v)) SirstrapConfiguration.Telemetry = v; }
            ),
            ["TRAY_MODE"] = (
                () => SirstrapConfiguration.TrayMode.ToString(),
                value => { if (Enum.TryParse<TrayMode>(value, true, out var v)) SirstrapConfiguration.TrayMode = v; }
            )
        };

        private static string GetSettingsFilePath()
        {
            var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap");

            if (!Directory.Exists(settingsPath))
                Directory.CreateDirectory(settingsPath);
            else
            {
                var oldSettingsFilePath = Path.Combine(settingsPath, "settings.ini");

                if (File.Exists(oldSettingsFilePath))
                    File.Delete(oldSettingsFilePath);
            }

            return Path.Combine(settingsPath, "Sirstrap.ini");
        }

        public static void LoadSettings(string? settingsFilePath = null)
        {
            try
            {
                settingsFilePath ??= GetSettingsFilePath();

                Log.Information("[{0}] Loading settings (SettingsFilePath: {1})...", nameof(LoadSettings), settingsFilePath);

                if (!File.Exists(settingsFilePath))
                    SaveSettings();

                var rows = File.ReadAllLines(settingsFilePath);
                var settingsSection = false;
                var settingsDefinitions = GetSettingsDefinitions();
                bool hasRobloxCdnUriOverride = rows
                    .Select(row => row.Trim())
                    .Any(row => row.StartsWith("ROBLOX_CDN_URI_OVERRIDE", StringComparison.OrdinalIgnoreCase));

                foreach (var row in rows)
                {
                    var trimmedRow = row.Trim();

                    if (trimmedRow.StartsWith('['))
                    {
                        settingsSection = trimmedRow.Equals("[SETTINGS]", StringComparison.InvariantCultureIgnoreCase);

                        continue;
                    }

                    if (!settingsSection
                        || !trimmedRow.Contains('='))
                        continue;

                    var parts = trimmedRow.Split('=');

                    if (parts.Length != 2)
                        continue;

                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    if (settingsDefinitions.TryGetValue(key, out var definition))
                    {
                        Log.Information("[{0}] Setting {1} to {2}...", nameof(LoadSettings), key, value);

                        definition.Setter(value);
                    }
                    else if (_legacySettingsKeys.Contains(key)
                        && !hasRobloxCdnUriOverride)
                    {
                        string normalized = RobloxCdnService.NormalizeCdnUriOverride(value);

                        if (normalized.Equals(RobloxCdnService.DefaultBaseUri, StringComparison.OrdinalIgnoreCase))
                            normalized = string.Empty;

                        Log.Information("[{0}] Migrating {1} to ROBLOX_CDN_URI_OVERRIDE...", nameof(LoadSettings), key);

                        SirstrapConfiguration.RobloxCdnUriOverride = normalized;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(LoadSettings));
            }
        }

        public static void EmitSettingsMetrics()
        {
            try
            {
                SentrySdk.Metrics.EmitCounter("settings.AutoUpdate", 1, new Dictionary<string, object> { ["value"] = SirstrapConfiguration.AutoUpdate });
                SentrySdk.Metrics.EmitCounter("settings.MultiInstance", 1, new Dictionary<string, object> { ["value"] = SirstrapConfiguration.MultiInstance });
                SentrySdk.Metrics.EmitCounter("settings.Incognito", 1, new Dictionary<string, object> { ["value"] = SirstrapConfiguration.Incognito });
                SentrySdk.Metrics.EmitCounter("settings.RobloxApi", 1, new Dictionary<string, object> { ["value"] = SirstrapConfiguration.RobloxApi });
                SentrySdk.Metrics.EmitCounter("settings.TrayMode", 1, new Dictionary<string, object> { ["value"] = SirstrapConfiguration.TrayMode.ToString() });
                SentrySdk.Metrics.EmitCounter("settings.ChannelName", 1, new Dictionary<string, object> { ["value"] = SirstrapConfiguration.ChannelName });
                SentrySdk.Metrics.EmitCounter("settings.FontFamily", 1, new Dictionary<string, object> { ["value"] = SirstrapConfiguration.FontFamily });
                SentrySdk.Metrics.EmitCounter("settings.RobloxCdnUriOverride", 1, new Dictionary<string, object> { ["value"] = string.IsNullOrEmpty(SirstrapConfiguration.RobloxCdnUriOverride) ? "Auto" : "Custom" });
                SentrySdk.Metrics.EmitCounter("settings.RobloxVersionOverride", 1, new Dictionary<string, object> { ["value"] = string.IsNullOrEmpty(SirstrapConfiguration.RobloxVersionOverride) ? "None" : "Custom" });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, nameof(EmitSettingsMetrics));
            }
        }

        public static void SaveSettings(string? settingsFilePath = null)
        {
            try
            {
                settingsFilePath ??= GetSettingsFilePath();

                Log.Information("[{0}] Saving settings (SettingsFilePath: {1})...", nameof(SaveSettings), settingsFilePath);

                var settingsDefinitions = GetSettingsDefinitions();

                if (!File.Exists(settingsFilePath))
                {
                    var directory = Path.GetDirectoryName(settingsFilePath);

                    if (!string.IsNullOrWhiteSpace(directory)
                        && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    var content = new StringBuilder();

                    content.AppendLine("[SETTINGS]");

                    foreach (var setting in settingsDefinitions)
                        content.AppendLine($"{setting.Key}={setting.Value.Getter()}");

                    File.WriteAllText(settingsFilePath, content.ToString(), Encoding.UTF8);

                    return;
                }

                var rows = File.ReadAllLines(settingsFilePath).ToList();
                var settingsSection = false;
                var settingsSectionIndex = -1;
                var foundKeys = new HashSet<string>();

                var i = 0;
                while (i < rows.Count)
                {
                    var trimmedRow = rows[i].Trim();

                    if (trimmedRow.StartsWith('['))
                    {
                        var wasSettingsSection = settingsSection;

                        settingsSection = trimmedRow.Equals("[SETTINGS]", StringComparison.InvariantCultureIgnoreCase);

                        if (settingsSection)
                            settingsSectionIndex = i;

                        if (wasSettingsSection
                            && !settingsSection)
                        {
                            var missingKeys = GetMissingKeys(foundKeys);

                            rows.InsertRange(i, missingKeys);

                            i += missingKeys.Count;
                        }

                        i++;
                        continue;
                    }

                    if (!settingsSection
                        || !trimmedRow.Contains('='))
                    {
                        i++;
                        continue;
                    }

                    var parts = trimmedRow.Split('=');

                    if (parts.Length != 2)
                    {
                        i++;
                        continue;
                    }

                    var key = parts[0].Trim();

                    if (_legacySettingsKeys.Contains(key))
                    {
                        rows.RemoveAt(i);

                        continue;
                    }

                    foundKeys.Add(key);

                    if (settingsDefinitions.TryGetValue(key, out var definition))
                        rows[i] = $"{key}={definition.Getter()}";

                    i++;
                }

                if (settingsSectionIndex == -1)
                {
                    rows.Insert(0, "[SETTINGS]");
                    rows.InsertRange(1, GetMissingKeys(foundKeys));
                }
                else if (settingsSection)
                    rows.AddRange(GetMissingKeys(foundKeys));

                File.WriteAllLines(settingsFilePath, rows);
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(SaveSettings));
            }
        }
    }
}
