namespace Sirstrap.Core
{
    public static class AppSettingsManager
    {
        private static readonly Lock _lock = new();
        private static AppSettings? _appSettings;

        private static AppSettings LoadSettings()
        {
            string settingsPath = GetSettingsPath();
            HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase);
            AppSettings appSettings = new();
            bool toUpdate = false;

            try
            {
                if (File.Exists(settingsPath))
                {
                    string[] lines = File.ReadAllLines(settingsPath);

                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();

                        if (string.IsNullOrEmpty(trimmedLine)
                            || trimmedLine.StartsWith('#'))
                            continue;

                        string[] parts = trimmedLine.Split('=', 2);

                        if (parts.Length != 2)
                            continue;

                        string trimmedKey = parts[0].Trim();
                        string trimmedValue = parts[1].Trim();

                        if (string.IsNullOrEmpty(trimmedKey)
                            || string.IsNullOrEmpty(trimmedValue))
                            continue;

                        keys.Add(trimmedKey);

                        switch (trimmedKey)
                        {
                            case "RobloxCdnUrl":
                                appSettings.RobloxCdnUrl = trimmedValue;
                                break;
                            case "SirstrapUpdateChannel":
                                appSettings.SirstrapUpdateChannel = trimmedValue;
                                break;
                            case "SafeMode":
                                if (bool.TryParse(trimmedValue, out bool safeMode))
                                    appSettings.SafeMode = safeMode;
                                break;
                            case "MultiInstance":
                                if (bool.TryParse(trimmedValue, out bool multiInstance))
                                    appSettings.MultiInstance = multiInstance;
                                break;
                            case "IncognitoMode":
                                if (bool.TryParse(trimmedValue, out bool incognitoMode))
                                    appSettings.IncognitoMode = incognitoMode;
                                break;
                            default:
                                Log.Warning("[*] Unknown setting: {0}={1}.", trimmedKey, trimmedValue);
                                break;
                        }
                    }

                    if (!keys.Contains("RobloxCdnUrl")
                        || !keys.Contains("SirstrapUpdateChannel")
                        || !keys.Contains("SafeMode")
                        || !keys.Contains("MultiInstance")
                        || !keys.Contains("IncognitoMode"))
                        toUpdate = true;

                    Log.Information("[*] Loaded settings with success from {0}.", settingsPath);

                    if (toUpdate)
                    {
                        Log.Information("[*] Updating settings...");

                        SaveSettings(appSettings);
                    }

                    return appSettings;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[*] Loaded settings with exception: {0}", ex.Message);
            }

            SaveSettings(appSettings);

            return appSettings;
        }

        public static AppSettings GetSettings()
        {
            if (_appSettings == null)
                lock (_lock)
                    _appSettings ??= LoadSettings();

            return _appSettings;
        }

        public static string GetSettingsPath()
        {
            string settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap");

            Directory.CreateDirectory(settingsDirectory);

            return Path.Combine(settingsDirectory, "settings.ini");
        }

        public static bool SaveSettings(AppSettings settings)
        {
            string settingsPath = GetSettingsPath();

            try
            {
                List<string> lines =
                [
                    $"RobloxCdnUrl={settings.RobloxCdnUrl}",
                    $"SirstrapUpdateChannel={settings.SirstrapUpdateChannel}",
                    $"SafeMode={settings.SafeMode}",
                    $"MultiInstance={settings.MultiInstance}",
                    $"[WIP]",
                    $"IncognitoMode={settings.IncognitoMode}"
                ];

                File.WriteAllLines(settingsPath, lines);

                _appSettings = settings;

                Log.Information("[*] Saved settings with success to {0}.", settingsPath);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Saved settings with exception: {0}", ex.Message);

                return false;
            }
        }
    }
}