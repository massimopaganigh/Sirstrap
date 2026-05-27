namespace Sirstrap.Core
{
    public class RobloxVersionService(HttpClient httpClient)
    {
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

        public async Task<string> GetLatestVersionAsync()
        {
            using ITelemetryScope scope = Telemetry.Performance.Measure("version.resolve");

            string version;
            VersionResolutionSource source;

            if (!string.IsNullOrWhiteSpace(SirstrapConfiguration.RobloxVersionOverride))
            {
                Log.Information("[*] Roblox version override is set, using Roblox version override to retrieve version...");

                version = SirstrapConfiguration.RobloxVersionOverride;
                source = VersionResolutionSource.Override;

                FinishVersionResolution(scope, version, source);

                return version;
            }

            if (SirstrapConfiguration.RobloxApi)
            {
                Log.Information("[*] Roblox API is enabled, using Roblox API to retrieve version...");

                version = await GetRobloxVersionAsync();
                source = string.IsNullOrWhiteSpace(version) ? VersionResolutionSource.Failed : VersionResolutionSource.RobloxApi;

                if (string.IsNullOrWhiteSpace(version))
                    Log.Error("[!] Failed to retrieve version.");
            }
            else
            {
                Log.Information("[*] Roblox API is disabled, using SirHurt API to retrieve version.");

                var (sirhurtVersion, isOutdated) = await GetSirhurtVersionAsync();

                if (string.IsNullOrEmpty(sirhurtVersion))
                {
                    Log.Error("[!] Failed to retrieve version from SirHurt API, using Roblox API to retrieve version...");

                    version = await GetRobloxVersionAsync();
                    source = string.IsNullOrWhiteSpace(version) ? VersionResolutionSource.Failed : VersionResolutionSource.RobloxApi;

                    if (string.IsNullOrEmpty(version))
                        Log.Error("[!] Failed to retrieve version.");
                }
                else if (isOutdated)
                {
                    Log.Warning("[*] SirHurt hasn't updated in more than 10 days, falling back to Roblox API...");

                    version = await GetRobloxVersionAsync();

                    if (string.IsNullOrEmpty(version))
                    {
                        Log.Error("[!] Failed to retrieve version from Roblox API, using outdated SirHurt version...");

                        version = sirhurtVersion;
                        source = VersionResolutionSource.SirHurtFallback;
                    }
                    else
                        source = VersionResolutionSource.RobloxApi;
                }
                else
                {
                    version = sirhurtVersion;
                    source = VersionResolutionSource.SirHurt;
                }
            }

            Log.Information("[*] Using version: {0}.", version);

            FinishVersionResolution(scope, version, source);

            return version;
        }

        private static void FinishVersionResolution(ITelemetryScope scope, string version, VersionResolutionSource source)
        {
            scope.SetTag("source", source.ToString());

            if (string.IsNullOrEmpty(version))
                scope.MarkFailed();

            Telemetry.Performance.RecordCounter("version.resolve.outcome", new Dictionary<string, object>
            {
                ["source"] = source.ToString(),
                ["success"] = !string.IsNullOrEmpty(version)
            });
        }

#pragma warning disable S1075 // URIs should not be hardcoded - These are external API endpoints
        private const string ROBLOX_API_URI = "https://clientsettingscdn.roblox.com/v1/client-version/WindowsPlayer";
        private const string SIRHURT_API_URI = "https://sirhurt.net/status/fetch_2.php?exploit=SirHurt%20V5";
#pragma warning restore S1075
    }
}
