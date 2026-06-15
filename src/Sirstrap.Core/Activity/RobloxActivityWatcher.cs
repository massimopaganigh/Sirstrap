namespace Sirstrap.Core.Activity
{
    public class RobloxActivityWatcher(IServerLocationService serverLocationService)
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private string? _currentServerLocation;
        private Task? _logReadingTask;
        private FileSystemWatcher? _logWatcher;

        public event EventHandler<string>? ServerLocationChanged;

        public void StartWatching()
        {
            try
            {
                var robloxLogsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "logs");

                if (!Directory.Exists(robloxLogsPath))
                    return;

                var latestLogFile = Directory.GetFiles(robloxLogsPath, "*.log", SearchOption.TopDirectoryOnly).OrderByDescending(File.GetLastWriteTime).FirstOrDefault();

                if (latestLogFile != null)
                    StartReading(latestLogFile);

                _logWatcher = new FileSystemWatcher(robloxLogsPath)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
                    Filter = "*.log",
                    EnableRaisingEvents = true
                };

                _logWatcher.Created += OnLogFileCreated;

                Log.Information("[*] Started watching Roblox activity logs.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to start the Roblox activity watcher.");
            }
        }

        public void StopWatching()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _logReadingTask?.Wait(TimeSpan.FromSeconds(5));

                _logReadingTask = null;

                _cancellationTokenSource?.Dispose();

                _cancellationTokenSource = null;

                if (_logWatcher != null)
                {
                    _logWatcher.Created -= OnLogFileCreated;

                    _logWatcher.Dispose();

                    _logWatcher = null;
                }

                Log.Information("[*] Stopped watching Roblox activity logs.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to stop the Roblox activity watcher.");
            }
        }

        public string CurrentServerLocation => _currentServerLocation ?? "UNKNOWN";

        #region PRIVATE METHODS
        private void OnLogFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _logReadingTask?.Wait(TimeSpan.FromSeconds(2));
                StartReading(e.FullPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to handle the creation of a Roblox log file.");
            }
        }

        private async Task ReadLogFileAsync(string logFilePath, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(500, cancellationToken);

                using var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var reader = new StreamReader(fileStream);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);

                    if (line is null)
                        await Task.Delay(1000, cancellationToken);
                    else
                    {
                        var ipAddress = RobloxLogParser.ExtractServerIp(line);

                        if (!string.IsNullOrEmpty(ipAddress))
                            _ = UpdateServerLocationAsync(ipAddress);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Log.Debug(ex, "[*] Stopped reading the Roblox log file {LogFilePath}.", logFilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to read the Roblox log file {LogFilePath}.", logFilePath);
            }
        }

        private void StartReading(string logFilePath)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _logReadingTask = Task.Run(() => ReadLogFileAsync(logFilePath, _cancellationTokenSource.Token));
        }

        private async Task UpdateServerLocationAsync(string ipAddress)
        {
            try
            {
                var location = await serverLocationService.GetServerLocationAsync(ipAddress);

                if (location != _currentServerLocation)
                {
                    _currentServerLocation = location;

                    ServerLocationChanged?.Invoke(this, location);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to update the server location for IP {IpAddress}.", ipAddress);
            }
        }
        #endregion
    }
}
