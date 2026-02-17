namespace Sirstrap.Core
{
    public class RobloxDownloader
    {
        private readonly PackageManager _packageManager;
        private readonly RobloxVersionService _robloxVersionService;

        public RobloxDownloader()
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            _robloxVersionService = new RobloxVersionService(httpClient);
            _packageManager = new PackageManager(httpClient);
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

        private async Task DownloadAndInstallAsync(Configuration configuration)
        {
            if (!await InitializeDownloadAsync(configuration).ConfigureAwait(false))
            {
                throw new InvalidOperationException("Failed to initialize download: could not resolve version hash.");
            }

            if (IsAlreadyInstalled(configuration))
            {
                Log.Information("[*] Version {0} is already installed, skipping download.", configuration.VersionHash);

                return;
            }

            Configuration.ClearCacheDirectory();

            await DownloadAndProcessFilesAsync(configuration).ConfigureAwait(false);

            if (configuration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase))
                Installer.Install(configuration);
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
            return configuration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase) && Directory.Exists(PathManager.GetExtractionPath(configuration.VersionHash));
        }

        private static bool LaunchApplication(Configuration configuration)
        {
            return configuration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase) && RobloxLauncher.Launch(configuration);
        }

        private async Task RunVisitAsync(Configuration configuration, CancellationToken cancellationToken)
        {
            Log.Information("[VISIT] ========================================");
            Log.Information("[VISIT] Visit Mode Configuration");
            Log.Information("[VISIT] ========================================");
            Log.Information("[VISIT] BinaryType: {0}", configuration.BinaryType);
            Log.Information("[VISIT] ChannelName: {0}", configuration.ChannelName);
            Log.Information("[VISIT] VersionHash: {0}", configuration.VersionHash);
            Log.Information("[VISIT] CookiesFile: {0}", configuration.CookiesFile);
            Log.Information("[VISIT] PlaceId: {0}", configuration.PlaceId);
            Log.Information("[VISIT] Timeout: {0}s", configuration.Timeout);
            Log.Information("[VISIT] CookiesFile exists: {0}", File.Exists(configuration.CookiesFile));

            if (SirstrapConfiguration.MultiInstance)
                Log.Warning("[VISIT] MultiInstance is enabled. The visit loop may block if RobloxLauncher waits for the singleton. Consider disabling MultiInstance for visit mode.");

            Log.Information("[VISIT] ========================================");
            Log.Information("[VISIT] Downloading & Installing Roblox");
            Log.Information("[VISIT] ========================================");

            await DownloadAndInstallAsync(configuration).ConfigureAwait(false);

            Log.Information("[VISIT] Roblox installed successfully, version hash: {0}", configuration.VersionHash);

            Log.Information("[VISIT] ========================================");
            Log.Information("[VISIT] Starting Visit Loop");
            Log.Information("[VISIT] ========================================");
            Log.Information("[VISIT] Press Ctrl+C to stop the visit loop.");

            using var authClient = new RobloxAuthClient();

            Log.Information("[VISIT] RobloxAuthClient created.");

            var visitService = new VisitService(authClient);

            Log.Information("[VISIT] VisitService created.");

            await visitService.RunVisitLoopAsync(configuration, configuration.CookiesFile, configuration.PlaceId, configuration.Timeout, cancellationToken).ConfigureAwait(false);

            Log.Information("[VISIT] Visit loop has ended.");
        }

        public async Task ExecuteAsync(string[] args, SirstrapType sirstrapType, CancellationToken cancellationToken = default)
        {
            try
            {
                using var updateService = new SirstrapUpdateService();

                await updateService.UpdateAsync(sirstrapType, args);

                var configuration = ConfigurationService.CreateConfigurationFromArguments(ConfigurationService.ParseConfiguration(args));

                if (configuration.IsVisitMode())
                {
                    Log.Information("[*] Visit mode detected, starting visit generation pipeline...");

                    await RunVisitAsync(configuration, cancellationToken).ConfigureAwait(false);

                    return;
                }

                if (!await InitializeDownloadAsync(configuration).ConfigureAwait(false))
                {
                    return;
                }

                if (IsAlreadyInstalled(configuration))
                {
                    Log.Information("[*] Version {0} is already installed.", configuration.VersionHash);

                    if (LaunchApplication(configuration))
                    {
                        return;
                    }
                }

                Configuration.ClearCacheDirectory();

                await DownloadAndProcessFilesAsync(configuration).ConfigureAwait(false);

                InstallAndLaunchApplication(configuration);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error: {0}", ex.Message);

                Environment.ExitCode = 1;
            }
        }
    }
}
