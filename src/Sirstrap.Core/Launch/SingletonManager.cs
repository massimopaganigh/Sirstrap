namespace Sirstrap.Core.Launch
{
    public sealed class SingletonManager(IRobloxProcessService robloxProcessService) : ISingletonManager
    {
        private const string ROBLOX_MUTEX_NAME = "ROBLOX_singletonMutex";

        private InstanceType _currentInstanceType = InstanceType.None;
        private readonly Lock _lockObject = new();
        private Mutex? _robloxMutex;

        public event EventHandler<InstanceType>? InstanceTypeChanged;

        public InstanceType CurrentInstanceType
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

                        Log.Information("[*] The instance type changed to {InstanceType}.", value);

                        InstanceTypeChanged?.Invoke(this, value);
                    }
            }
        }

        public bool HasCapturedSingleton => _robloxMutex != null;

        public bool CaptureSingleton()
        {
            Log.Information("[*] Capturing the Roblox singleton...");

            lock (_lockObject)
            {
                if (_robloxMutex != null)
                {
                    Log.Warning("[!] The Roblox singleton is already captured by this instance.");

                    return true;
                }
                try
                {
                    _robloxMutex = new Mutex(true, ROBLOX_MUTEX_NAME, out bool createdNew);

                    if (createdNew)
                    {
                        Log.Information("[*] Captured the Roblox singleton.");

                        CurrentInstanceType = InstanceType.Master;

                        robloxProcessService.KillAll();

                        return true;
                    }

                    Log.Warning("[!] Failed to capture the Roblox singleton, another instance is already running.");

                    ReleaseAsSlave();
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Warning(ex, "[!] Failed to capture the Roblox singleton, access was denied (another instance may be running at a different privilege level).");

                    ReleaseAsSlave();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Failed to capture the Roblox singleton.");

                    ReleaseAsSlave();
                }

                return false;
            }
        }

        public bool ReleaseSingleton()
        {
            Log.Information("[*] Releasing the Roblox singleton...");

            lock (_lockObject)
            {
                if (_robloxMutex == null)
                {
                    Log.Warning("[!] Failed to release the Roblox singleton, it is not currently captured.");

                    return false;
                }
                try
                {
                    _robloxMutex.Dispose();
                    _robloxMutex = null;

                    CurrentInstanceType = InstanceType.None;

                    Log.Information("[*] Released the Roblox singleton.");

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Failed to release the Roblox singleton.");

                    return false;
                }
            }
        }

        private void ReleaseAsSlave()
        {
            CurrentInstanceType = InstanceType.Slave;

            _robloxMutex?.Dispose();

            _robloxMutex = null;
        }
    }
}
