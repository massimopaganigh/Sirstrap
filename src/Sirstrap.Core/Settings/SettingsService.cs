namespace Sirstrap.Core.Settings
{
    public sealed class SettingsService(ISettingsRegistry settingsRegistry) : ISettingsService
    {
        public void EmitSettingsMetrics()
        {
            try
            {
                foreach (var setting in settingsRegistry.Settings)
                    setting.MetricEmitter?.Invoke();
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
                var definitionsByKey = GetDefinitionsByKey();
                var definitionsByAlias = GetDefinitionsByAlias();

                SettingsSection? currentSection = null;

                foreach (var row in rows)
                {
                    var trimmedRow = row.Trim();

                    if (IniFormat.TryParseSectionHeader(trimmedRow, out var section))
                    {
                        currentSection = section;

                        continue;
                    }

                    if (currentSection == null
                        || !IniFormat.TryParseRow(trimmedRow, out var key, out var value))
                        continue;

                    if (definitionsByKey.TryGetValue(key, out var definition)
                        && definition.Section == currentSection)
                    {
                        Log.Information("[*] Setting {Key} to {Value}...", key, value);

                        ApplySetting(definition, value);
                    }
                    else if (definitionsByAlias.TryGetValue(key, out var aliasDefinition)
                        && aliasDefinition.Section == currentSection)
                    {
                        Log.Information("[*] Migrating the setting {LegacyKey} to {TargetKey}...", key, aliasDefinition.Key);

                        ApplySetting(aliasDefinition, value);
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

        private static void ApplySetting(SettingDefinition definition, string rawValue)
            => definition.Setter(definition.ValueMigrator?.Invoke(rawValue) ?? rawValue);

        private IReadOnlyDictionary<string, SettingDefinition> GetDefinitionsByAlias()
        {
            var map = new Dictionary<string, SettingDefinition>(StringComparer.OrdinalIgnoreCase);

            foreach (var definition in settingsRegistry.Settings)
                foreach (var legacyKey in definition.LegacyKeys)
                    map[legacyKey] = definition;

            return map;
        }

        private IReadOnlyDictionary<string, SettingDefinition> GetDefinitionsByKey()
            => settingsRegistry.Settings.ToDictionary(definition => definition.Key, StringComparer.OrdinalIgnoreCase);

        private List<string> GetMissingRows(SettingsSection section, HashSet<string> foundKeys)
            => settingsRegistry.Settings
                .Where(definition => definition.Section == section && !foundKeys.Contains(definition.Key))
                .Select(definition => $"{definition.Key}={definition.Getter()}")
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

        private static string SectionHeader(SettingsSection section) => section switch
        {
            SettingsSection.Settings => "[SETTINGS]",
            SettingsSection.State => "[STATE]",
            _ => throw new ArgumentOutOfRangeException(nameof(section))
        };

        private List<string> UpdateSettingsRows(List<string> rows)
        {
            var definitionsByKey = GetDefinitionsByKey();
            var sectionIndexes = new Dictionary<SettingsSection, int>();
            var foundKeys = new Dictionary<SettingsSection, HashSet<string>>
            {
                [SettingsSection.Settings] = new(StringComparer.OrdinalIgnoreCase),
                [SettingsSection.State] = new(StringComparer.OrdinalIgnoreCase)
            };

            SettingsSection? currentSection = null;

            var i = 0;

            while (i < rows.Count)
            {
                var trimmedRow = rows[i].Trim();

                if (IniFormat.TryParseSectionHeader(trimmedRow, out var section))
                {
                    var previousSection = currentSection;

                    currentSection = section;

                    if (currentSection.HasValue)
                        sectionIndexes[currentSection.Value] = i;

                    if (previousSection.HasValue
                        && previousSection != currentSection)
                    {
                        var missingRows = GetMissingRows(previousSection.Value, foundKeys[previousSection.Value]);
                        var insertIndex = i;

                        while (insertIndex > 0
                            && string.IsNullOrWhiteSpace(rows[insertIndex - 1]))
                            insertIndex--;

                        rows.InsertRange(insertIndex, missingRows);

                        i += missingRows.Count;
                    }

                    i++;

                    continue;
                }

                if (currentSection == null
                    || !IniFormat.TryParseRow(trimmedRow, out var key, out _))
                {
                    i++;

                    continue;
                }

                if (!definitionsByKey.TryGetValue(key, out var definition)
                    || definition.Section != currentSection.Value)
                {
                    rows.RemoveAt(i);

                    continue;
                }

                foundKeys[currentSection.Value].Add(key);
                rows[i] = $"{key}={definition.Getter()}";

                i++;
            }

            AppendOrInsertSection(rows, SettingsSection.Settings, currentSection, sectionIndexes, foundKeys);
            AppendOrInsertSection(rows, SettingsSection.State, currentSection, sectionIndexes, foundKeys);

            return rows;
        }

        private void AppendOrInsertSection(List<string> rows, SettingsSection section, SettingsSection? currentSection, Dictionary<SettingsSection, int> sectionIndexes, Dictionary<SettingsSection, HashSet<string>> foundKeys)
        {
            if (sectionIndexes.ContainsKey(section))
            {
                if (currentSection == section)
                    rows.AddRange(GetMissingRows(section, foundKeys[section]));

                return;
            }

            if (rows.Count > 0)
                rows.Add(string.Empty);

            rows.Add(SectionHeader(section));
            rows.AddRange(GetMissingRows(section, foundKeys[section]));
        }

        private void WriteFreshSettingsFile(string settingsFilePath)
        {
            var directory = Path.GetDirectoryName(settingsFilePath);

            if (!string.IsNullOrWhiteSpace(directory)
                && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var content = new StringBuilder();

            AppendSection(content, SettingsSection.Settings);
            content.AppendLine();
            AppendSection(content, SettingsSection.State);

            File.WriteAllText(settingsFilePath, content.ToString(), Encoding.UTF8);
        }

        private void AppendSection(StringBuilder content, SettingsSection section)
        {
            content.AppendLine(SectionHeader(section));

            foreach (var definition in settingsRegistry.Settings.Where(definition => definition.Section == section))
                content.AppendLine($"{definition.Key}={definition.Getter()}");
        }
    }
}
