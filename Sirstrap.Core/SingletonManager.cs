using Serilog;

namespace Sirstrap.Core
{
    /// <summary>
    /// Provides functionality to ensure only one instance of the Roblox runs at a time
    /// by utilizing a system-wide mutex.
    /// </summary>
    public static class SingletonManager
    {
        private const string ROBLOX_MUTEX_NAME = "ROBLOX_singletonMutex";
        private static Mutex? _robloxMutex;
        private static readonly object _lockObject = new();

        /// <summary>
        /// Gets a value indicating whether the singleton has been successfully captured.
        /// </summary>
        /// <value>
        /// <c>true</c> if the mutex has been captured; otherwise, <c>false</c>.
        /// </value>
        public static bool HasCapturedSingleton => _robloxMutex != null;

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

                        return true;
                    }
                    else
                    {
                        Log.Warning("[!] Cannot to capture singleton - another instance is already running.");

                        _robloxMutex.Dispose();
                        _robloxMutex = null;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Error occurred while attempting to capture singleton: {0}.", ex.Message);
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
    }
}