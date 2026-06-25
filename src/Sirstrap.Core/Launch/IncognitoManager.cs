namespace Sirstrap.Core.Launch
{
    public sealed class IncognitoManager : IIncognitoManager
    {
        private readonly string _incognitoCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "IncognitoCache", "Roblox");
        private bool _isRobloxFolderMoved;
        private readonly Lock _lockObject = new();
        private readonly string _robloxFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox");
        private readonly IRobloxProcessService _robloxProcessService;

        public IncognitoManager(ISingletonManager singletonManager, IRobloxProcessService robloxProcessService)
        {
            _robloxProcessService = robloxProcessService;

            singletonManager.InstanceTypeChanged += OnInstanceTypeChanged;
        }

        public bool MoveRobloxFolderToCache()
        {
            lock (_lockObject)
            {
                try
                {
                    if (_isRobloxFolderMoved)
                    {
                        Log.Information("[*] The Roblox folder is already in the Incognito cache.");

                        return true;
                    }

                    if (!Directory.Exists(_robloxFolderPath))
                    {
                        Log.Information("[*] The Roblox folder does not exist, nothing to move.");

                        _isRobloxFolderMoved = false;

                        return true;
                    }

                    var incognitoCacheParent = Path.GetDirectoryName(_incognitoCachePath);

                    if (incognitoCacheParent != null
                        && !Directory.Exists(incognitoCacheParent))
                        Directory.CreateDirectory(incognitoCacheParent);

                    if (Directory.Exists(_incognitoCachePath))
                    {
                        Log.Information("[*] Removing the existing Incognito cache...");

                        FileSystemOperations.DeleteDirectory(_incognitoCachePath);
                    }

                    _robloxProcessService.WaitForExit();

                    Log.Information("[*] Moving the Roblox folder from {RobloxFolderPath} to the Incognito cache {IncognitoCachePath}...", _robloxFolderPath, _incognitoCachePath);

                    FileSystemOperations.MoveDirectory(_robloxFolderPath, _incognitoCachePath);

                    _isRobloxFolderMoved = true;

                    Log.Information("[*] Moved the Roblox folder to the Incognito cache.");

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Failed to move the Roblox folder to the Incognito cache.");

                    return false;
                }
            }
        }

        public bool RestoreRobloxFolderFromCache()
        {
            lock (_lockObject)
            {
                try
                {
                    if (!_isRobloxFolderMoved)
                    {
                        Log.Information("[*] The Roblox folder was not moved, nothing to restore.");

                        return true;
                    }

                    if (!Directory.Exists(_incognitoCachePath))
                    {
                        Log.Warning("[!] The Incognito cache does not exist, nothing to restore.");

                        _isRobloxFolderMoved = false;

                        return true;
                    }

                    if (Directory.Exists(_robloxFolderPath))
                    {
                        Log.Warning("[!] The Roblox folder already exists, removing it before the restore...");

                        _robloxProcessService.WaitForExit();

                        FileSystemOperations.DeleteDirectory(_robloxFolderPath);
                    }

                    Log.Information("[*] Restoring the Roblox folder from the Incognito cache {IncognitoCachePath} to {RobloxFolderPath}...", _incognitoCachePath, _robloxFolderPath);

                    FileSystemOperations.MoveDirectory(_incognitoCachePath, _robloxFolderPath);

                    _isRobloxFolderMoved = false;

                    Log.Information("[*] Restored the Roblox folder from the Incognito cache.");

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Failed to restore the Roblox folder from the Incognito cache.");

                    return false;
                }
            }
        }

        private void OnInstanceTypeChanged(object? sender, InstanceType newType)
        {
            if (newType != InstanceType.Master
                && _isRobloxFolderMoved)
            {
                Log.Information("[*] The instance type changed from Master to {InstanceType}, restoring the Roblox folder...", newType);

                RestoreRobloxFolderFromCache();
            }
        }
    }
}
