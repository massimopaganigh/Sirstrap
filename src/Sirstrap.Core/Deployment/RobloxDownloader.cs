namespace Sirstrap.Core.Deployment
{
    public sealed class RobloxDownloader(
        ISirstrapUpdateService sirstrapUpdateService,
        IRobloxVersionService robloxVersionService,
        IPackageManager packageManager,
        ICdnResolver cdnResolver,
        IInstaller installer,
        IRobloxLauncher robloxLauncher,
        IPathManager pathManager,
        IPerformanceTelemetry performanceTelemetry) : IRobloxDownloader
    {
        public async Task ExecuteAsync(string[] args, SirstrapType sirstrapType)
        {
            using ITelemetryScope scope = performanceTelemetry.Measure("sirstrap.execute", new Dictionary<string, object>
            {
                ["sirstrapType"] = sirstrapType.ToString()
            });

            try
            {
                await sirstrapUpdateService.UpdateAsync(sirstrapType, args);

                var configuration = ConfigurationService.CreateConfigurationFromArguments(ConfigurationService.ParseConfiguration(args));

                scope.SetTag("channel", configuration.ChannelName);
                scope.SetTag("binaryType", configuration.BinaryType);

                if (!await ResolveVersionAsync(configuration).ConfigureAwait(false))
                {
                    scope.MarkFailed();

                    performanceTelemetry.RecordCounter("sirstrap.execute.outcome", new Dictionary<string, object> { ["value"] = "VersionResolutionFailed" });

                    return;
                }

                if (IsAlreadyInstalled(configuration))
                {
                    Log.Information("[*] The version {VersionHash} is already installed.", configuration.VersionHash);

                    performanceTelemetry.RecordCounter("sirstrap.execute.cache_hit", new Dictionary<string, object> { ["binaryType"] = configuration.BinaryType });

                    if (LaunchApplication(configuration))
                    {
                        performanceTelemetry.RecordCounter("sirstrap.execute.outcome", new Dictionary<string, object> { ["value"] = "Cached" });

                        return;
                    }
                }

                await cdnResolver.ResolveAsync(configuration).ConfigureAwait(false);

                pathManager.ClearCacheDirectory();

                await DownloadArchiveAsync(configuration).ConfigureAwait(false);

                InstallAndLaunchApplication(configuration);

                performanceTelemetry.RecordCounter("sirstrap.execute.outcome", new Dictionary<string, object> { ["value"] = "Success" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to execute Sirstrap.");

                scope.MarkFailed();

                performanceTelemetry.RecordCounter("sirstrap.execute.outcome", new Dictionary<string, object> { ["value"] = "Failed" });

                Environment.ExitCode = 1;
            }
        }

        private async Task DownloadArchiveAsync(Configuration configuration)
        {
            if (configuration.IsMacBinary())
                await packageManager.DownloadMacArchiveAsync(configuration).ConfigureAwait(false);
            else
                await packageManager.DownloadWindowsArchiveAsync(configuration).ConfigureAwait(false);
        }

        private void InstallAndLaunchApplication(Configuration configuration)
        {
            if (!configuration.IsWindowsPlayer())
                return;

            installer.Install(configuration);

            LaunchApplication(configuration);
        }

        private bool IsAlreadyInstalled(Configuration configuration)
            => configuration.IsWindowsPlayer() && File.Exists(Path.Combine(pathManager.GetExtractionPath(configuration.VersionHash), "RobloxPlayerBeta.exe"));

        private bool LaunchApplication(Configuration configuration)
            => configuration.IsWindowsPlayer() && robloxLauncher.Launch(configuration);

        private async Task<bool> ResolveVersionAsync(Configuration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.VersionHash))
                return true;

            configuration.VersionHash = await robloxVersionService.GetLatestVersionAsync();

            return !string.IsNullOrEmpty(configuration.VersionHash);
        }
    }
}
