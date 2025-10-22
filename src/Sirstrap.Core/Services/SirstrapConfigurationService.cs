namespace Sirstrap.Core.Services
{
    public class SirstrapConfigurationService : ISirstrapConfigurationService
    {
        public void GetSettings(string? settingsPath = null)
        {
            try
            {
                settingsPath ??= GetSettingsPath();

                if (!File.Exists(settingsPath))
                    SetSettings();

                var rows = File.ReadAllLines(settingsPath);
                var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var row in rows)
                {
                    var trimmedRow = row.Trim();

                    if (string.IsNullOrEmpty(trimmedRow)
                        || trimmedRow.StartsWith('#'))
                        continue;

                    var trimmedRowParts = trimmedRow.Split('=', 2);

                    if (trimmedRowParts.Length != 2)
                        continue;

                    var trimmedRowKey = trimmedRowParts[0].Trim();
                    var trimmedRowValue = trimmedRowParts[1].Trim();

                    if (string.IsNullOrEmpty(trimmedRowKey))
                        continue;

                    keys.Add(trimmedRowKey);

                    if (string.IsNullOrEmpty(trimmedRowValue))
                        continue;

                    _ = trimmedRowKey switch
                    {
                        "AutoUpdate" => Set(() =>
                        {
                            if (bool.TryParse(trimmedRowValue, out var v))
                                SirstrapConfiguration.AutoUpdate = v;
                        }),
                        "ChannelName" => Set(() => SirstrapConfiguration.ChannelName = trimmedRowValue),
                        "MultiInstance" => Set(() =>
                        {
                            if (bool.TryParse(trimmedRowValue, out var v))
                                SirstrapConfiguration.MultiInstance = v;
                        }),
                        "Incognito" => Set(() =>
                        {
                            if (bool.TryParse(trimmedRowValue, out var v))
                                SirstrapConfiguration.Incognito = v;
                        }),
                        "RobloxApi" => Set(() =>
                        {
                            if (bool.TryParse(trimmedRowValue, out var v))
                                SirstrapConfiguration.RobloxApi = v;
                        }),
                        "RobloxCdnUri" => Set(() => SirstrapConfiguration.RobloxCdnUri = trimmedRowValue),
                        "SirHurtPath" => Set(() => SirstrapConfiguration.SirHurtPath = trimmedRowValue),
                        _ => Set(() => Log.Warning("[*] Configuration unknown values: {0}={1}.", trimmedRowKey, trimmedRowValue))
                    };
                }

                if (keys.Count != 7)
                {
                    SetSettings();

                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting settings: {0}.", ex.Message);

                throw;
            }
        }

        public void SetSettings(string? settingsPath = null)
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
                    $"MultiInstance={SirstrapConfiguration.MultiInstance}",
                    $"Incognito={SirstrapConfiguration.Incognito}",
                    $"RobloxApi={SirstrapConfiguration.RobloxApi}",
                    $"RobloxCdnUri={SirstrapConfiguration.RobloxCdnUri}",
                    $"SirHurtPath={SirstrapConfiguration.SirHurtPath}"
                });

                GetSettings();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error setting settings: {0}.", ex.Message);

                throw;
            }
        }

        #region PRIVATE METHODS
        private static string GetSettingsPath()
        {
            var settingsPath = Directories.SirstrapDirectory;

            settingsPath.BetterDirectoryCreate();

            return Path.Combine(settingsPath, "settings.ini");
        }

        private static object? Set(Action action)
        {
            action();

            return null;
        }
        #endregion
    }
}
