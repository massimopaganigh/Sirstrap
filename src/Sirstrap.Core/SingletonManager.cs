namespace Sirstrap.Core
{
    /// <summary>
    /// Defines the type of instance in a multi-instance environment.
    /// </summary>
    public enum InstanceType
    {
        /// <summary>
        /// Instance type not yet determined or multi-instance mode is disabled.
        /// </summary>
        None,

        /// <summary>
        /// Master instance - has captured the Roblox singleton and manages the primary Roblox process.
        /// </summary>
        Master,

        /// <summary>
        /// Slave instance - failed to capture the singleton and operates alongside a master instance.
        /// </summary>
        Slave
    }

    /// <summary>
    /// Provides functionality to ensure only one instance of the Roblox runs at a time
    /// by utilizing a system-wide mutex.
    /// </summary>
    public static class SingletonManager
    {
        private const string ROBLOX_MUTEX_NAME = "ROBLOX_singletonMutex";
        private static Mutex? _robloxMutex;
        private static readonly Lock _lockObject = new();
        private static InstanceType _currentInstanceType = InstanceType.None;

        /// <summary>
        /// Occurs when the instance type changes.
        /// </summary>
        public static event EventHandler<InstanceType>? InstanceTypeChanged;

        /// <summary>
        /// Gets a value indicating whether the singleton has been successfully captured.
        /// </summary>
        /// <value>
        /// <c>true</c> if the mutex has been captured; otherwise, <c>false</c>.
        /// </value>
        public static bool HasCapturedSingleton => _robloxMutex != null;

        /// <summary>
        /// Gets the current instance type (Master, Slave, or None).
        /// </summary>
        /// <value>
        /// The current instance type.
        /// </value>
        public static InstanceType CurrentInstanceType
        {
            get
            {
                lock (_lockObject)
                {
                    return _currentInstanceType;
                }
            }
            private set
            {
                lock (_lockObject)
                {
                    if (_currentInstanceType != value)
                    {
                        _currentInstanceType = value;
                        Log.Information("[*] Instance type changed to: {0}", value);
                        InstanceTypeChanged?.Invoke(null, value);
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to capture the Roblox singleton by acquiring the mutex.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the singleton was successfully captured or was already captured by this instance;
        /// <c>false</c> if another instance of the Roblox is already running.
        /// </returns>
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

                        // This instance becomes the Master
                        CurrentInstanceType = InstanceType.Master;

                        CloseAllRobloxInstances();

                        return true;
                    }
                    else
                    {
                        Log.Warning("[!] Cannot to capture singleton - another instance is already running.");

                        // This instance becomes a Slave
                        CurrentInstanceType = InstanceType.Slave;

                        _robloxMutex.Dispose();
                        _robloxMutex = null;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Error occurred while attempting to capture singleton: {0}.", ex.Message);

                    // On error, mark as Slave (failed to capture)
                    CurrentInstanceType = InstanceType.Slave;
                }

                return false;
            }
        }

        /// <summary>
        /// Releases the Roblox singleton by releasing the mutex.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the singleton was successfully released;
        /// <c>false</c> if the singleton was not captured by this instance or an error occurred.
        /// </returns>
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

                    // Reset instance type when releasing singleton
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

        /// <summary>
        /// Closes all running instances of Roblox by killing the processes directly.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the operation was successful; otherwise, <c>false</c>.
        /// </returns>
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
                            // Kill the process directly
                            process.Kill();
                        }
                        catch
                        {
                            // No logging of exceptions
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
    }
}