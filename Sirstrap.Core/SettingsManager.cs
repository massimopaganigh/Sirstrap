using Serilog;

namespace Sirstrap.Core
{
    /// <summary>
    /// Manages application settings, providing functionality to load and save
    /// configuration to a persistent file in the local application data directory.
    /// </summary>
    /// <remarks>
    /// The SettingsManager implements a singleton pattern to ensure that settings
    /// are loaded only once and then cached for future access. Settings are stored
    /// in a simple INI-style format in the user's local application data folder.
    /// 
    /// The class handles parsing of multiple setting types including strings and booleans,
    /// and provides default values when settings are missing or invalid.
    /// </remarks>
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
        /// <remarks>
        /// This method ensures that the Sirstrap directory exists in the user's
        /// local application data folder before returning the settings file path.
        /// </remarks>
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
        /// The current <see cref="AppSettings"/> instance, either from cache or freshly loaded.
        /// </returns>
        /// <remarks>
        /// This method implements a thread-safe lazy loading pattern. Settings are loaded
        /// from disk only once, then cached for subsequent accesses. The method is thread-safe
        /// through the use of a lock object to prevent multiple concurrent initializations.
        /// </remarks>
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
        /// <remarks>
        /// This method reads the settings file and parses it line by line, extracting key-value pairs.
        /// It handles multiple setting types:
        /// - String values are assigned directly
        /// - Boolean values are parsed using bool.TryParse
        /// 
        /// The method is resilient to:
        /// - Missing or invalid settings files (creates defaults)
        /// - Malformed lines (skips them)
        /// - Comments (lines starting with #)
        /// - Empty lines
        /// 
        /// If any error occurs during loading, default settings are used and saved to disk.
        /// </remarks>
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
                        else if (string.Equals(key, "SirstrapUpdateChannel", StringComparison.OrdinalIgnoreCase))
                        {
                            settings.SirstrapUpdateChannel = value;
                        }
                        else if (string.Equals(key, "SafeMode", StringComparison.OrdinalIgnoreCase))
                        {
                            if (bool.TryParse(value, out bool safeMode))
                            {
                                settings.SafeMode = safeMode;
                            }
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
        /// <remarks>
        /// This method writes all settings to the configuration file in a simple key=value format.
        /// Each setting is written on a separate line. The method handles both string and boolean values.
        /// 
        /// If successful, the internal settings cache is updated with the saved settings.
        /// If an error occurs during saving, it is logged but not propagated to the caller.
        /// </remarks>
        public static bool SaveSettings(AppSettings settings)
        {
            string filePath = GetSettingsFilePath();

            try
            {
                var lines = new List<string>
                {
                    $"RobloxCdnUrl={settings.RobloxCdnUrl}",
                    $"SirstrapUpdateChannel={settings.SirstrapUpdateChannel}",
                    $"SafeMode={settings.SafeMode}"
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