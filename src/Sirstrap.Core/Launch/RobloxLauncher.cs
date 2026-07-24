namespace Sirstrap.Core.Launch
{
    public sealed class RobloxLauncher(
        SirstrapConfiguration sirstrapConfiguration,
        IPathManager pathManager,
        ISingletonManager singletonManager,
        IIncognitoManager incognitoManager,
        IRobloxProcessService robloxProcessService,
        IPerformanceTelemetry performanceTelemetry,
        IFFlagManager fflagManager) : IRobloxLauncher
    {
        private const string ROBLOX_PLAYER_BETA_EXE = "RobloxPlayerBeta.exe";

        public bool Launch(Configuration configuration)
        {
            using ITelemetryScope scope = performanceTelemetry.Measure("roblox.launch", new Dictionary<string, object>
            {
                ["multiInstance"] = sirstrapConfiguration.RobloxMultiInstance,
                ["incognito"] = sirstrapConfiguration.RobloxIncognito
            });

            var extractionPath = pathManager.GetExtractionPath(configuration.VersionHash);
            var robloxPlayerBetaExePath = Path.Combine(extractionPath, ROBLOX_PLAYER_BETA_EXE);

            if (!File.Exists(robloxPlayerBetaExePath))
            {
                Log.Error("[!] The Roblox executable was not found at {ExePath}.", robloxPlayerBetaExePath);

                return Fail(scope, "NotFound");
            }

            fflagManager.DeployFFlags(extractionPath);

            var singletonCaptured = false;

            try
            {
                var existingPids = robloxProcessService.SnapshotGameProcessIds();

                if (sirstrapConfiguration.RobloxMultiInstance)
                {
                    singletonCaptured = singletonManager.CaptureSingleton();

                    if (sirstrapConfiguration.RobloxIncognito
                        && singletonManager.CurrentInstanceType == InstanceType.Master)
                        incognitoManager.MoveRobloxFolderToCache();
                }

                if (!StartRoblox(robloxPlayerBetaExePath, configuration.LaunchUri))
                {
                    Log.Error("[!] Failed to launch Roblox (Process.Start returned null).");

                    return Fail(scope, "ProcessStartFailed");
                }

                performanceTelemetry.RecordCounter("roblox.launch.outcome", new Dictionary<string, object> { ["value"] = "Success" });

                if (singletonCaptured)
                    WaitForGameExit(existingPids);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to launch Roblox.");

                return Fail(scope, "Exception");
            }
            finally
            {
                if (singletonCaptured)
                    singletonManager.ReleaseSingleton();
            }
        }

        private bool Fail(ITelemetryScope scope, string outcome)
        {
            scope.MarkFailed();

            performanceTelemetry.RecordCounter("roblox.launch.outcome", new Dictionary<string, object> { ["value"] = outcome });

            return false;
        }

        private static bool StartRoblox(string exePath, string launchUri)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = exePath,
                WorkingDirectory = Path.GetDirectoryName(exePath),
                UseShellExecute = true
            };

            if (!string.IsNullOrEmpty(launchUri))
            {
                Log.Information("[*] Launching Roblox with the launch URI {LaunchUri}...", launchUri);

                startInfo.Arguments = launchUri;
            }
            else
                Log.Information("[*] Launching Roblox without a launch URI...");

            using var process = Process.Start(startInfo);

            if (process == null)
                return false;

            WaitForInputIdle(process);

            return true;
        }

        private void WaitForGameExit(HashSet<int> existingPids)
        {
            Log.Information("[*] Launched Roblox, waiting for the game process to exit (MultiInstance mode)...");

            var gamePids = robloxProcessService.FindNewGameProcessIds(existingPids);

            if (gamePids.Count == 0)
            {
                Log.Warning("[!] No new Roblox game processes were detected after the launch, cannot monitor the exit.");

                return;
            }

            while (singletonManager.HasCapturedSingleton
                && robloxProcessService.AnyGameProcessRunning(gamePids))
                Thread.Sleep(100);

            Log.Information("[*] The game exit wait ended (singleton captured: {HasCapturedSingleton}, all game processes exited: {AllExited}, Roblox processes still running: {RunningCount}).", singletonManager.HasCapturedSingleton, !robloxProcessService.AnyGameProcessRunning(gamePids), robloxProcessService.GetRunningGameProcessCount());

            robloxProcessService.LogRunningGameProcesses();
        }

        private static void WaitForInputIdle(Process process)
        {
            Log.Information("[*] Waiting for the Roblox launcher to reach the input-idle state...");

            try
            {
                process.WaitForInputIdle();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to wait for the input-idle state, the launcher process may have already exited.");
            }
        }
    }
}
