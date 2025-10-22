namespace Sirstrap.Core
{
    public static class SingletonManager
    {
        private const string ROBLOX_MUTEX_NAME = "ROBLOX_singletonMutex";

        private static InstanceType _currentInstanceType = InstanceType.None;
        private static readonly Lock _lockObject = new();
        private static Mutex? _robloxMutex;

        public static event EventHandler<InstanceType>? InstanceTypeChanged;

        private static bool CloseAllRobloxInstances()
        {
            try
            {
                // Get Roblox processes with their exact names
                var processNames = new[] { "Roblox", "RobloxCrashHandler", "RobloxPlayerBeta" };

                foreach (var processName in processNames)
                {
                    foreach (var process in Process.GetProcessesByName(processName))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }

                return true;
            }
            catch
            {
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
                        Log.Warning("[!] Cannot to capture singleton - another instance is already running.");

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
