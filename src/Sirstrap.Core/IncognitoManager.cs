namespace Sirstrap.Core
{
    public static class IncognitoManager
    {
        private static readonly string _robloxFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox");
        private static readonly string _incognitoCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "IncognitoCache", "Roblox");
        private static bool _isRobloxFolderMoved = false;
        private static readonly Lock _lockObject = new();

        static IncognitoManager()
        {
            SingletonManager.InstanceTypeChanged += OnInstanceTypeChanged;
        }

        private static void OnInstanceTypeChanged(object? sender, InstanceType newType)
        {
            if (newType != InstanceType.Master
                && _isRobloxFolderMoved)
            {
                Log.Information("[*] Instance type changed from Master to {0}, restoring Roblox folder...", newType);

                RestoreRobloxFolderFromCache();
            }
        }

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

                    var incognitoCacheParent = Path.GetDirectoryName(_incognitoCachePath);

                    if (incognitoCacheParent != null
                        && !Directory.Exists(incognitoCacheParent))
                        Directory.CreateDirectory(incognitoCacheParent);

                    if (Directory.Exists(_incognitoCachePath))
                    {
                        Log.Information("[*] Removing existing Incognito cache...");

                        _incognitoCachePath.BetterDirectoryDelete();
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

                    if (Directory.Exists(_robloxFolderPath))
                    {
                        Log.Warning("[*] Roblox folder already exists, removing it before restore...");

                        SingletonManager.WaitForAllRobloxProcessesToExit();

                        _robloxFolderPath.BetterDirectoryDelete();
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
    }
}
