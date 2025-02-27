using Serilog;

namespace Sirstrap.Core
{
    /// <summary>
    /// Provides methods to manage the Roblox singleton mutex for enabling multi-instance capability.
    /// </summary>
    public static class SingletonManager
    {
        private static Mutex? _robloxMutex;

        /// <summary>
        /// Gets a value indicating whether this program has captured the Roblox singleton mutex.
        /// </summary>
        public static bool HasCapturedSingleton => _robloxMutex != null;

        /// <summary>
        /// Attempts to capture the Roblox singleton mutex to enable multi-instance capability.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the mutex was successfully captured; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method should be called before launching Roblox to ensure proper multi-instance support.
        /// </remarks>
        public static bool CaptureSingleton()
        {
            if (_robloxMutex == null)
            {
                try
                {
                    _robloxMutex = new Mutex(true, "ROBLOX_singletonMutex", out bool createdNew);

                    if (createdNew)
                    {
                        Log.Information("[*] Roblox singleton mutex captured.");

                        return true;
                    }
                    else
                    {
                        _robloxMutex = null;

                        Log.Warning("[*] Failed to capture Roblox singleton mutex. Multi-instance may not work.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Error capturing Roblox singleton mutex: {0}", ex.Message);
                }
            }
            return false;
        }

        /// <summary>
        /// Releases the Roblox singleton mutex if it was previously captured.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the mutex was successfully released; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method should be called after Roblox has closed to clean up resources.
        /// </remarks>
        public static bool ReleaseSingleton()
        {
            if (_robloxMutex != null)
            {
                try
                {
                    _robloxMutex.Close();
                    _robloxMutex.Dispose();
                    _robloxMutex = null;

                    Log.Information("[*] Roblox singleton mutex released.");

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Error releasing Roblox singleton mutex: {0}", ex.Message);
                }
            }

            return false;
        }
    }
}