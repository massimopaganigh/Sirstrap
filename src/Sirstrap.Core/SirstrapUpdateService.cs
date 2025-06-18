namespace Sirstrap.Core
{
    public class SirstrapUpdateService
    {
        private const string SIRSTRAP_API = "https://api.github.com/repos/massimopaganigh/sirstrap/releases";
        private const string SIRSTRAP_CURRENT_VERSION = "1.1.8.2";

        private readonly HttpClient _httpClient;

        public SirstrapUpdateService()
        {
            _httpClient = new()
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Sirstrap");
        }

        private async Task<bool> DownloadAndApplyUpdateAsync(SirstrapType sirstrapType, string[] args)
        {
            try
            {
                var (_, _, downloadUrl) = await GetLatestVersionChannelAndDownloadUriAsync(sirstrapType);

                if (string.IsNullOrEmpty(downloadUrl))
                    throw new Exception($"{nameof(GetLatestVersionChannelAndDownloadUriAsync)} failed.");

                string updateDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Update");

                if (Directory.Exists(updateDirectory))
                {
                    Log.Information("[*] Cleaning {0}...", updateDirectory);

                    try
                    {
                        Directory.Delete(updateDirectory, recursive: true);
                        Directory.CreateDirectory(updateDirectory);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[!] Error during the cleaning of {0}...", updateDirectory);
                    }
                }
                else
                {
                    Log.Information("[*] Creating {0}...", updateDirectory);
                    Directory.CreateDirectory(updateDirectory);
                }

                Log.Information("[*] Downloading update from {0}...", downloadUrl);

                byte[] zipData = await _httpClient.GetByteArrayAsync(downloadUrl);
                string zipPath = Path.Combine(updateDirectory, "Sirstrap.zip");

                await File.WriteAllBytesAsync(zipPath, zipData);

                ZipFile.ExtractToDirectory(zipPath, updateDirectory, overwriteFiles: true);
                File.Delete(zipPath);

                var exePath = Process.GetCurrentProcess().MainModule?.FileName ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sirstrap.exe");
                var exeDirectory = Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory;
                var batchPath = Path.Combine(updateDirectory, "update.bat");
                string arguments = string.Empty;

                if (args != null
                    && args.Length > 0)
                {
                    var escapedArgs = args.Select(arg => arg.Contains(' ') ? $"\"{arg.Replace("\"", "\"\"")}\"" : arg);

                    arguments = " " + string.Join(" ", escapedArgs);
                }
                
                var batchContent = $@"
@echo off
echo Updating Sirstrap...
timeout /t 2 /nobreak >nul
xcopy ""{updateDirectory}\*"" ""{exeDirectory}"" /E /Y
start """" ""{Path.Combine(exeDirectory, "Sirstrap.exe")}""{arguments}
exit
";

                await File.WriteAllTextAsync(batchPath, batchContent);

                ProcessStartInfo updateBatStartInfo = new()
                {
                    FileName = batchPath,
                    CreateNoWindow = true,
                    UseShellExecute = true
                };

                Log.Information("[*] Applying update to {0}...", exeDirectory);
                Process.Start(updateBatStartInfo);
                Environment.Exit(0);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(DownloadAndApplyUpdateAsync));

                return false;
            }
        }

        private static string GetCurrentChannel() => AppSettingsManager.GetSettings().SirstrapUpdateChannel;

        private static Version GetCurrentVersion() => new(SIRSTRAP_CURRENT_VERSION);

        private async Task<(Version latestVersion, string latestChannel, string latestDownloadUri)> GetLatestVersionChannelAndDownloadUriAsync(SirstrapType sirstrapType)
        {
            try
            {
                string response = await _httpClient.GetStringAsync(SIRSTRAP_API);
                JsonDocument jsonDocument = JsonDocument.Parse(response);
                JsonElement rootElement = jsonDocument.RootElement;
                Version latestVersion = new("0.0.0.0");
                string latestChannel = string.Empty;
                string latestDownloadUri = string.Empty;

                foreach (JsonElement jsonElement in rootElement.EnumerateArray())
                {
                    bool draft = false;

                    if (jsonElement.TryGetProperty("draft", out JsonElement draftElement))
                        draft = draftElement.GetBoolean();

                    if (draft)
                        continue;

                    string tagName = string.Empty;

                    if (jsonElement.TryGetProperty("tag_name", out JsonElement tagNameElement))
                        tagName = tagNameElement.GetString() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(tagName))
                        continue;

                    string[] tagParts = tagName.Split('-');
                    string versionPart = tagParts[0].TrimStart('v');
                    string channelPart = tagParts.Length > 1
                        ? $"-{tagParts[1]}"
                        : string.Empty;

                    if (!Version.TryParse(versionPart, out Version? version))
                        continue;

                    if (!string.Equals(channelPart, GetCurrentChannel(), StringComparison.OrdinalIgnoreCase))
                        continue;

                    var downloadUri = string.Empty;

                    if (jsonElement.TryGetProperty("assets", out JsonElement assetsElement))
                    {
                        foreach (JsonElement assetElement in assetsElement.EnumerateArray())
                        {
                            var name = string.Empty;

                            if (assetElement.TryGetProperty("name", out JsonElement nameElement))
                                name = nameElement.GetString() ?? string.Empty;

                            if (sirstrapType == SirstrapType.CLI
                                && name.Equals("Sirstrap.CLI.zip", StringComparison.OrdinalIgnoreCase))
                            {
                                if (assetElement.TryGetProperty("browser_download_url", out JsonElement browserDownloadUrlElement))
                                {
                                    downloadUri = browserDownloadUrlElement.GetString() ?? string.Empty;

                                    break;
                                }
                            }
                            else if (sirstrapType == SirstrapType.UI
                                && name.Equals("Sirstrap.UI.zip", StringComparison.OrdinalIgnoreCase))
                                if (assetElement.TryGetProperty("browser_download_url", out JsonElement browserDownloadUrlElement))
                                {
                                    downloadUri = browserDownloadUrlElement.GetString() ?? string.Empty;

                                    break;
                                }
                        }
                    }

                    if (version > latestVersion)
                    {
                        latestVersion = version;
                        latestChannel = channelPart;
                        latestDownloadUri = downloadUri;
                    }
                }

                return (latestVersion, latestChannel, latestDownloadUri);
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetLatestVersionChannelAndDownloadUriAsync));

                return (new Version("0.0.0.0"), string.Empty, string.Empty);
            }
        }

        private async Task<bool> IsUpToDateAsync(SirstrapType sirstrapType)
        {
            try
            {
                Version currentVersion = GetCurrentVersion();
                string currentChannel = GetCurrentChannel();

                var (latestVersion, latestChannel, _) = await GetLatestVersionChannelAndDownloadUriAsync(sirstrapType);

                if (latestVersion.Major == 0
                    && latestVersion.Minor == 0
                    && latestVersion.Build == 0
                    && latestVersion.Revision == 0
                    || string.IsNullOrWhiteSpace(latestChannel))
                    throw new Exception($"{nameof(GetLatestVersionChannelAndDownloadUriAsync)} failed.");

                if (latestVersion > currentVersion)
                {
                    Log.Information("[*] Updating v{0}{1} to v{2}{3}...", currentVersion, currentChannel, latestVersion, latestChannel);

                    return false;
                }

                Log.Information("[*] Up to date: v{0}{1}.", currentVersion, currentChannel);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(IsUpToDateAsync));

                return true;
            }
        }

        /// <summary>
        /// Gets the current full version of Sirstrap.
        /// </summary>
        /// <returns>The current full version of Sirstrap.</returns>
        public static string GetCurrentFullVersion() => $"v{GetCurrentVersion()}{GetCurrentChannel()}";

        /// <summary>
        /// Updates the Sirstrap application to the latest version.
        /// </summary>
        /// <param name="sirstrapType">The type of Sirstrap to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(SirstrapType sirstrapType, string[] args)
        {
            try
            {
                if (!await IsUpToDateAsync(sirstrapType))
                    await DownloadAndApplyUpdateAsync(sirstrapType, args);
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(UpdateAsync));
            }
        }
    }
}