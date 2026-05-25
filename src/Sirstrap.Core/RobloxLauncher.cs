namespace Sirstrap.Core
{
    public static class RobloxLauncher
    {
        private const string ROBLOX_PLAYER_BETA_EXE = "RobloxPlayerBeta.exe";

        private static readonly string[] _robloxProcessNames = ["RobloxPlayerBeta", "RobloxPlayerBeta.exe"];

        private static List<Process> FindNewRobloxProcesses(HashSet<int> snapshot)
        {
            for (var i = 0; i < 20; i++)
            {
                var processes = _robloxProcessNames.SelectMany(name => Process.GetProcessesByName(name)).Where(p => !snapshot.Contains(p.Id)).ToList();

                if (processes.Count > 0)
                    return processes;

                Thread.Sleep(100);
            }

            return [];
        }

        private static void LogRobloxProcesses()
        {
            foreach (var processName in _robloxProcessNames)
                foreach (var process in Process.GetProcessesByName(processName))
                    Log.Information("[*] Roblox process — Name: {0} | PID: {1} | Started: {2} | Memory: {3} MB | Title: {4}", processName, process.Id, process.StartTime.ToString("yyyy-MM-dd HH:mm:ss"), process.WorkingSet64 / 1024 / 1024, process.MainWindowTitle);
        }

        private static HashSet<int> SnapshotRobloxProcessIds() => [.. _robloxProcessNames.SelectMany(name => Process.GetProcessesByName(name)).Select(p => p.Id)];

        private static Process? StartRoblox(string exePath, string launchUri)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = exePath,
                WorkingDirectory = Path.GetDirectoryName(exePath),
                UseShellExecute = true
            };

            if (!string.IsNullOrEmpty(launchUri))
            {
                Log.Information("[*] Launching Roblox with launch URI: {0}...", launchUri);

                startInfo.Arguments = launchUri;
            }
            else
                Log.Information("[*] Launching Roblox without URI...");

            return Process.Start(startInfo);
        }

        private static void WaitForGameExit(HashSet<int> existingPids)
        {
            Log.Information("[*] Roblox launched. Waiting for game process to exit (MultiInstance mode)...");

            var gameProcesses = FindNewRobloxProcesses(existingPids);

            if (gameProcesses.Count == 0)
            {
                Log.Warning("[!] No new Roblox game processes detected after launch — cannot monitor exit.");

                return;
            }

            while (SingletonManager.HasCapturedSingleton
                && gameProcesses.Any(p => !p.HasExited))
                Thread.Sleep(100);

            Log.Information("[*] Game exit wait loop ended. HasCapturedSingleton: {0} | All game processes exited: {1} | Total Roblox processes still running: {2}.", SingletonManager.HasCapturedSingleton, gameProcesses.All(p => p.HasExited), _robloxProcessNames.Sum(name => Process.GetProcessesByName(name).Length));

            LogRobloxProcesses();
        }

        private static void WaitForInputIdle(Process process)
        {
            Log.Information("[*] Waiting for Roblox launcher to reach input-idle state...");

            try
            {
                process.WaitForInputIdle();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] WaitForInputIdle failed — launcher process may have already exited: {0}.", ex.Message);
            }
        }

        public static bool Launch(Configuration configuration)
        {
            using ITelemetryScope scope = Telemetry.Performance.Measure("roblox.launch", new Dictionary<string, object>
            {
                ["multiInstance"] = SirstrapConfiguration.MultiInstance,
                ["incognito"] = SirstrapConfiguration.Incognito
            });

            var robloxPlayerBetaExePath = Path.Combine(PathManager.GetExtractionPath(configuration.VersionHash), ROBLOX_PLAYER_BETA_EXE);

            if (!File.Exists(robloxPlayerBetaExePath))
            {
                Log.Error("[!] Roblox executable not found at path: {0}.", robloxPlayerBetaExePath);

                scope.MarkFailed();

                Telemetry.Performance.RecordCounter("roblox.launch.outcome", new Dictionary<string, object> { ["value"] = "NotFound" });

                return false;
            }

            var multiInstance = SirstrapConfiguration.MultiInstance;
            var incognito = SirstrapConfiguration.Incognito;
            var singletonCaptured = false;

            try
            {
                var existingPids = SnapshotRobloxProcessIds();

                if (multiInstance)
                {
                    singletonCaptured = SingletonManager.CaptureSingleton();

                    if (incognito
                        && SingletonManager.CurrentInstanceType == InstanceType.Master)
                        IncognitoManager.MoveRobloxFolderToCache();
                }

                var launcherProcess = StartRoblox(robloxPlayerBetaExePath, configuration.LaunchUri);

                if (launcherProcess == null)
                {
                    Log.Error("[!] Process.Start returned null — Roblox failed to launch.");

                    scope.MarkFailed();

                    Telemetry.Performance.RecordCounter("roblox.launch.outcome", new Dictionary<string, object> { ["value"] = "ProcessStartFailed" });

                    return false;
                }

                WaitForInputIdle(launcherProcess);

                Telemetry.Performance.RecordCounter("roblox.launch.outcome", new Dictionary<string, object> { ["value"] = "Success" });

                if (singletonCaptured)
                    WaitForGameExit(existingPids);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Unhandled exception during Roblox launch: {0}.", ex.Message);

                scope.MarkFailed();

                Telemetry.Performance.RecordCounter("roblox.launch.outcome", new Dictionary<string, object> { ["value"] = "Exception" });

                return false;
            }
            finally
            {
                if (singletonCaptured)
                    SingletonManager.ReleaseSingleton();
            }
        }
    }
}
