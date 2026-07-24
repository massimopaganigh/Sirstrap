using System.Text.Json;

namespace Sirstrap.Core.FFlags
{
    public sealed class FFlagManager(IPerformanceTelemetry performanceTelemetry) : IFFlagManager
    {
        public string GetFFlagsPath()
        {
            var dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap");
            Directory.CreateDirectory(dataDir);

            return Path.Combine(dataDir, "ClientAppSettings.json");
        }

        public Dictionary<string, object> LoadFFlags()
        {
            using var scope = performanceTelemetry.Measure("fflags.load");

            try
            {
                var filePath = GetFFlagsPath();

                if (!File.Exists(filePath))
                    return [];

                var json = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(json))
                    return [];

                using var doc = JsonDocument.Parse(json);
                var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    switch (prop.Value.ValueKind)
                    {
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            result[prop.Name] = prop.Value.GetBoolean();
                            break;
                        case JsonValueKind.Number:
                            if (prop.Value.TryGetInt64(out var lVal))
                                result[prop.Name] = lVal;
                            else if (prop.Value.TryGetDouble(out var dVal))
                                result[prop.Name] = dVal;
                            break;
                        case JsonValueKind.String:
                            result[prop.Name] = prop.Value.GetString() ?? string.Empty;
                            break;
                        default:
                            result[prop.Name] = prop.Value.ToString();
                            break;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to load FFlags.");
                scope.MarkFailed();

                return [];
            }
        }

        public void SaveFFlags(Dictionary<string, object> flags)
        {
            using var scope = performanceTelemetry.Measure("fflags.save");

            try
            {
                var filePath = GetFFlagsPath();
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(flags, options);

                File.WriteAllText(filePath, json);

                Log.Information("[*] Saved custom FFlags to {FilePath}.", filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to save FFlags.");
                scope.MarkFailed();
            }
        }

        public void DeployFFlags(string targetDirectory)
        {
            using var scope = performanceTelemetry.Measure("fflags.deploy");

            try
            {
                var flags = LoadFFlags();

                if (flags.Count == 0)
                    return;

                var clientSettingsDir = Path.Combine(targetDirectory, "ClientSettings");
                Directory.CreateDirectory(clientSettingsDir);

                var targetJsonPath = Path.Combine(clientSettingsDir, "ClientAppSettings.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(flags, options);

                File.WriteAllText(targetJsonPath, json);

                Log.Information("[*] Deployed {Count} FFlags to {TargetPath}.", flags.Count, targetJsonPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to deploy FFlags.");
                scope.MarkFailed();
            }
        }
    }
}
