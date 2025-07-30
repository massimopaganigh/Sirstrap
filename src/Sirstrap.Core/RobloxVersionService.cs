namespace Sirstrap.Core
{
    public class RobloxVersionService(HttpClient httpClient)
    {
        private const string ROBLOX_API_URI = "https://clientsettingscdn.roblox.com/v1/client-version/WindowsPlayer";
        private const string SIRHURT_API_URI = "https://sirhurt.net/status/fetch.php?exploit=SirHurt%20V5";

        private readonly HttpClient _httpClient = httpClient;

        private async Task<string> GetRobloxVersionAsync()
        {
            try
            {
                string response = await _httpClient.GetStringAsync(ROBLOX_API_URI);

                using JsonDocument jsonDocument = JsonDocument.Parse(response);

                if (jsonDocument.RootElement.TryGetProperty("clientVersionUpload", out var version))
                    return version.GetString() ?? string.Empty;

                Log.Error("[!] clientVersionUpload field not found in JSON response.");

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting Roblox version from API: {0}", ex.Message);

                return string.Empty;
            }
        }

        private async Task<(string version, bool isOutdated)> GetSirhurtVersionAsync()
        {
            try
            {
                string response = await _httpClient.GetStringAsync(SIRHURT_API_URI);

                using JsonDocument jsonDocument = JsonDocument.Parse(response);

                if (jsonDocument.RootElement.TryGetProperty("SirHurt V5", out var sirhurt))
                {
                    string version = string.Empty;
                    bool isOutdated = false;

                    if (sirhurt.TryGetProperty("roblox_version", out var versionElement))
                        version = versionElement.GetString() ?? string.Empty;

                    if (sirhurt.TryGetProperty("last_update_unix", out var lastUpdateElement))
                    {
                        long lastUpdateUnix = lastUpdateElement.GetInt64();
                        DateTimeOffset lastUpdate = DateTimeOffset.FromUnixTimeSeconds(lastUpdateUnix);
                        TimeSpan timeSinceUpdate = DateTimeOffset.UtcNow - lastUpdate;

                        isOutdated = timeSinceUpdate.TotalDays > 14;
                    }

                    if (string.IsNullOrEmpty(version))
                    {
                        Log.Error("[!] roblox_version field not found in JSON response.");

                        return (string.Empty, false);
                    }

                    return (version, isOutdated);
                }

                Log.Error("[!] SirHurt V5 field not found in JSON response.");

                return (string.Empty, false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting SirHurt version from API: {0}", ex.Message);

                return (string.Empty, false);
            }
        }

        public async Task<string> GetLatestVersionAsync()
        {
            string version;

            if (SirstrapConfiguration.RobloxApi)
            {
                Log.Information("[*] Roblox API is enabled, using Roblox API to retrieve version...");

                version = await GetRobloxVersionAsync();

                if (string.IsNullOrWhiteSpace(version))
                    Log.Error("[!] Failed to retrieve version.");
                else
                    Log.Information("[*] Using version: {0}.", version);

                return version;
            }
            else
            {
                Log.Information("[*] Roblox API is disabled, using SirHurt API to retrieve version.");

                var (sirhurtVersion, isOutdated) = await GetSirhurtVersionAsync();

                if (string.IsNullOrEmpty(sirhurtVersion))
                {
                    Log.Error("[!] Failed to retrieve version from SirHurt API, using Roblox API to retrieve version...");

                    version = await GetRobloxVersionAsync();

                    if (string.IsNullOrEmpty(version))
                        Log.Error("[!] Failed to retrieve version.");
                    else
                        Log.Information("[*] Using version: {0}.", version);
                }
                else if (isOutdated)
                {
                    Log.Warning("[!] SirHurt hasn't updated in more than 3 days, falling back to Roblox API...");

                    version = await GetRobloxVersionAsync();

                    if (string.IsNullOrEmpty(version))
                    {
                        Log.Error("[!] Failed to retrieve version from Roblox API, using outdated SirHurt version...");

                        version = sirhurtVersion;

                        Log.Information("[*] Using version: {0}.", version);
                    }
                    else
                        Log.Information("[*] Using version: {0}.", version);
                }
                else
                {
                    version = sirhurtVersion;

                    Log.Information("[*] Using version: {0}.", version);
                }
            }

            return version;
        }
    }
}