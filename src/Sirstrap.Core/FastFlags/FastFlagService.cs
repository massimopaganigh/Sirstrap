namespace Sirstrap.Core.FastFlags
{
    public sealed class FastFlagService(SirstrapConfiguration sirstrapConfiguration) : IFastFlagService
    {
        private const string CLIENT_APP_SETTINGS_FILE_NAME = "ClientAppSettings.json";
        private const string CLIENT_SETTINGS_DIRECTORY_NAME = "ClientSettings";

        public void Apply(string versionDirectory, string? fastFlagsFilePath = null)
        {
            try
            {
                var clientSettingsDirectory = Path.Combine(versionDirectory, CLIENT_SETTINGS_DIRECTORY_NAME);
                var clientAppSettingsPath = Path.Combine(clientSettingsDirectory, CLIENT_APP_SETTINGS_FILE_NAME);
                var flags = LoadFlags(fastFlagsFilePath ?? GetFastFlagsFilePath());

                if (!sirstrapConfiguration.RobloxFastFlagsEnabled
                    || flags.Count == 0)
                {
                    if (File.Exists(clientAppSettingsPath))
                    {
                        File.Delete(clientAppSettingsPath);
                        Log.Information("[*] Removed the FastFlags file {ClientAppSettingsPath}.", clientAppSettingsPath);
                    }

                    return;
                }

                Directory.CreateDirectory(clientSettingsDirectory);
                File.WriteAllText(clientAppSettingsPath, SerializeFlags(flags), Encoding.UTF8);
                Log.Information("[*] Applied {FastFlagsCount} FastFlags to {ClientAppSettingsPath}.", flags.Count, clientAppSettingsPath);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to apply the FastFlags.");
            }
        }

        public IReadOnlyDictionary<string, string> GetFlags(string? fastFlagsFilePath = null)
        {
            SortedDictionary<string, string> flags = new(StringComparer.Ordinal);

            foreach (var (name, value) in LoadFlags(fastFlagsFilePath ?? GetFastFlagsFilePath()))
                flags[name] = ToDisplayValue(value);

            return flags;
        }

        public void SetFlags(IReadOnlyDictionary<string, string> flags, string? fastFlagsFilePath = null)
        {
            try
            {
                fastFlagsFilePath ??= GetFastFlagsFilePath();

                SortedDictionary<string, object> typedFlags = new(StringComparer.Ordinal);

                foreach (var (name, value) in flags)
                {
                    var trimmedName = name.Trim();

                    if (string.IsNullOrEmpty(trimmedName))
                        continue;

                    typedFlags[trimmedName] = ToTypedValue(value.Trim());
                }

                var directory = Path.GetDirectoryName(fastFlagsFilePath);

                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(fastFlagsFilePath, SerializeFlags(typedFlags), Encoding.UTF8);
                Log.Information("[*] Saved {FastFlagsCount} FastFlags to {FastFlagsFilePath}.", typedFlags.Count, fastFlagsFilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to save the FastFlags.");
            }
        }

        private static string GetFastFlagsFilePath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "FastFlags.json");

        private static SortedDictionary<string, object> LoadFlags(string fastFlagsFilePath)
        {
            SortedDictionary<string, object> flags = new(StringComparer.Ordinal);

            try
            {
                if (!File.Exists(fastFlagsFilePath))
                    return flags;

                using var document = JsonDocument.Parse(File.ReadAllText(fastFlagsFilePath));

                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    Log.Warning("[!] The FastFlags file {FastFlagsFilePath} is not a JSON object, ignoring it.", fastFlagsFilePath);

                    return flags;
                }

                foreach (var property in document.RootElement.EnumerateObject())
                {
                    object? value = property.Value.ValueKind switch
                    {
                        JsonValueKind.String => property.Value.GetString(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Number => property.Value.TryGetInt64(out var integerValue) ? integerValue : property.Value.GetDouble(),
                        _ => null
                    };

                    if (value == null)
                    {
                        Log.Warning("[!] The FastFlag {FastFlagName} has a non-scalar value, ignoring it.", property.Name);

                        continue;
                    }

                    flags[property.Name] = value;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to load the FastFlags from {FastFlagsFilePath}.", fastFlagsFilePath);
                flags.Clear();
            }

            return flags;
        }

        private static string SerializeFlags(SortedDictionary<string, object> flags)
        {
            using MemoryStream stream = new();

            using (Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();

                foreach (var (name, value) in flags)
                    switch (value)
                    {
                        case bool boolValue:
                            writer.WriteBoolean(name, boolValue);

                            break;
                        case long longValue:
                            writer.WriteNumber(name, longValue);

                            break;
                        case double doubleValue:
                            writer.WriteNumber(name, doubleValue);

                            break;
                        default:
                            writer.WriteString(name, (string)value);

                            break;
                    }

                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private static string ToDisplayValue(object value) => value switch
        {
            bool boolValue => boolValue ? "True" : "False",
            long longValue => longValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            double doubleValue => doubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => (string)value
        };

        private static object ToTypedValue(string value)
        {
            if (bool.TryParse(value, out var boolValue))
                return boolValue;

            if (long.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var longValue))
                return longValue;

            if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var doubleValue))
                return doubleValue;

            return value;
        }
    }
}
