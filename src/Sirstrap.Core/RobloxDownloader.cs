namespace Sirstrap.Core
{
    public class RobloxDownloader
    {
        private readonly PackageManager _packageManager;
        private readonly HttpClient _httpClient;
        private readonly RobloxVersionService _robloxVersionService;

        public RobloxDownloader()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            _robloxVersionService = new RobloxVersionService(_httpClient);
            _packageManager = new PackageManager(_httpClient);
        }

        private async Task DownloadAndProcessFilesAsync(Configuration configuration)
        {
            if (configuration.IsMacBinary())
            {
                await _packageManager.Download4MacAsync(configuration).ConfigureAwait(false);
            }
            else
            {
                await _packageManager.Download4WindowsAsync(configuration).ConfigureAwait(false);
            }
        }

        private async Task<bool> InitializeDownloadAsync(Configuration configuration)
        {
            if (string.IsNullOrEmpty(configuration.VersionHash))
            {
                configuration.VersionHash = await _robloxVersionService.GetLatestVersionAsync();

                if (string.IsNullOrEmpty(configuration.VersionHash))
                {
                    return false;
                }
            }

            return true;
        }

        private static void InstallAndLaunchApplication(Configuration configuration)
        {
            if (!configuration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Installer.Install(configuration);

            LaunchApplication(configuration);
        }

        private static bool IsAlreadyInstalled(Configuration configuration)
        {
            return configuration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(PathManager.GetExtractionPath(configuration.VersionHash), "RobloxPlayerBeta.exe"));
        }

        private static bool LaunchApplication(Configuration configuration)
        {
            return configuration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase) && RobloxLauncher.Launch(configuration);
        }

        public async Task ExecuteAsync(string[] args, SirstrapType sirstrapType)
        {
            using ITelemetryScope scope = Telemetry.Performance.Measure("sirstrap.execute", new Dictionary<string, object>
            {
                ["sirstrapType"] = sirstrapType.ToString()
            });

            try
            {
                using var updateService = new SirstrapUpdateService();

                await updateService.UpdateAsync(sirstrapType, args);

                var configuration = ConfigurationService.CreateConfigurationFromArguments(ConfigurationService.ParseConfiguration(args));

                scope.SetTag("channel", configuration.ChannelName);
                scope.SetTag("binaryType", configuration.BinaryType);

                if (!await InitializeDownloadAsync(configuration).ConfigureAwait(false))
                {
                    scope.MarkFailed();

                    Telemetry.Performance.RecordCounter("sirstrap.execute.outcome", new Dictionary<string, object> { ["value"] = "VersionResolutionFailed" });

                    return;
                }

                if (IsAlreadyInstalled(configuration))
                {
                    Log.Information("[*] Version {0} is already installed.", configuration.VersionHash);

                    Telemetry.Performance.RecordCounter("sirstrap.execute.cache_hit", new Dictionary<string, object> { ["binaryType"] = configuration.BinaryType });

                    if (LaunchApplication(configuration))
                    {
                        Telemetry.Performance.RecordCounter("sirstrap.execute.outcome", new Dictionary<string, object> { ["value"] = "Cached" });

                        return;
                    }
                }

                await RobloxCdnService.ResolveAsync(_httpClient, configuration).ConfigureAwait(false);

                Configuration.ClearCacheDirectory();

                await DownloadAndProcessFilesAsync(configuration).ConfigureAwait(false);

                InstallAndLaunchApplication(configuration);

                Telemetry.Performance.RecordCounter("sirstrap.execute.outcome", new Dictionary<string, object> { ["value"] = "Success" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error: {0}", ex.Message);

                scope.MarkFailed();

                Telemetry.Performance.RecordCounter("sirstrap.execute.outcome", new Dictionary<string, object> { ["value"] = "Failed" });

                Environment.ExitCode = 1;
            }
        }
    }
}
