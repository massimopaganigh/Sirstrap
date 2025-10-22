namespace Sirstrap.Core
{
    public static class RobloxLauncher
    {
        private const string ROBLOX_PLAYER_BETA_EXE = "RobloxPlayerBeta.exe";

        public static bool Launch(Configuration configuration)
        {
            string robloxPlayerBetaExePath = Path.Combine(PathManager.GetExtractionPath(configuration.VersionHash), ROBLOX_PLAYER_BETA_EXE);

            if (!File.Exists(robloxPlayerBetaExePath))
            {
                Log.Error("[!] Roblox not found in: {0}.", robloxPlayerBetaExePath);

                return false;
            }

            bool multiInstance = SirstrapConfiguration.MultiInstance;
            bool singletonCaptured = false;

            try
            {
                if (multiInstance)
                {
                    singletonCaptured = SingletonManager.CaptureSingleton();

                    // Execute dedicated operations based on instance type
                    InstanceType currentInstanceType = SingletonManager.CurrentInstanceType;

                    if (currentInstanceType == InstanceType.Master)
                    {
                        Log.Information("[*] Running as MASTER instance - executing master-specific operations...");
                        OnMasterInstancePreLaunch(configuration);
                    }
                    else if (currentInstanceType == InstanceType.Slave)
                    {
                        Log.Information("[*] Running as SLAVE instance - executing slave-specific operations...");
                        OnSlaveInstancePreLaunch(configuration);
                    }
                }

                ProcessStartInfo robloxPlayerBetaExeStartInfo = new()
                {
                    FileName = robloxPlayerBetaExePath,
                    WorkingDirectory = Path.GetDirectoryName(robloxPlayerBetaExePath),
                    UseShellExecute = true
                };

                string launchUri = configuration.LaunchUri;

                if (!string.IsNullOrEmpty(launchUri))
                {
                    Log.Information("[*] Launching Roblox with URI: {0}...", launchUri);

                    robloxPlayerBetaExeStartInfo.Arguments = launchUri;
                }
                else
                    Log.Information("[*] Launching Roblox...");

                Process? robloxPlayerBetaExeProcess = Process.Start(robloxPlayerBetaExeStartInfo);

                if (robloxPlayerBetaExeProcess == null)
                {
                    Log.Error("[!] Failed to launch Roblox.");

                    return false;
                }

                Log.Information("[*] Waiting for input idle...");

                robloxPlayerBetaExeProcess.WaitForInputIdle();

                if (singletonCaptured)
                {
                    Log.Information("[*] Waiting for exit (MultiInstance enabled)...");

                    while (SingletonManager.HasCapturedSingleton
                        && !robloxPlayerBetaExeProcess.HasExited
                        && Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ROBLOX_PLAYER_BETA_EXE)).Length > 0)
                        Thread.Sleep(100);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Exception while launching Roblox: {0}.", ex.Message);

                return false;
            }
            finally
            {
                if (singletonCaptured)
                    SingletonManager.ReleaseSingleton();
            }
        }

        /// <summary>
        /// Executes operations specific to the Master instance before launching Roblox.
        /// The Master instance is the first instance that successfully captures the Roblox singleton.
        /// </summary>
        /// <param name="configuration">The configuration object containing launch parameters.</param>
        private static void OnMasterInstancePreLaunch(Configuration configuration)
        {
            // TODO: Add master-specific operations here
            // Examples:
            // - Initialize shared resources
            // - Setup monitoring or logging systems
            // - Perform administrative tasks
            // - Clean up previous sessions

            Log.Debug("[*] Master instance pre-launch operations completed.");
        }

        /// <summary>
        /// Executes operations specific to Slave instances before launching Roblox.
        /// Slave instances are instances that failed to capture the singleton because a Master already exists.
        /// </summary>
        /// <param name="configuration">The configuration object containing launch parameters.</param>
        private static void OnSlaveInstancePreLaunch(Configuration configuration)
        {
            // TODO: Add slave-specific operations here
            // Examples:
            // - Connect to master instance
            // - Register with coordination service
            // - Skip initialization of shared resources
            // - Configure different settings for secondary instances

            Log.Debug("[*] Slave instance pre-launch operations completed.");
        }
    }
}