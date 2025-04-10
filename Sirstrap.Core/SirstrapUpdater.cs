using Serilog;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace Sirstrap.Core
{
    /// <summary>
    /// Provides functionality to check for and install updates to the Sirstrap application.
    /// </summary>
    public class SirstrapUpdater
    {
        private const string SIRSTRAP_API = "https://api.github.com/repos/massimopaganigh/sirstrap/releases";

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SirstrapUpdater"/> class.
        /// </summary>
        public SirstrapUpdater()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Sirstrap");
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Checks for updates and installs them if available.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CheckAndInstallUpdateAsync()
        {
            try
            {
                if (await CheckForUpdatesAsync().ConfigureAwait(false))
                {
                    Log.Information("[*] Update is available. Starting update process...");

                    await DownloadAndInstallUpdateAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error checking and installing update: {0}.", ex.Message);
            }
        }

        /// <summary>
        /// Checks for updates by comparing the current version with the latest version on GitHub.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result is <c>true</c> if an update is available; otherwise, <c>false</c>.</returns>
        private async Task<bool> CheckForUpdatesAsync()
        {
            try
            {
                Log.Information("[*] Checking for Sirstrap updates...");

                var currentVersion = GetCurrentVersion();
                var currentChannel = GetCurrentChannel();

                Log.Information("[*] Current version: {0}, channel: {1}.", currentVersion, currentChannel);

                var (latestVersion, latestChannel, downloadUrl) = await GetLatestVersionInfoAsync(currentChannel).ConfigureAwait(false);

                if (latestVersion.Major == 0 && latestVersion.Minor == 0 && latestVersion.Build == 0 && latestVersion.Revision == 0)
                {
                    Log.Warning("[!] Could not retrieve latest version information.");

                    return false;
                }

                Log.Information("[*] Latest version: {0}, channel: {1}.", latestVersion, latestChannel);

                bool updateAvailable = IsUpdateAvailable(currentVersion, latestVersion);

                if (updateAvailable)
                {
                    Log.Information("[*] Update available: {0} -> {1}.", currentVersion, latestVersion);
                }
                else
                {
                    Log.Information("[*] No update available.");
                }

                return updateAvailable;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error checking for updates: {0}.", ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Downloads and installs the latest update from GitHub.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result is <c>true</c> if the update was successful; otherwise, <c>false</c>.</returns>
        private async Task<bool> DownloadAndInstallUpdateAsync()
        {
            try
            {
                var currentChannel = GetCurrentChannel();
                var (_, _, downloadUrl) = await GetLatestVersionInfoAsync(currentChannel).ConfigureAwait(false);

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    Log.Error("[!] Could not find download URL for update.");

                    return false;
                }

                var sirstrapDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap");
                var updateDirectory = Path.Combine(sirstrapDirectory, "Update");

                if (Directory.Exists(updateDirectory))
                {
                    Log.Information("[*] Cleaning update directory...");

                    foreach (var file in Directory.GetFiles(updateDirectory))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    Log.Information("[*] Creating update directory...");
                    Directory.CreateDirectory(updateDirectory);
                }

                Log.Information("[*] Downloading update from {0}...", downloadUrl);

                var zipData = await _httpClient.GetByteArrayAsync(downloadUrl).ConfigureAwait(false);
                var zipPath = Path.Combine(updateDirectory, "Sirstrap.zip");

                await File.WriteAllBytesAsync(zipPath, zipData).ConfigureAwait(false);

                Log.Information("[*] Extracting update to {0}...", updateDirectory);
                ZipFile.ExtractToDirectory(zipPath, updateDirectory, overwriteFiles: true);
                File.Delete(zipPath);

                var currentExePath = Process.GetCurrentProcess().MainModule?.FileName ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sirstrap.exe");
                var currentDirectory = Path.GetDirectoryName(currentExePath) ?? AppDomain.CurrentDomain.BaseDirectory;
                var batchPath = Path.Combine(updateDirectory, "update.bat");
                var batchContent = $@"
@echo off
echo Updating Sirstrap...
timeout /t 2 /nobreak >nul
xcopy ""{updateDirectory}\*"" ""{currentDirectory}"" /E /Y
start """" ""{Path.Combine(currentDirectory, "Sirstrap.exe")}""
exit
";

                await File.WriteAllTextAsync(batchPath, batchContent).ConfigureAwait(false);

                ProcessStartInfo updateBatStartInfo = new()
                {
                    FileName = batchPath,
                    CreateNoWindow = true,
                    UseShellExecute = true
                };

                Log.Information("[*] Starting update process and terminating current instance...");
                Process.Start(updateBatStartInfo);
                Environment.Exit(0);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error downloading and installing update: {0}.", ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Gets the current full version of Sirstrap.
        /// </summary>
        /// <returns>The current version of Sirstrap.</returns>
        public static string GetCurrentFullVersion()
        {
            return $"v{GetCurrentVersion()}{GetCurrentChannel()}";
        }

        /// <summary>
        /// Gets the current version of Sirstrap.
        /// </summary>
        /// <returns>The current version of Sirstrap.</returns>
        private static Version GetCurrentVersion()
        {
            return new Version("1.1.5.3");
        }

        /// <summary>
        /// Gets the current update channel from settings.
        /// </summary>
        /// <returns>The current update channel.</returns>
        private static string GetCurrentChannel()
        {
            return SettingsManager.GetSettings().SirstrapUpdateChannel;
        }

        /// <summary>
        /// Gets the latest version information from GitHub for a specific channel.
        /// </summary>
        /// <param name="targetChannel">The channel to look for.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the latest version, channel, and download URL.</returns>
        private async Task<(Version version, string channel, string downloadUrl)> GetLatestVersionInfoAsync(string targetChannel)
        {
            try
            {
                var jsonResponse = await _httpClient.GetStringAsync(SIRSTRAP_API).ConfigureAwait(false);
                var jsonDocument = JsonDocument.Parse(jsonResponse);
                var rootElement = jsonDocument.RootElement;
                var latestVersion = new Version("0.0.0.0");
                var latestChannel = string.Empty;
                var latestDownloadUrl = string.Empty;

                foreach (JsonElement release in rootElement.EnumerateArray())
                {
                    var isDraft = false;

                    if (release.TryGetProperty("draft", out JsonElement draftElement))
                    {
                        isDraft = draftElement.GetBoolean();
                    }

                    if (isDraft)
                    {
                        continue;
                    }

                    var tagName = string.Empty;

                    if (release.TryGetProperty("tag_name", out JsonElement tagElement))
                    {
                        tagName = tagElement.GetString() ?? string.Empty;
                    }

                    if (string.IsNullOrEmpty(tagName))
                    {
                        continue;
                    }

                    var tagParts = tagName.Split('-');
                    var versionStr = tagParts[0].TrimStart('v');
                    var channel = tagParts.Length > 1 ? $"-{tagParts[1]}" : string.Empty;

                    if (!string.Equals(channel, targetChannel, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!Version.TryParse(versionStr, out Version? releaseVersion))
                    {
                        continue;
                    }

                    var downloadUrl = string.Empty;

                    if (release.TryGetProperty("assets", out JsonElement assetsElement))
                    {
                        foreach (var asset in assetsElement.EnumerateArray())
                        {
                            var name = string.Empty;

                            if (asset.TryGetProperty("name", out JsonElement nameElement))
                            {
                                name = nameElement.GetString() ?? string.Empty;
                            }

                            if (name.Equals("Sirstrap.UI.zip", StringComparison.OrdinalIgnoreCase))
                            {
                                if (asset.TryGetProperty("browser_download_url", out JsonElement urlElement))
                                {
                                    downloadUrl = urlElement.GetString() ?? string.Empty;
                                    break;
                                }
                            }
                        }
                    }

                    if (releaseVersion > latestVersion && !string.IsNullOrEmpty(downloadUrl))
                    {
                        latestVersion = releaseVersion;
                        latestChannel = channel;
                        latestDownloadUrl = downloadUrl;
                    }
                }

                return (latestVersion, latestChannel, latestDownloadUrl);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting latest version info: {0}.", ex.Message);

                return (new Version("0.0.0.0"), string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Determines whether an update is available by comparing versions.
        /// </summary>
        /// <param name="currentVersion">The current version of Sirstrap.</param>
        /// <param name="latestVersion">The latest version available on GitHub.</param>
        /// <returns><c>true</c> if an update is available; otherwise, <c>false</c>.</returns>
        private static bool IsUpdateAvailable(Version currentVersion, Version latestVersion)
        {
            return latestVersion > currentVersion;
        }
    }
}