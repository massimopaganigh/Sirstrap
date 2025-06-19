namespace Sirstrap.Core
{
    public static class SirstrapConfigurationService
    {
        private static string GetConfigurationPath()
        {
            string configurationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap");

            Directory.CreateDirectory(configurationPath);

            return Path.Combine(configurationPath, "settings.ini");
        }

        private static void SaveConfiguration()
        {
            string configurationPath = GetConfigurationPath();

            try
            {
                List<string> lines =
                [
                    $"ChannelName={SirstrapConfiguration.ChannelName}",
                    $"MultiInstance={SirstrapConfiguration.MultiInstance}",
                    $"RobloxApi={SirstrapConfiguration.RobloxApi}",
                    $"RobloxCdnUri={SirstrapConfiguration.RobloxCdnUri}",
                    $"# WIP",
                    $"Incognito={SirstrapConfiguration.Incognito}"
                ];

                File.WriteAllLines(configurationPath, lines);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Configuration saving exception: {0}", ex.Message);
            }
        }

        public static void LoadConfiguration()
        {
            string configurationPath = GetConfigurationPath();
            HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase);
            bool toUpdate = false;

            try
            {
                if (File.Exists(configurationPath))
                {
                    string[] lines = File.ReadAllLines(configurationPath);

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
                            case "ChannelName":
                                SirstrapConfiguration.ChannelName = trimmedValue;
                                break;
                            case "MultiInstance":
                                if (bool.TryParse(trimmedValue, out bool multiInstance))
                                    SirstrapConfiguration.MultiInstance = multiInstance;
                                break;
                            case "RobloxCdnUri":
                                SirstrapConfiguration.RobloxCdnUri = trimmedValue;
                                break;
                            case "RobloxApi":
                                if (bool.TryParse(trimmedValue, out bool safeMode))
                                    SirstrapConfiguration.RobloxApi = safeMode;
                                break;
                            case "Incognito":
                                if (bool.TryParse(trimmedValue, out bool incognitoMode))
                                    SirstrapConfiguration.Incognito = incognitoMode;
                                break;
                            default:
                                Log.Warning("[*] Configuration unknown values: {0}={1}.", trimmedKey, trimmedValue);
                                break;
                        }
                    }

                    if (!keys.Contains("ChannelName")
                        || !keys.Contains("MultiInstance")
                        || !keys.Contains("RobloxApi")
                        || !keys.Contains("RobloxCdnUri")
                        || !keys.Contains("Incognito"))
                        toUpdate = true;

                    if (toUpdate)
                        SaveConfiguration();

                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[*] Configuration loading exception: {0}", ex.Message);
            }

            SaveConfiguration();
        }
    }
}