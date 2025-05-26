using Serilog;
using System.Text.Json;

namespace Sirstrap.Core
{
    /// <summary>
    /// Manages version information for Roblox deployments, providing strategies to 
    /// retrieve the latest versions from multiple sources based on user preferences.
    /// </summary>
    /// <remarks>
    /// This class implements different strategies for obtaining Roblox version information:
    /// - Through the official Roblox API (safe mode)
    /// - Through third-party APIs like SirHurt with fallback to official API (regular mode)
    /// 
    /// The strategy used is determined by the SafeMode setting in the application configuration.
    /// </remarks>
    public class VersionManager(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        /// <summary>
        /// Retrieves the latest version for the specified binary type based on the configured SafeMode setting.
        /// </summary>
        /// <param name="binaryType">The type of binary to get the version for (e.g., "WindowsPlayer", "WindowsStudio64").</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains
        /// the latest version string, or an empty string if version retrieval fails from all sources.
        /// </returns>
        /// <remarks>
        /// The version retrieval strategy depends on the SafeMode setting:
        /// 
        /// When SafeMode is enabled (default):
        /// - Only the official Roblox API is used, providing the most reliable and compatible version
        /// - If the Roblox API fails, no version is returned
        /// 
        /// When SafeMode is disabled:
        /// - First attempts to retrieve version information from the SirHurt API
        /// - If SirHurt API retrieval fails, falls back to the Roblox API
        /// - If both APIs fail, no version is returned
        /// 
        /// The method logs detailed information about the retrieval process,
        /// including which API was used and the retrieved version.
        /// </remarks>
        public async Task<string> GetLatestVersionAsync(string binaryType)
        {
            Log.Information("[*] No version specified, getting version from APIs...");
            
            var safeMode = SettingsManager.GetSettings().SafeMode;

            Log.Information("[*] Safe mode is {0}.", safeMode ? "enabled" : "disabled");

            if (safeMode)
            {
                // In safe mode, only use Roblox API
                var robloxVersion = await GetRobloxVersionAsync().ConfigureAwait(false);

                if (!string.IsNullOrEmpty(robloxVersion))
                {
                    Log.Information("[*] Using Roblox version: {0}.", robloxVersion);

                    return robloxVersion;
                }

                Log.Error("[!] Failed to retrieve version from Roblox API.");

                return string.Empty;
            }
            else
            {
                // In normal mode, try SirHurt API first with fallback to Roblox API
                var sirhurtVersion = await GetSirhurtVersionAsync(binaryType).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(sirhurtVersion))
                {
                    Log.Information("[*] Using SirHurt version: {0}.", sirhurtVersion);

                    return sirhurtVersion;
                }

                // Fall back to Roblox API if SirHurt API failed
                Log.Information("[*] Failed to retrieve version from SirHurt API, trying Roblox API...");

                var robloxVersion = await GetRobloxVersionAsync().ConfigureAwait(false);

                if (!string.IsNullOrEmpty(robloxVersion))
                {
                    Log.Information("[*] Using Roblox version: {0}.", robloxVersion);

                    return robloxVersion;
                }

                // Both APIs failed
                Log.Error("[!] Failed to retrieve version from both APIs.");

                return string.Empty;
            }
        }

        /// <summary>
        /// Retrieves the latest Roblox version from the SirHurt API for the specified binary type.
        /// </summary>
        /// <param name="binaryType">The type of binary to get the version for (e.g., "WindowsPlayer").</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains
        /// the version string from the SirHurt API, or an empty string if retrieval fails.
        /// </returns>
        /// <remarks>
        /// This method queries the SirHurt API which may provide more recent version information
        /// useful for specific use cases.
        /// 
        /// Currently, only "WindowsPlayer" binary type is supported for SirHurt version retrieval.
        /// The method parses the JSON response to extract the "roblox_version" field.
        /// 
        /// The API call is made to a third-party service and may be subject to availability constraints.
        /// </remarks>
        private async Task<string> GetSirhurtVersionAsync(string binaryType)
        {
            var versionApiUrl = GetVersionApiUrl(binaryType);

            if (string.IsNullOrEmpty(versionApiUrl))
            {
                Log.Error("[!] Cannot get version for binary type '{0}'.", binaryType);

                return string.Empty;
            }

            try
            {
                var response = await _httpClient.GetStringAsync(versionApiUrl).ConfigureAwait(false);

                using var jsonDocument = JsonDocument.Parse(response);

                if (jsonDocument.RootElement.EnumerateArray().FirstOrDefault().TryGetProperty("SirHurt V5", out var sirhurt))
                {
                    if (sirhurt.TryGetProperty("roblox_version", out var version))
                    {
                        return version.GetString() ?? string.Empty;
                    }

                    Log.Error("[!] roblox_version field not found in JSON response.");

                    return string.Empty;
                }

                Log.Error("[!] SirHurt V5 field not found in JSON response.");

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting SirHurt version from API: {0}.", ex.Message);

                return string.Empty;
            }
        }

        /// <summary>
        /// Retrieves the latest Roblox version from the official Roblox API.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains
        /// the version string from the Roblox API, or an empty string if retrieval fails.
        /// </returns>
        /// <remarks>
        /// This method provides the most reliable and official version information by
        /// querying the Roblox client settings CDN directly.
        /// 
        /// The method parses the JSON response to extract the "clientVersionUpload" field,
        /// which contains the latest version identifier. This is the recommended source
        /// for version information in production environments.
        /// 
        /// The API endpoint being used is: https://clientsettingscdn.roblox.com/v1/client-version/WindowsPlayer
        /// </remarks>
        private async Task<string> GetRobloxVersionAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("https://clientsettingscdn.roblox.com/v1/client-version/WindowsPlayer").ConfigureAwait(false);

                using var jsonDocument = JsonDocument.Parse(response);

                if (jsonDocument.RootElement.TryGetProperty("clientVersionUpload", out var version))
                {
                    return version.GetString() ?? string.Empty;
                }

                Log.Error("[!] clientVersionUpload field not found in JSON response.");

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting Roblox version from API: {0}.", ex.Message);

                return string.Empty;
            }
        }

        /// <summary>
        /// Ensures a version string is in the standardized "version-X.Y.Z.W" format.
        /// </summary>
        /// <param name="version">The version string to normalize.</param>
        /// <returns>
        /// A normalized version string that always starts with "version-".
        /// </returns>
        /// <remarks>
        /// This utility method maintains consistency in version formatting throughout the application.
        /// If the input version already starts with "version-" (case-insensitive), it is returned unchanged.
        /// Otherwise, "version-" is prepended to the input string.
        /// 
        /// Example transformations:
        /// - "1.2.3.4" → "version-1.2.3.4"
        /// - "VERSION-1.2.3.4" → "VERSION-1.2.3.4" (unchanged)
        /// - "version-1.2.3.4" → "version-1.2.3.4" (unchanged)
        /// </remarks>
        public static string NormalizeVersion(string version)
        {
            return version.StartsWith("version-", StringComparison.CurrentCultureIgnoreCase) ? version : $"version-{version}";
        }

        /// <summary>
        /// Determines the appropriate SirHurt API URL for the specified binary type.
        /// </summary>
        /// <param name="binaryType">The type of binary to get the version for.</param>
        /// <returns>
        /// The URL to the SirHurt API for the specified binary type, or an empty string
        /// if the binary type is not supported.
        /// </returns>
        /// <remarks>
        /// Currently, only "WindowsPlayer" binary type is supported, which returns
        /// the SirHurt V5 status API URL.
        /// 
        /// For unsupported binary types, an empty string is returned, indicating that
        /// SirHurt API version retrieval is not available for that binary type.
        /// </remarks>
        private static string GetVersionApiUrl(string binaryType)
        {
            return binaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase) ? "https://sirhurt.net/status/fetch.php?exploit=SirHurt%20V5" : string.Empty;
        }
    }
}