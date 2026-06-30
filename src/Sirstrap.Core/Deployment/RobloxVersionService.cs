namespace Sirstrap.Core.Deployment
{
    public sealed class RobloxVersionService(RobloxClientVersionApi robloxApi, SirHurtVersionApi sirHurtApi, IWeaoService weaoService, SirstrapConfiguration sirstrapConfiguration, IPerformanceTelemetry performanceTelemetry) : IRobloxVersionService
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

        private async Task<(string Version, VersionResolutionSource Source)> GetSirHurtVersionAsync()
        {
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

        private async Task<(string Version, VersionResolutionSource Source)> GetWeaoVersionAsync()
        {
            string? version = await weaoService.GetCurrentWindowsVersionAsync();

            if (string.IsNullOrWhiteSpace(version))
            {
                Log.Error("[!] Failed to retrieve the current Roblox version from WEAO, falling back to the Roblox API...");

                return await GetRobloxApiVersionAsync();
            }

            return (version, VersionResolutionSource.Weao);
        }

        private async Task<(string Version, VersionResolutionSource Source)> GetExecutorVersionAsync(string executor)
        {
            string? version = await weaoService.GetExecutorVersionAsync(executor);

            if (string.IsNullOrWhiteSpace(version))
            {
                Log.Error("[!] Failed to retrieve the version supported by the executor {Executor} from WEAO, falling back to the Roblox API...", executor);

                return await GetRobloxApiVersionAsync();
            }

            return (version, VersionResolutionSource.Executor);
        }

        private async Task<(string Version, VersionResolutionSource Source)> ResolveAsync()
        {
            var source = (sirstrapConfiguration.RobloxVersionSource ?? string.Empty).Trim();

            if (source.StartsWith(RobloxVersionSources.VersionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var pinned = source[RobloxVersionSources.VersionPrefix.Length..].Trim();

                if (!string.IsNullOrWhiteSpace(pinned))
                {
                    Log.Information("[*] Using the pinned Roblox version {Version}...", pinned);

                    return (pinned, VersionResolutionSource.Override);
                }
            }

            if (source.StartsWith(RobloxVersionSources.ExecutorPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var executor = source[RobloxVersionSources.ExecutorPrefix.Length..].Trim();

                Log.Information("[*] Resolving the Roblox version supported by the executor {Executor} from WEAO...", executor);

                return await GetExecutorVersionAsync(executor);
            }

            if (source.Equals(RobloxVersionSources.Roblox, StringComparison.OrdinalIgnoreCase))
            {
                Log.Information("[*] Retrieving the Roblox version from the Roblox API...");

                return await GetRobloxApiVersionAsync();
            }

            if (source.Equals(RobloxVersionSources.Weao, StringComparison.OrdinalIgnoreCase))
            {
                Log.Information("[*] Retrieving the current Roblox version from WEAO...");

                return await GetWeaoVersionAsync();
            }

            Log.Information("[*] Retrieving the Roblox version from the SirHurt API...");

            return await GetSirHurtVersionAsync();
        }
    }
}
