namespace Sirstrap.Core
{
    public static class SirstrapConfigurationService
    {
        private static object? Set(Action action)
        {
            action();

            return null;
        }

        public static string GetSettingsPath()
        {
            var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap");

            if (!Directory.Exists(settingsPath))
                Directory.CreateDirectory(settingsPath);

            return Path.Combine(settingsPath, "settings.ini");
        }

        public static void LoadSettings(string? settingsPath = null)
        {
            try
            {
                settingsPath ??= GetSettingsPath();

                if (!File.Exists(settingsPath))
                    SaveSettings();

                var lines = File.ReadAllLines(settingsPath);
                var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();

                    if (string.IsNullOrEmpty(trimmedLine)
                        || trimmedLine.StartsWith('#'))
                        continue;

                    var trimmedLineParts = trimmedLine.Split('=', 2);

                    if (trimmedLineParts.Length != 2)
                        continue;

                    string trimmedTrimmedLineKey = trimmedLineParts[0].Trim();
                    string trimmedTrimmedLineValue = trimmedLineParts[1].Trim();

                    if (string.IsNullOrEmpty(trimmedTrimmedLineKey))
                        continue;

                    keys.Add(trimmedTrimmedLineKey);

                    if (string.IsNullOrEmpty(trimmedTrimmedLineValue))
                        continue;

                    _ = trimmedTrimmedLineKey switch
                    {
                        "AutoUpdate" => Set(() =>
                        {
                            if (bool.TryParse(trimmedTrimmedLineValue, out var v))
                                SirstrapConfiguration.AutoUpdate = v;
                        }),
                        "ChannelName" => Set(() => SirstrapConfiguration.ChannelName = trimmedTrimmedLineValue),
                        "FontFamily" => Set(() => SirstrapConfiguration.FontFamily = trimmedTrimmedLineValue),
                        "MultiInstance" => Set(() =>
                        {
                            if (bool.TryParse(trimmedTrimmedLineValue, out var v))
                                SirstrapConfiguration.MultiInstance = v;
                        }),
                        "Incognito" => Set(() =>
                        {
                            if (bool.TryParse(trimmedTrimmedLineValue, out var v))
                                SirstrapConfiguration.Incognito = v;
                        }),
                        "RobloxApi" => Set(() =>
                        {
                            if (bool.TryParse(trimmedTrimmedLineValue, out var v))
                                SirstrapConfiguration.RobloxApi = v;
                        }),
                        "RobloxCdnUri" => Set(() => SirstrapConfiguration.RobloxCdnUri = trimmedTrimmedLineValue),
                        "RobloxVersionOverride" => Set(() => SirstrapConfiguration.RobloxVersionOverride = trimmedTrimmedLineValue),
                        "SirHurtPath" => Set(() => SirstrapConfiguration.SirHurtPath = trimmedTrimmedLineValue),
                        _ => Set(() => Log.Warning("[*] Configuration unknown values: {0}={1}.", trimmedTrimmedLineKey, trimmedTrimmedLineValue))
                    };
                }

                if (keys.Count != 9)
                {
                    SaveSettings();

                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while trying to load the settings: {0}.", ex.Message);
            }
        }

        public static void SaveSettings(string? settingsPath = null)
        {

            try
            {
                settingsPath ??= GetSettingsPath();

                var (result, sirHurtPath) = SirHurtService.GetSirHurtPath();

                if (result)
                    SirstrapConfiguration.SirHurtPath = sirHurtPath;

                File.WriteAllLines(settingsPath, new List<string>
                {
                    $"AutoUpdate={SirstrapConfiguration.AutoUpdate}",
                    $"ChannelName={SirstrapConfiguration.ChannelName}",
                    $"FontFamily={SirstrapConfiguration.FontFamily}",
                    $"MultiInstance={SirstrapConfiguration.MultiInstance}",
                    $"Incognito={SirstrapConfiguration.Incognito}",
                    $"RobloxApi={SirstrapConfiguration.RobloxApi}",
                    $"RobloxCdnUri={SirstrapConfiguration.RobloxCdnUri}",
                    $"RobloxVersionOverride={SirstrapConfiguration.RobloxVersionOverride}",
                    $"SirHurtPath={SirstrapConfiguration.SirHurtPath}"
                });

                LoadSettings();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while trying to save the settings: {0}.", ex.Message);
            }
        }
    }
}
