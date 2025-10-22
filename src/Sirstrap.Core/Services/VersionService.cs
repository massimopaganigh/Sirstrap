namespace Sirstrap.Core.Services
{
    public class VersionService(HttpClient httpClient)
: IVersionService
    {
        public async Task<string> GetVersionAsync()
        {
            try
            {
                var version = string.Empty;

                if (!SirstrapConfiguration.RobloxApi)
                {
                    var isOutdated = false;

                    (version, isOutdated) = await GetSirHurtVersionAsync();

                    if (!string.IsNullOrEmpty(version)
                        && !isOutdated)
                        return version;
                }

                version = await GetRobloxVersionAsync();

                if (!string.IsNullOrEmpty(version))
                    return version;

                throw new Exception("Version is null or empty");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting version: {0}.", ex.Message);

                throw;
            }
        }

        #region PRIVATE METHODS
        private async Task<string> GetRobloxVersionAsync()
        {
            if (JsonDocument.Parse((await httpClient.BetterGetStringAsync("https://clientsettingscdn.roblox.com/v1/client-version/WindowsPlayer"))!).RootElement.TryGetProperty("clientVersionUpload", out var version))
                return version.GetString() ?? string.Empty;

            return string.Empty;
        }

        private async Task<(string version, bool isOutdated)> GetSirHurtVersionAsync()
        {
            using var jsonDocument = JsonDocument.Parse((await httpClient.BetterGetStringAsync("https://sirhurt.net/status/fetch.php?exploit=SirHurt%20V5"))!);

            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array
                && jsonDocument.RootElement.GetArrayLength() > 0
                && jsonDocument.RootElement[0].TryGetProperty("SirHurt V5", out var sirhurt))
            {
                var version = string.Empty;

                if (sirhurt.TryGetProperty("roblox_version", out var robloxVersion))
                    version = robloxVersion.GetString() ?? string.Empty;

                var isOutdated = false;

                if (sirhurt.TryGetProperty("last_update_unix", out var lastUpdateUnix))
                    isOutdated = (DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(lastUpdateUnix.GetInt64())).TotalDays > 10;

                return (version, isOutdated);
            }

            return (string.Empty, false);
        }
        #endregion
    }
}
