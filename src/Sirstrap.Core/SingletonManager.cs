namespace Sirstrap.Core
{
    public static class SingletonManager
    {
        private const string ROBLOX_MUTEX_NAME = "ROBLOX_singletonMutex";

        private static InstanceType _currentInstanceType = InstanceType.None;
        private static readonly Lock _lockObject = new();
        private static Mutex? _robloxMutex;

        public static event EventHandler<InstanceType>? InstanceTypeChanged;

        private static void CloseAllRobloxInstances()
        {
            try
            {
                var processNames = new[] { "Roblox", "RobloxCrashHandler", "RobloxPlayerBeta" };

                foreach (var processName in processNames)
                {
                    foreach (var process in Process.GetProcessesByName(processName))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch (Exception ex)
                        {
                            // Process may have already exited or access may be denied
                            Log.Debug(ex, "[!] Failed to kill process {0}: {1}", processName, ex.Message);
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error closing Roblox instances: {0}", ex.Message);
            }
        }

        public static bool WaitForAllRobloxProcessesToExit(int timeoutMs = 5000)
        {
            try
            {
                var processNames = new[] { "Roblox", "RobloxCrashHandler", "RobloxPlayerBeta" };
                var startTime = DateTime.UtcNow;

                while ((DateTime.UtcNow - startTime).TotalMilliseconds < timeoutMs)
                {
                    bool anyRunning = false;

                    foreach (var processName in processNames)
                        if (Process.GetProcessesByName(processName).Length > 0)
                        {
                            anyRunning = true;

                            break;
                        }

                    if (!anyRunning)
                    {
                        Log.Information("[*] All Roblox processes have exited.");

                        return true;
                    }

                    Thread.Sleep(100);
                }

                Log.Warning("[*] Timeout waiting for Roblox processes to exit.");

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error while waiting for Roblox processes to exit: {0}.", ex.Message);

                return false;
            }
        }

        public static bool CaptureSingleton()
        {
            Log.Information("[*] Attempting to capture Roblox singleton...");

            lock (_lockObject)
            {
                if (_robloxMutex != null)
                {
                    Log.Warning("[*] Singleton already captured by this instance.");

                    return true;
                }
                try
                {
                    _robloxMutex = new Mutex(true, ROBLOX_MUTEX_NAME, out bool createdNew);

                    if (createdNew)
                    {
                        Log.Information("[*] Successfully captured Roblox singleton.");

                        CurrentInstanceType = InstanceType.Master;

                        CloseAllRobloxInstances();

                        return true;
                    }
                    else
                    {
                        Log.Warning("[*] Cannot to capture singleton - another instance is already running.");

                        CurrentInstanceType = InstanceType.Slave;

                        _robloxMutex.Dispose();
                        _robloxMutex = null;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Error occurred while attempting to capture singleton: {0}.", ex.Message);

                    CurrentInstanceType = InstanceType.Slave;
                }

                return false;
            }
        }

        public static bool ReleaseSingleton()
        {
            Log.Information("[*] Attempting to release Roblox singleton...");

            lock (_lockObject)
            {
                if (_robloxMutex == null)
                {
                    Log.Warning("[*] Cannot release singleton - not currently captured.");

                    return false;
                }
                try
                {
                    _robloxMutex.Dispose();
                    _robloxMutex = null;

                    CurrentInstanceType = InstanceType.None;

                    Log.Information("[*] Successfully released Roblox singleton.");

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Error occurred while attempting to release singleton: {0}.", ex.Message);

                    return false;
                }
            }
        }

        public static InstanceType CurrentInstanceType
        {
            get
            {
                lock (_lockObject)
                    return _currentInstanceType;
            }
            private set
            {
                lock (_lockObject)
                    if (_currentInstanceType != value)
                    {
                        _currentInstanceType = value;

                        Log.Information("[*] Instance type changed to: {0}.", value);

                        InstanceTypeChanged?.Invoke(null, value);
                    }
            }
        }

        public static bool HasCapturedSingleton => _robloxMutex != null;
    }
}
