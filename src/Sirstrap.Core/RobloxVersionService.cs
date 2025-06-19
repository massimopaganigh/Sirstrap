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

        private async Task<string> GetSirhurtVersionAsync()
        {
            try
            {
                string response = await _httpClient.GetStringAsync(SIRHURT_API_URI);

                using JsonDocument jsonDocument = JsonDocument.Parse(response);

                if (jsonDocument.RootElement.EnumerateArray().FirstOrDefault().TryGetProperty("SirHurt V5", out var sirhurt))
                {
                    if (sirhurt.TryGetProperty("roblox_version", out var version))
                        return version.GetString() ?? string.Empty;

                    Log.Error("[!] roblox_version field not found in JSON response.");

                    return string.Empty;
                }

                Log.Error("[!] SirHurt V5 field not found in JSON response.");

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting SirHurt version from API: {0}", ex.Message);

                return string.Empty;
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

                version = await GetSirhurtVersionAsync();

                if (string.IsNullOrEmpty(version))
                {
                    Log.Error("[!] Failed to retrieve version, using Roblox API to retrieve version...");

                    version = await GetRobloxVersionAsync();

                    if (string.IsNullOrEmpty(version))
                        Log.Error("[!] Failed to retrieve version.");
                }
                else
                    Log.Information("[*] Using version: {0}.", version);
            }

            return version;
        }
    }
}