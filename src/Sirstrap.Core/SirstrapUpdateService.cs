namespace Sirstrap.Core
{
    public class SirstrapUpdateService : IDisposable
    {
        private const string SIRSTRAP_API = "https://api.github.com/repos/massimopaganigh/sirstrap/releases";
        private const string SIRSTRAP_CURRENT_VERSION = "1.1.8.9";
        private const string UPDATE_FOLDER_NAME = "Update";
        private const string SIRSTRAP_ZIP_FILENAME = "Sirstrap.zip";
        private const string UPDATE_BATCH_FILENAME = "update.bat";
        private const string SIRSTRAP_EXE_FILENAME = "Sirstrap.exe";
        private const string CLI_ZIP_FILENAME = "Sirstrap.CLI.zip";
        private const string UI_ZIP_FILENAME = "Sirstrap.UI.zip";
        private const string USER_AGENT = "Sirstrap";
        private const int HTTP_TIMEOUT_MINUTES = 5;

        private readonly HttpClient _httpClient;

        public SirstrapUpdateService()
        {
            _httpClient = new()
            {
                Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT_MINUTES)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
        }

        private static string PrepareUpdateDirectory()
        {
            string updateDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", UPDATE_FOLDER_NAME);

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

            return updateDirectory;
        }

        private async Task DownloadAndExtractUpdateAsync(string downloadUrl, string updateDirectory)
        {
            Log.Information("[*] Downloading update from {0}...", downloadUrl);

            byte[] zipData = await _httpClient.GetByteArrayAsync(downloadUrl);
            string zipPath = Path.Combine(updateDirectory, SIRSTRAP_ZIP_FILENAME);

            await File.WriteAllBytesAsync(zipPath, zipData);

            ZipFile.ExtractToDirectory(zipPath, updateDirectory, overwriteFiles: true);
            File.Delete(zipPath);
        }

        private static string GetCurrentExecutablePath()
        {
            return Process.GetCurrentProcess().MainModule?.FileName ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SIRSTRAP_EXE_FILENAME);
        }

        private static string BuildArgumentsString(string[] args)
        {
            if (args == null || args.Length == 0)
                return string.Empty;

            var escapedArgs = args.Select(arg => arg.Contains(' ') ? $"\"{arg.Replace("\"", "\"\"")}\"" : arg);
            return " " + string.Join(" ", escapedArgs);
        }

        private static async Task CreateAndExecuteUpdateBatchAsync(string updateDirectory, string exeDirectory, string exePath, string[] args)
        {
            var batchPath = Path.Combine(updateDirectory, UPDATE_BATCH_FILENAME);
            string arguments = BuildArgumentsString(args);

            var batchContent = $@"
@echo off
echo Updating Sirstrap...
timeout /t 2 /nobreak >nul
xcopy ""{updateDirectory}\*"" ""{exeDirectory}"" /E /Y
start """" ""{Path.Combine(exeDirectory, SIRSTRAP_EXE_FILENAME)}""{arguments}
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
        }

        private async Task<bool> DownloadAndApplyUpdateAsync(SirstrapType sirstrapType, string[] args)
        {
            try
            {
                var (_, _, downloadUrl) = await GetLatestVersionChannelAndDownloadUriAsync(sirstrapType);

                if (string.IsNullOrEmpty(downloadUrl))
                    throw new Exception($"{nameof(GetLatestVersionChannelAndDownloadUriAsync)} failed.");

                string updateDirectory = PrepareUpdateDirectory();
                await DownloadAndExtractUpdateAsync(downloadUrl, updateDirectory);
                
                var exePath = GetCurrentExecutablePath();
                var exeDirectory = Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory;
                
                await CreateAndExecuteUpdateBatchAsync(updateDirectory, exeDirectory, exePath, args);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(DownloadAndApplyUpdateAsync));

                return false;
            }
        }

        private static string GetCurrentChannel() => SirstrapConfiguration.ChannelName;

        private static Version GetCurrentVersion() => new(SIRSTRAP_CURRENT_VERSION);

        private static bool IsReleaseDraft(JsonElement release)
        {
            if (release.TryGetProperty("draft", out JsonElement draftElement))
                return draftElement.GetBoolean();
            return false;
        }

        private static string GetReleaseTagName(JsonElement release)
        {
            if (release.TryGetProperty("tag_name", out JsonElement tagNameElement))
                return tagNameElement.GetString() ?? string.Empty;
            return string.Empty;
        }

        private static (string versionPart, string channelPart) ParseTagName(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return (string.Empty, string.Empty);

            string[] tagParts = tagName.Split('-');
            string versionPart = tagParts[0].TrimStart('v');
            string channelPart = tagParts.Length > 1 ? $"-{tagParts[1]}" : string.Empty;
            return (versionPart, channelPart);
        }

        private static string FindAssetDownloadUrl(JsonElement release, SirstrapType sirstrapType)
        {
            if (!release.TryGetProperty("assets", out JsonElement assetsElement))
                return string.Empty;

            string targetFileName = sirstrapType == SirstrapType.CLI ? CLI_ZIP_FILENAME : UI_ZIP_FILENAME;

            foreach (JsonElement assetElement in assetsElement.EnumerateArray())
            {
                if (assetElement.TryGetProperty("name", out JsonElement nameElement))
                {
                    string name = nameElement.GetString() ?? string.Empty;
                    if (name.Equals(targetFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (assetElement.TryGetProperty("browser_download_url", out JsonElement urlElement))
                            return urlElement.GetString() ?? string.Empty;
                    }
                }
            }

            return string.Empty;
        }

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
                    if (IsReleaseDraft(jsonElement))
                        continue;

                    string tagName = GetReleaseTagName(jsonElement);
                    if (string.IsNullOrWhiteSpace(tagName))
                        continue;

                    var (versionPart, channelPart) = ParseTagName(tagName);
                    if (!Version.TryParse(versionPart, out Version? version))
                        continue;

                    if (!string.Equals(channelPart, GetCurrentChannel(), StringComparison.OrdinalIgnoreCase))
                        continue;

                    string downloadUri = FindAssetDownloadUrl(jsonElement, sirstrapType);

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

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}