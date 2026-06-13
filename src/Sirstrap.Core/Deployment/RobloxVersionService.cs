namespace Sirstrap.Core.Deployment
{
    public sealed class RobloxVersionService(RobloxClientVersionApi robloxApi, SirHurtVersionApi sirHurtApi, SirstrapConfiguration sirstrapConfiguration, IPerformanceTelemetry performanceTelemetry) : IRobloxVersionService
    {
        public async Task<string> GetLatestVersionAsync()
        {
            using ITelemetryScope scope = performanceTelemetry.Measure("version.resolve");

            var (version, source) = await ResolveAsync();

            scope.SetTag("source", source.ToString());

            if (string.IsNullOrEmpty(version))
                scope.MarkFailed();
            else
                Log.Information("[*] Using the Roblox version {Version} (source: {Source}).", version, source);

            performanceTelemetry.RecordCounter("version.resolve.outcome", new Dictionary<string, object>
            {
                ["source"] = source.ToString(),
                ["success"] = !string.IsNullOrEmpty(version)
            });

            return version;
        }

        private async Task<(string Version, VersionResolutionSource Source)> GetRobloxApiVersionAsync()
        {
            string version = await robloxApi.GetVersionAsync();

            if (string.IsNullOrWhiteSpace(version))
            {
                Log.Error("[!] Failed to retrieve the Roblox version from the Roblox API.");

                return (version, VersionResolutionSource.Failed);
            }

            return (version, VersionResolutionSource.RobloxApi);
        }

        private async Task<(string Version, VersionResolutionSource Source)> ResolveAsync()
        {
            if (!string.IsNullOrWhiteSpace(sirstrapConfiguration.RobloxVersionOverride))
            {
                Log.Information("[*] Using the Roblox version override...");

                return (sirstrapConfiguration.RobloxVersionOverride, VersionResolutionSource.Override);
            }

            if (sirstrapConfiguration.RobloxApi)
            {
                Log.Information("[*] The Roblox API is enabled, retrieving the Roblox version from the Roblox API...");

                return await GetRobloxApiVersionAsync();
            }

            Log.Information("[*] The Roblox API is disabled, retrieving the Roblox version from the SirHurt API...");

            var (sirHurtVersion, isOutdated) = await sirHurtApi.GetVersionAsync();

            if (string.IsNullOrEmpty(sirHurtVersion))
            {
                Log.Error("[!] Failed to retrieve the Roblox version from the SirHurt API, falling back to the Roblox API...");

                return await GetRobloxApiVersionAsync();
            }

            if (isOutdated)
            {
                Log.Warning("[!] SirHurt has not updated in more than 10 days, falling back to the Roblox API...");

                string robloxVersion = await robloxApi.GetVersionAsync();

                if (string.IsNullOrEmpty(robloxVersion))
                {
                    Log.Error("[!] Failed to retrieve the Roblox version from the Roblox API, falling back to the outdated SirHurt version...");

                    return (sirHurtVersion, VersionResolutionSource.SirHurtFallback);
                }

                return (robloxVersion, VersionResolutionSource.RobloxApi);
            }

            return (sirHurtVersion, VersionResolutionSource.SirHurt);
        }
    }
}
