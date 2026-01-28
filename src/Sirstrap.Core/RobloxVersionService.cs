namespace Sirstrap.Core
{
    public class RobloxVersionService(HttpClient httpClient)
    {
        private const string ROBLOX_API_URI = "https://clientsettingscdn.roblox.com/v1/client-version/WindowsPlayer";
        private const string SIRHURT_API_URI = "https://sirhurt.net/status/fetch.php?exploit=SirHurt%20V5";
        private const string WEAO_API_URI = "https://v3.weao.xyz/documentation/status-exploits";
        private const string WEAO_USER_AGENT = "WEAO-3PService";

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

                if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array && jsonDocument.RootElement.GetArrayLength() > 0)
                {
                    JsonElement firstElement = jsonDocument.RootElement[0];

                    if (firstElement.TryGetProperty("SirHurt V5", out var sirhurt))
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

                            isOutdated = timeSinceUpdate.TotalDays > 10;
                        }

                        if (string.IsNullOrEmpty(version))
                        {
                            Log.Error("[!] roblox_version field not found in JSON response.");

                            return (string.Empty, false);
                        }

                        return (version, isOutdated);
                    }

                    Log.Error("[!] SirHurt V5 field not found in JSON response.");
                }
                else
                {
                    Log.Error("[!] API returned unexpected JSON structure (expected array).");
                }

                return (string.Empty, false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting SirHurt version from API: {0}", ex.Message);

                return (string.Empty, false);
            }
        }

        private async Task<(string version, bool isOutdated)> GetWeaoVersionAsync(string exploitName)
        {
            try
            {
                // Note: The exploit names must match exactly with the JSON property names
                // returned by the weao.xyz API. The API returns an array of objects,
                // where each element contains exploit data as properties.
                // API requires User-Agent: WEAO-3PService header
                var request = new HttpRequestMessage(HttpMethod.Get, WEAO_API_URI);
                request.Headers.Add("User-Agent", WEAO_USER_AGENT);
                
                var httpResponse = await _httpClient.SendAsync(request);
                httpResponse.EnsureSuccessStatusCode();
                
                string response = await httpResponse.Content.ReadAsStringAsync();

                using JsonDocument jsonDocument = JsonDocument.Parse(response);

                if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array && jsonDocument.RootElement.GetArrayLength() > 0)
                {
                    // Iterate through all array elements to find the exploit
                    foreach (JsonElement element in jsonDocument.RootElement.EnumerateArray())
                    {
                        if (element.TryGetProperty(exploitName, out var exploit))
                        {
                            string version = string.Empty;
                            bool isOutdated = false;

                            if (exploit.TryGetProperty("roblox_version", out var versionElement))
                                version = versionElement.GetString() ?? string.Empty;

                            if (exploit.TryGetProperty("last_update_unix", out var lastUpdateElement))
                            {
                                long lastUpdateUnix = lastUpdateElement.GetInt64();
                                DateTimeOffset lastUpdate = DateTimeOffset.FromUnixTimeSeconds(lastUpdateUnix);
                                TimeSpan timeSinceUpdate = DateTimeOffset.UtcNow - lastUpdate;

                                isOutdated = timeSinceUpdate.TotalDays > 10;
                            }

                            if (string.IsNullOrEmpty(version))
                            {
                                Log.Error("[!] roblox_version field not found in JSON response for {0}.", exploitName);

                                return (string.Empty, false);
                            }

                            return (version, isOutdated);
                        }
                    }

                    Log.Error("[!] {0} field not found in JSON response.", exploitName);
                }
                else
                {
                    Log.Error("[!] API returned unexpected JSON structure (expected array).");
                }

                return (string.Empty, false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting {0} version from Weao API: {1}", exploitName, ex.Message);

                return (string.Empty, false);
            }
        }

        private async Task<bool> ValidateVersion(string version)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(version))
                    return false;

                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, UriBuilder.GetManifestUri(new Configuration
                {
                    ChannelName = SirstrapConfiguration.ChannelName,
                    VersionHash = version
                })));

                if (response.IsSuccessStatusCode)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetLatestVersionAsync()
        {
            string version;

            if (!string.IsNullOrWhiteSpace(SirstrapConfiguration.RobloxVersionOverride))
            {
                Log.Information("[*] Roblox version override is set, using Roblox version override to retrieve version...");

                version = SirstrapConfiguration.RobloxVersionOverride;

                Log.Information("[*] Using version: {0}.", version);

                return version;

                //                if (!await ValidateVersion(version))
                //                {
                //                    Log.Error("[!] Validation failed. Invalid Roblox version override.");

                //#pragma warning disable IDE0059 // Assegnazione non necessaria di un valore
                //                    version = string.Empty;
                //#pragma warning restore IDE0059 // Assegnazione non necessaria di un valore
                //                }
                //                else
                //                {
                //                    Log.Information("[*] Using version: {0}.", version);

                //                    return version;
                //                }
            }

            if (SirstrapConfiguration.RobloxApi)
            {
                Log.Information("[*] Roblox API is enabled, using Roblox API to retrieve version...");

                version = await GetRobloxVersionAsync();

                if (string.IsNullOrWhiteSpace(version))
                    Log.Error("[!] Failed to retrieve version.");
            }
            else
            {
                Log.Information("[*] Roblox API is disabled, using exploit API to retrieve version.");

                var (exploitVersion, isOutdated) = SirstrapConfiguration.ExploitName == "SirHurt V5"
                    ? await GetSirhurtVersionAsync()
                    : await GetWeaoVersionAsync(SirstrapConfiguration.ExploitName);

                if (string.IsNullOrEmpty(exploitVersion))
                {
                    Log.Error("[!] Failed to retrieve version from {0} API, using Roblox API to retrieve version...", SirstrapConfiguration.ExploitName);

                    version = await GetRobloxVersionAsync();

                    if (string.IsNullOrEmpty(version))
                        Log.Error("[!] Failed to retrieve version.");
                }
                else if (isOutdated)
                {
                    Log.Warning("[*] {0} hasn't updated in more than 10 days, falling back to Roblox API...", SirstrapConfiguration.ExploitName);

                    version = await GetRobloxVersionAsync();

                    if (string.IsNullOrEmpty(version))
                    {
                        Log.Error("[!] Failed to retrieve version from Roblox API, using outdated {0} version...", SirstrapConfiguration.ExploitName);

                        version = exploitVersion;
                    }
                }
                else
                    version = exploitVersion;
            }

            Log.Information("[*] Using version: {0}.", version);

            return version;
        }
    }
}
