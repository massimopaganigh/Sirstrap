namespace Sirstrap.Core.Settings
{
    public sealed class SettingsService(ISettingsRegistry settingsRegistry) : ISettingsService
    {
        private const string SettingsSectionHeader = "[SETTINGS]";

        public void EmitSettingsMetrics()
        {
            try
            {
                foreach (var setting in settingsRegistry.Settings)
                    setting.EmitMetric();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to emit the settings metrics.");
            }
        }

        public void LoadSettings(string? settingsFilePath = null)
        {
            try
            {
                settingsFilePath ??= GetSettingsFilePath();

                Log.Information("[*] Loading the settings from {SettingsFilePath}...", settingsFilePath);

                if (!File.Exists(settingsFilePath))
                    SaveSettings(settingsFilePath);

                var rows = File.ReadAllLines(settingsFilePath);
                var settingsByKey = GetSettingsByKey();
                var migrationsByKey = GetMigrationsByKey();
                var existingKeys = IniFormat.ExtractSectionKeys(rows, SettingsSectionHeader);

                var inSettingsSection = false;

                foreach (var row in rows)
                {
                    var trimmedRow = row.Trim();

                    if (IniFormat.IsSectionHeader(trimmedRow, SettingsSectionHeader, out var isSettingsSection))
                    {
                        inSettingsSection = isSettingsSection;

                        continue;
                    }

                    if (!inSettingsSection
                        || !IniFormat.TryParseRow(trimmedRow, out var key, out var value))
                        continue;

                    if (settingsByKey.TryGetValue(key, out var setting))
                    {
                        Log.Information("[*] Setting {Key} to {Value}...", key, value);

                        setting.Write(value);
                    }
                    else if (migrationsByKey.TryGetValue(key, out var migration)
                        && migration.ShouldMigrate(existingKeys))
                    {
                        Log.Information("[*] Migrating the setting {LegacyKey} to {TargetKey}...", key, migration.TargetKey);

                        migration.Apply(value);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to load the settings.");
            }
        }

        public void SaveSettings(string? settingsFilePath = null)
        {
            try
            {
                settingsFilePath ??= GetSettingsFilePath();

                Log.Information("[*] Saving the settings to {SettingsFilePath}...", settingsFilePath);

                if (!File.Exists(settingsFilePath))
                {
                    WriteFreshSettingsFile(settingsFilePath);

                    return;
                }

                File.WriteAllLines(settingsFilePath, UpdateSettingsRows([.. File.ReadAllLines(settingsFilePath)]));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to save the settings.");
            }
        }

        private HashSet<string> GetLegacyKeys()
            => new(settingsRegistry.Migrations.Select(migration => migration.LegacyKey), StringComparer.OrdinalIgnoreCase);

        private IReadOnlyDictionary<string, ISettingMigration> GetMigrationsByKey()
            => settingsRegistry.Migrations.ToDictionary(migration => migration.LegacyKey, StringComparer.OrdinalIgnoreCase);

        private List<string> GetMissingKeys(HashSet<string> foundKeys)
            => settingsRegistry.Settings
                .Where(setting => !foundKeys.Contains(setting.Key))
                .Select(setting => $"{setting.Key}={setting.Read()}")
                .ToList();

        private IReadOnlyDictionary<string, ISetting> GetSettingsByKey()
            => settingsRegistry.Settings.ToDictionary(setting => setting.Key, StringComparer.OrdinalIgnoreCase);

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

        private List<string> UpdateSettingsRows(List<string> rows)
        {
            var legacyKeys = GetLegacyKeys();
            var settingsByKey = GetSettingsByKey();
            var inSettingsSection = false;
            var settingsSectionIndex = -1;
            var foundKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var i = 0;

            while (i < rows.Count)
            {
                var trimmedRow = rows[i].Trim();

                if (IniFormat.IsSectionHeader(trimmedRow, SettingsSectionHeader, out var isSettingsSection))
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
                    || !IniFormat.TryParseRow(trimmedRow, out var key, out _))
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

            return rows;
        }

        private void WriteFreshSettingsFile(string settingsFilePath)
        {
            var directory = Path.GetDirectoryName(settingsFilePath);

            if (!string.IsNullOrWhiteSpace(directory)
                && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var content = new StringBuilder();

            content.AppendLine(SettingsSectionHeader);

            foreach (var setting in settingsRegistry.Settings)
                content.AppendLine($"{setting.Key}={setting.Read()}");

            File.WriteAllText(settingsFilePath, content.ToString(), Encoding.UTF8);
        }
    }
}
