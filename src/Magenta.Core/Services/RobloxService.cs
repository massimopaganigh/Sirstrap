namespace Magenta.Core.Services
{
    public class RobloxService : IRobloxService
    {
        private Mutex? _mutex;

        public bool KillRoblox()
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                Log.Information("[{0}.{1}] Killing Roblox...", nameof(RobloxService), nameof(KillRoblox));

                string[] processNames = ["Roblox", "RobloxPlayerBeta"];

                foreach (string processName in processNames)
                    foreach (Process process in Process.GetProcessesByName(processName))
                    {
                        process.Kill();
                        process.WaitForExit();
                    }

                Log.Information("[{0}.{1}] Done in {2} ms.", nameof(RobloxService), nameof(KillRoblox), stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[{0}.{1}] Exception message: {2}", nameof(RobloxService), nameof(KillRoblox), ex.Message);

                return false;
            }
        }

        public bool StartRoblox(string startUri)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                Log.Information("[{0}.{1}] Starting Roblox...", nameof(RobloxService), nameof(StartRoblox), startUri);

                if (_mutex == null
                    && KillRoblox())
                    _mutex = new Mutex(true, "ROBLOX_singletonMutex");

                Process process = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        Arguments = $"/c start {startUri}",
                        CreateNoWindow = true,
                        FileName = "cmd.exe",
                        UseShellExecute = true
                    }
                };

                process.Start();
                process.WaitForInputIdle();

                Log.Information("[{0}.{1}] Done in {2} ms.", nameof(RobloxService), nameof(StartRoblox), stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[{0}.{1}] Exception message: {2}", nameof(RobloxService), nameof(StartRoblox), ex.Message);

                return false;
            }
        }
    }
}
