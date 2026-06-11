namespace Sirstrap.Core
{
    public static class SirstrapConfigurationService
    {
        private const string SettingsSectionHeader = "[SETTINGS]";

        private static IReadOnlyDictionary<string, ISetting> GetSettingsByKey()
            => SirstrapSettingsRegistry.Settings.ToDictionary(setting => setting.Key, StringComparer.OrdinalIgnoreCase);

        private static IReadOnlyDictionary<string, ISettingMigration> GetMigrationsByKey()
            => SirstrapSettingsRegistry.Migrations.ToDictionary(migration => migration.LegacyKey, StringComparer.OrdinalIgnoreCase);

        private static HashSet<string> GetLegacyKeys()
            => new(SirstrapSettingsRegistry.Migrations.Select(migration => migration.LegacyKey), StringComparer.OrdinalIgnoreCase);

        private static List<string> GetMissingKeys(HashSet<string> foundKeys)
            => SirstrapSettingsRegistry.Settings
                .Where(setting => !foundKeys.Contains(setting.Key))
                .Select(setting => $"{setting.Key}={setting.Read()}")
                .ToList();

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

        private static bool IsSectionHeader(string trimmedRow, out bool isSettingsSection)
        {
            isSettingsSection = false;

            if (!trimmedRow.StartsWith('['))
                return false;

            isSettingsSection = trimmedRow.Equals(SettingsSectionHeader, StringComparison.InvariantCultureIgnoreCase);

            return true;
        }

        private static bool TryParseRow(string trimmedRow, out string key, out string value)
        {
            key = string.Empty;
            value = string.Empty;

            if (!trimmedRow.Contains('='))
                return false;

            var parts = trimmedRow.Split('=', 2);

            if (parts.Length != 2)
                return false;

            key = parts[0].Trim();
            value = parts[1].Trim();

            return !string.IsNullOrEmpty(key);
        }

        private static HashSet<string> ExtractExistingKeys(IEnumerable<string> rows)
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var inSettingsSection = false;

            foreach (var row in rows)
            {
                var trimmedRow = row.Trim();

                if (IsSectionHeader(trimmedRow, out var isSettingsSection))
                {
                    inSettingsSection = isSettingsSection;

                    continue;
                }

                if (!inSettingsSection)
                    continue;

                if (TryParseRow(trimmedRow, out var key, out _))
                    keys.Add(key);
            }

            return keys;
        }

        public static void LoadSettings(string? settingsFilePath = null)
        {
            try
            {
                settingsFilePath ??= GetSettingsFilePath();

                Log.Information("[{0}] Loading settings (SettingsFilePath: {1})...", nameof(LoadSettings), settingsFilePath);

                if (!File.Exists(settingsFilePath))
                    SaveSettings(settingsFilePath);

                var rows = File.ReadAllLines(settingsFilePath);
                var settingsByKey = GetSettingsByKey();
                var migrationsByKey = GetMigrationsByKey();
                var existingKeys = ExtractExistingKeys(rows);

                var inSettingsSection = false;

                foreach (var row in rows)
                {
                    var trimmedRow = row.Trim();

                    if (IsSectionHeader(trimmedRow, out var isSettingsSection))
                    {
                        inSettingsSection = isSettingsSection;

                        continue;
                    }

                    if (!inSettingsSection
                        || !TryParseRow(trimmedRow, out var key, out var value))
                        continue;

                    if (settingsByKey.TryGetValue(key, out var setting))
                    {
                        Log.Information("[{0}] Setting {1} to {2}...", nameof(LoadSettings), key, value);

                        setting.Write(value);
                    }
                    else if (migrationsByKey.TryGetValue(key, out var migration)
                        && migration.ShouldMigrate(existingKeys))
                    {
                        Log.Information("[{0}] Migrating {1} to {2}...", nameof(LoadSettings), key, migration.TargetKey);

                        migration.Apply(value);
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
                foreach (var setting in SirstrapSettingsRegistry.Settings)
                    setting.EmitMetric();
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

                var legacyKeys = GetLegacyKeys();
                var settingsByKey = GetSettingsByKey();

                if (!File.Exists(settingsFilePath))
                {
                    WriteFreshSettingsFile(settingsFilePath);

                    return;
                }

                var rows = File.ReadAllLines(settingsFilePath).ToList();
                var inSettingsSection = false;
                var settingsSectionIndex = -1;
                var foundKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var i = 0;

                while (i < rows.Count)
                {
                    var trimmedRow = rows[i].Trim();

                    if (IsSectionHeader(trimmedRow, out var isSettingsSection))
                    {
                        var wasSettingsSection = inSettingsSection;

                        inSettingsSection = isSettingsSection;

                        if (inSettingsSection)
                            settingsSectionIndex = i;

                        if (wasSettingsSection
                            && !inSettingsSection)
                        {
                            var missingKeys = GetMissingKeys(foundKeys);

                            rows.InsertRange(i, missingKeys);

                            i += missingKeys.Count;
                        }

                        i++;

                        continue;
                    }

                    if (!inSettingsSection
                        || !TryParseRow(trimmedRow, out var key, out _))
                    {
                        i++;

                        continue;
                    }

                    if (legacyKeys.Contains(key))
                    {
                        rows.RemoveAt(i);

                        continue;
                    }

                    foundKeys.Add(key);

                    if (settingsByKey.TryGetValue(key, out var setting))
                        rows[i] = $"{key}={setting.Read()}";

                    i++;
                }

                if (settingsSectionIndex == -1)
                {
                    rows.Insert(0, SettingsSectionHeader);
                    rows.InsertRange(1, GetMissingKeys(foundKeys));
                }
                else if (inSettingsSection)
                    rows.AddRange(GetMissingKeys(foundKeys));

                File.WriteAllLines(settingsFilePath, rows);
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(SaveSettings));
            }
        }

        private static void WriteFreshSettingsFile(string settingsFilePath)
        {
            var directory = Path.GetDirectoryName(settingsFilePath);

            if (!string.IsNullOrWhiteSpace(directory)
                && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var content = new StringBuilder();

            content.AppendLine(SettingsSectionHeader);

            foreach (var setting in SirstrapSettingsRegistry.Settings)
                content.AppendLine($"{setting.Key}={setting.Read()}");

            File.WriteAllText(settingsFilePath, content.ToString(), Encoding.UTF8);
        }
    }
}
