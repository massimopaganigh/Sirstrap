namespace Sirstrap.Core.Deployment
{
    public sealed class RobloxClientVersionApi(HttpClient httpClient)
    {
#pragma warning disable S1075 // URIs should not be hardcoded - External API endpoint.
        private const string ROBLOX_API_URI = "https://clientsettingscdn.roblox.com/v1/client-version/WindowsPlayer";
#pragma warning restore S1075

        public async Task<string> GetVersionAsync()
        {
            try
            {
                string response = await httpClient.GetStringAsync(ROBLOX_API_URI);

                using JsonDocument jsonDocument = JsonDocument.Parse(response);

                if (jsonDocument.RootElement.TryGetProperty("clientVersionUpload", out var version))
                    return version.GetString() ?? string.Empty;

                Log.Error("[!] The clientVersionUpload field was not found in the Roblox API response.");

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to retrieve the Roblox version from the Roblox API.");

                return string.Empty;
            }
        }
    }
}
