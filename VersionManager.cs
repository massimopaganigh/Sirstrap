using Serilog;
using System.Text.Json;

namespace Sirstrap
{
    /// <summary>
    /// Manages version information for Roblox deployments, providing functionality to 
    /// retrieve the latest versions from both SirHurt and official Roblox APIs, 
    /// compare them, and help users select the appropriate version.
    /// </summary>
    public class VersionManager(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        /// <summary>
        /// Retrieves the latest version for the specified binary type by checking both
        /// SirHurt and official Roblox APIs, and prompting for user selection if versions differ.
        /// </summary>
        /// <param name="binaryType">The type of binary to get the version for (e.g., "WindowsPlayer").</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains
        /// the selected version string, or an empty string if version retrieval fails.
        /// </returns>
        /// <remarks>
        /// This method:
        /// 1. Attempts to retrieve version information from both SirHurt and Roblox APIs
        /// 2. If both versions match, returns that version automatically
        /// 3. If versions differ, prompts the user to choose which version to use
        /// 4. Returns an empty string if version retrieval fails from either source
        /// </remarks>
        public async Task<string> GetLatestVersionAsync(string binaryType)
        {
            Log.Information("[*] No version specified, getting versions from APIs...");

            var sirhurtVersion = await GetSirhurtVersionAsync(binaryType).ConfigureAwait(false);
            var robloxVersion = await GetRobloxVersionAsync().ConfigureAwait(false);

            if (string.IsNullOrEmpty(sirhurtVersion) || string.IsNullOrEmpty(robloxVersion))
            {
                Log.Error("[!] Failed to retrieve one or both versions.");

                return string.Empty;
            }

            if (sirhurtVersion.Equals(robloxVersion, StringComparison.OrdinalIgnoreCase))
            {
                Log.Information("[*] Using version: {0}", sirhurtVersion);

                return sirhurtVersion;
            }

            Log.Information("[*] Version mismatch detected:");
            Log.Information("[*] Please choose which version to use:");
            Log.Information("    1. SirHurt version: {0}", sirhurtVersion);
            Log.Information("    2. Roblox version: {0}", robloxVersion);

            var choice = Console.ReadLine();

            return choice == "1" ? sirhurtVersion : robloxVersion;
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
        /// Currently, only "WindowsPlayer" binary type is supported for SirHurt version retrieval.
        /// The method parses the JSON response to extract the "roblox_version" field.
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
                Log.Error(ex, "[!] Error getting SirHurt version from API: {0}", ex.Message);

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
        /// The method queries the Roblox client settings CDN and parses the JSON response
        /// to extract the "clientVersionUpload" field, which contains the latest version.
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
                Log.Error(ex, "[!] Error getting Roblox version from API: {0}", ex.Message);

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
        /// If the input version already starts with "version-", it is returned unchanged.
        /// Otherwise, "version-" is prepended to the input string.
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
        /// </remarks>
        private static string GetVersionApiUrl(string binaryType)
        {
            return binaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase) ? "https://sirhurt.net/status/fetch.php?exploit=SirHurt%20V5" : string.Empty;
        }
    }
}