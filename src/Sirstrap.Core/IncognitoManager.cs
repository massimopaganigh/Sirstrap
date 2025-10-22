namespace Sirstrap.Core
{
    /// <summary>
    /// Manages Roblox folder isolation in Incognito mode by moving the Roblox folder
    /// to a temporary cache location and restoring it when needed.
    /// </summary>
    public static class IncognitoManager
    {
        private static readonly string _robloxFolderPath;
        private static readonly string _incognitoCachePath;
        private static readonly Lock _lockObject = new();
        private static bool _isRobloxFolderMoved = false;

        static IncognitoManager()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _robloxFolderPath = Path.Combine(localAppData, "Roblox");
            _incognitoCachePath = Path.Combine(localAppData, "Sirstrap", "IncognitoCache", "Roblox");

            // Subscribe to instance type changes to restore folder when Master is released
            SingletonManager.InstanceTypeChanged += OnInstanceTypeChanged;
        }

        /// <summary>
        /// Moves the Roblox folder to the Incognito cache location.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the folder was successfully moved or was already moved;
        /// <c>false</c> if an error occurred.
        /// </returns>
        public static bool MoveRobloxFolderToCache()
        {
            lock (_lockObject)
            {
                try
                {
                    if (_isRobloxFolderMoved)
                    {
                        Log.Information("[*] Roblox folder already moved to Incognito cache.");
                        return true;
                    }

                    if (!Directory.Exists(_robloxFolderPath))
                    {
                        Log.Information("[*] Roblox folder does not exist, nothing to move.");
                        _isRobloxFolderMoved = false;
                        return true;
                    }

                    // Ensure the parent directory exists
                    string? incognitoCacheParent = Path.GetDirectoryName(_incognitoCachePath);
                    if (incognitoCacheParent != null && !Directory.Exists(incognitoCacheParent))
                    {
                        Directory.CreateDirectory(incognitoCacheParent);
                    }

                    // Remove existing cache if present
                    if (Directory.Exists(_incognitoCachePath))
                    {
                        Log.Information("[*] Removing existing Incognito cache...");
                        Directory.Delete(_incognitoCachePath, true);
                    }

                    Log.Information("[*] Moving Roblox folder to Incognito cache: {0} -> {1}", _robloxFolderPath, _incognitoCachePath);

                    Directory.Move(_robloxFolderPath, _incognitoCachePath);

                    _isRobloxFolderMoved = true;

                    Log.Information("[*] Roblox folder successfully moved to Incognito cache.");

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Error occurred while moving Roblox folder to cache: {0}.", ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Restores the Roblox folder from the Incognito cache location.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the folder was successfully restored or was already restored;
        /// <c>false</c> if an error occurred.
        /// </returns>
        public static bool RestoreRobloxFolderFromCache()
        {
            lock (_lockObject)
            {
                try
                {
                    if (!_isRobloxFolderMoved)
                    {
                        Log.Information("[*] Roblox folder was not moved, nothing to restore.");
                        return true;
                    }

                    if (!Directory.Exists(_incognitoCachePath))
                    {
                        Log.Warning("[*] Incognito cache does not exist, nothing to restore.");
                        _isRobloxFolderMoved = false;
                        return true;
                    }

                    // Remove existing Roblox folder if present (shouldn't happen, but just in case)
                    if (Directory.Exists(_robloxFolderPath))
                    {
                        Log.Warning("[*] Roblox folder already exists, removing it before restore...");
                        Directory.Delete(_robloxFolderPath, true);
                    }

                    Log.Information("[*] Restoring Roblox folder from Incognito cache: {0} -> {1}", _incognitoCachePath, _robloxFolderPath);

                    Directory.Move(_incognitoCachePath, _robloxFolderPath);

                    _isRobloxFolderMoved = false;

                    Log.Information("[*] Roblox folder successfully restored from Incognito cache.");

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Error occurred while restoring Roblox folder from cache: {0}.", ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Handles instance type changes to automatically restore the Roblox folder
        /// when the instance is no longer Master.
        /// </summary>
        private static void OnInstanceTypeChanged(object? sender, InstanceType newType)
        {
            // Restore folder when instance is no longer Master
            if (newType != InstanceType.Master && _isRobloxFolderMoved)
            {
                Log.Information("[*] Instance type changed from Master to {0}, restoring Roblox folder...", newType);
                RestoreRobloxFolderFromCache();
            }
        }
    }
}
