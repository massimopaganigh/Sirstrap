using Serilog;

namespace Sirstrap.Core
{
    /// <summary>
    /// Manages application settings, providing functionality to load and save
    /// configuration to a file in the local application data directory.
    /// </summary>
    public static class SettingsManager
    {
        private static AppSettings _settings;
        private static readonly object _lock = new();

        /// <summary>
        /// Gets the path where the settings file is stored.
        /// </summary>
        /// <returns>
        /// The full path to the settings.ini file in the %localappdata%\Sirstrap directory.
        /// </returns>
        public static string GetSettingsFilePath()
        {
            string settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap");

            Directory.CreateDirectory(settingsDir);

            return Path.Combine(settingsDir, "settings.ini");
        }

        /// <summary>
        /// Gets the current application settings, loading from file if necessary.
        /// </summary>
        /// <returns>
        /// The current <see cref="AppSettings"/> instance.
        /// </returns>
        public static AppSettings GetSettings()
        {
            if (_settings == null)
            {
                lock (_lock)
                {
                    _settings ??= LoadSettings();
                }
            }

            return _settings;
        }

        /// <summary>
        /// Loads settings from the configuration file, creating default settings if the file doesn't exist.
        /// </summary>
        /// <returns>
        /// The loaded <see cref="AppSettings"/> instance, or default settings if loading fails.
        /// </returns>
        private static AppSettings LoadSettings()
        {
            string filePath = GetSettingsFilePath();
            var settings = new AppSettings();

            try
            {
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);

                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();

                        if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                        {
                            continue;
                        }

                        string[] parts = trimmedLine.Split('=', 2);

                        if (parts.Length != 2)
                        {
                            continue;
                        }

                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                        {
                            continue;
                        }

                        if (string.Equals(key, "RobloxCdnUrl", StringComparison.OrdinalIgnoreCase))
                        {
                            settings.RobloxCdnUrl = value;
                        }
                        if (string.Equals(key, "SirstrapUpdateChannel", StringComparison.OrdinalIgnoreCase))
                        {
                            settings.SirstrapUpdateChannel = value;
                        }
                    }

                    Log.Information("[*] Settings loaded from {0}", filePath);

                    return settings;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Error loading settings: {0}", ex.Message);
            }

            SaveSettings(settings);

            return settings;
        }

        /// <summary>
        /// Saves the current settings to the configuration file.
        /// </summary>
        /// <param name="settings">The settings to save.</param>
        /// <returns>
        /// <c>true</c> if the settings were successfully saved; otherwise, <c>false</c>.
        /// </returns>
        public static bool SaveSettings(AppSettings settings)
        {
            string filePath = GetSettingsFilePath();

            try
            {
                var lines = new List<string>
                {
                    $"RobloxCdnUrl={settings.RobloxCdnUrl}",
                    $"SirstrapUpdateChannel={settings.SirstrapUpdateChannel}"
                };

                File.WriteAllLines(filePath, lines);

                _settings = settings;

                Log.Information("[*] Settings saved to {0}", filePath);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error saving settings: {0}", ex.Message);

                return false;
            }
        }
    }
}