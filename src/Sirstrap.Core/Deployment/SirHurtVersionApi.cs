namespace Sirstrap.Core.Deployment
{
    public sealed class SirHurtVersionApi(HttpClient httpClient)
    {
        private const int OUTDATED_AFTER_DAYS = 10;
#pragma warning disable S1075 // URIs should not be hardcoded - External API endpoint.
        private const string SIRHURT_API_URI = "https://sirhurt.net/status/fetch_2.php?exploit=SirHurt%20V5";
#pragma warning restore S1075

        public async Task<(string Version, bool IsOutdated)> GetVersionAsync()
        {
            try
            {
                string response = await httpClient.GetStringAsync(SIRHURT_API_URI);

                using JsonDocument jsonDocument = JsonDocument.Parse(response);

                if (jsonDocument.RootElement.ValueKind != JsonValueKind.Array || jsonDocument.RootElement.GetArrayLength() == 0)
                {
                    Log.Error("[!] The SirHurt API returned an unexpected JSON structure (expected an array).");

                    return (string.Empty, false);
                }

                if (!jsonDocument.RootElement[0].TryGetProperty("SirHurt V5", out var sirhurt))
                {
                    Log.Error("[!] The SirHurt V5 field was not found in the SirHurt API response.");

                    return (string.Empty, false);
                }

                string version = sirhurt.TryGetProperty("roblox_version", out var versionElement)
                    ? versionElement.GetString() ?? string.Empty
                    : string.Empty;

                if (string.IsNullOrEmpty(version))
                {
                    Log.Error("[!] The roblox_version field was not found in the SirHurt API response.");

                    return (string.Empty, false);
                }

                return (version, IsOutdated(sirhurt));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to retrieve the Roblox version from the SirHurt API.");

                return (string.Empty, false);
            }
        }

        private static bool IsOutdated(JsonElement sirhurt)
        {
            if (!sirhurt.TryGetProperty("last_update_unix", out var lastUpdateElement))
                return false;

            DateTimeOffset lastUpdate = DateTimeOffset.FromUnixTimeSeconds(lastUpdateElement.GetInt64());

            return (DateTimeOffset.UtcNow - lastUpdate).TotalDays > OUTDATED_AFTER_DAYS;
        }
    }
}
