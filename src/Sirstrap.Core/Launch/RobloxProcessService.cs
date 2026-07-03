namespace Sirstrap.Core.Launch
{
    public sealed class RobloxProcessService : IRobloxProcessService
    {
        private static readonly string[] _gameProcessNames = ["RobloxPlayerBeta"];
        private static readonly string[] _processNames = ["RobloxCrashHandler", "RobloxPlayerBeta"];

        public bool AnyGameProcessRunning(IEnumerable<int> processIds)
        {
            var running = SnapshotGameProcessIds();

            return processIds.Any(running.Contains);
        }

        public List<int> FindNewGameProcessIds(HashSet<int> knownIds, int attempts = 20)
        {
            for (var i = 0; i < attempts; i++)
            {
                var ids = SnapshotGameProcessIds().Where(id => !knownIds.Contains(id)).ToList();

                if (ids.Count > 0)
                    return ids;

                Thread.Sleep(100);
            }

            return [];
        }

        public int GetRunningGameProcessCount() => SnapshotGameProcessIds().Count;

        public void KillAll()
        {
            try
            {
                foreach (var process in GetRunning(_processNames))
                {
                    try
                    {
                        Log.Information("[*] Killing the Roblox process {ProcessName} (PID {ProcessId}, started {StartTime}, {MemoryMb} MB, title {MainWindowTitle})...", process.ProcessName, process.Id, process.StartTime.ToString("yyyy-MM-dd HH:mm:ss"), process.WorkingSet64 / 1024 / 1024, process.MainWindowTitle);

                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex, "[!] Failed to kill the Roblox process {ProcessName}.", process.ProcessName);
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to close the Roblox processes.");
            }
        }

        public void LogRunningGameProcesses()
        {
            foreach (var process in GetRunning(_gameProcessNames))
            {
                try
                {
                    Log.Information("[*] The Roblox process {ProcessName} is running (PID {ProcessId}, started {StartTime}, {MemoryMb} MB, title {MainWindowTitle}).", process.ProcessName, process.Id, process.StartTime.ToString("yyyy-MM-dd HH:mm:ss"), process.WorkingSet64 / 1024 / 1024, process.MainWindowTitle);
                }
                finally
                {
                    process.Dispose();
                }
            }
        }

        public HashSet<int> SnapshotGameProcessIds()
        {
            HashSet<int> ids = [];

            foreach (var process in GetRunning(_gameProcessNames))
            {
                ids.Add(process.Id);

                process.Dispose();
            }

            return ids;
        }

        public bool WaitForExit(int timeoutMs = 5000)
        {
            try
            {
                var startTime = DateTime.UtcNow;

                while ((DateTime.UtcNow - startTime).TotalMilliseconds < timeoutMs)
                {
                    if (!AnyRunning(_processNames))
                    {
                        Log.Information("[*] All the Roblox processes have exited.");

                        return true;
                    }

                    Thread.Sleep(100);
                }

                Log.Warning("[!] Timed out waiting for the Roblox processes to exit.");

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to wait for the Roblox processes to exit.");

                return false;
            }
        }

        private static bool AnyRunning(string[] names)
        {
            var any = false;

            foreach (var process in GetRunning(names))
            {
                any = true;

                process.Dispose();
            }

            return any;
        }

        private static IEnumerable<Process> GetRunning(string[] names) => names.SelectMany(Process.GetProcessesByName);
    }
}
