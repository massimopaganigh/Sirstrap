namespace Sirstrap.Core
{
    public partial class RobloxActivityWatcher
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private string? _currentLogFile;
        private string? _currentServerLocation;
        private Task? _logReadingTask;
        private FileSystemWatcher? _logWatcher;

        public event EventHandler<string>? ServerLocationChanged;

        [GeneratedRegex(@"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b")]
        private static partial Regex AltServerIpRegex();

        private static string? ExtractServerIp(string logLine)
        {
            try
            {
                if (logLine.Contains("UDMUX")
                    || logLine.Contains("GameHost"))
                {
                    var match = ServerIpRegex().Match(logLine);

                    if (match.Success)
                        return match.Groups[1].Value;
                }

                if (logLine.Contains("server", StringComparison.OrdinalIgnoreCase)
                    || logLine.Contains("connect", StringComparison.OrdinalIgnoreCase))
                {
                    var match = AltServerIpRegex().Match(logLine);

                    if (match.Success)
                    {
                        var ip = match.Groups[1].Value;

                        if (!ip.StartsWith("127.")
                            && !ip.StartsWith("192.168.")
                            && !ip.StartsWith("10.")
                            && !IsPrivateIpRange172(ip))
                            return ip;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error extracting IP from log line: {0}", ex.Message);
            }

            return null;
        }

        private static bool IsPrivateIpRange172(string ip)
        {
            if (!ip.StartsWith("172."))
                return false;

            var parts = ip.Split('.');

            if (parts.Length >= 2
                && int.TryParse(parts[1], out var secondOctet))
                return secondOctet >= 16 && secondOctet <= 31;

            return false;
        }

        private void OnLogFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _logReadingTask?.Wait(TimeSpan.FromSeconds(2));

                _currentLogFile = e.FullPath;
                _cancellationTokenSource = new CancellationTokenSource();
                _logReadingTask = Task.Run(() => ReadLogFileAsync(_currentLogFile, _cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error handling log file creation: {0}", ex.Message);
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
                    string? line = await reader.ReadLineAsync(cancellationToken);

                    if (line is null)
                        await Task.Delay(1000, cancellationToken);
                    else
                    {
                        var ipAddress = ExtractServerIp(line);

                        if (!string.IsNullOrEmpty(ipAddress))
                            _ = UpdateServerLocationAsync(ipAddress);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error reading log file: {0}", ex.Message);
            }
        }

        [GeneratedRegex(@"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b")]
        private static partial Regex ServerIpRegex();

        private async Task UpdateServerLocationAsync(string ipAddress)
        {
            try
            {
                var location = await ServerLocationService.GetServerLocationAsync(ipAddress);

                if (location != _currentServerLocation)
                {
                    _currentServerLocation = location;

                    ServerLocationChanged?.Invoke(this, location);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to update server location: {0}", ex.Message);
            }
        }

        public void StartWatching()
        {
            try
            {
                var robloxLogsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "logs");

                if (!Directory.Exists(robloxLogsPath))
                    return;

                var logFiles = Directory.GetFiles(robloxLogsPath, "*.log", SearchOption.TopDirectoryOnly).OrderByDescending(x => File.GetLastWriteTime(x)).ToArray();

                if (logFiles.Length > 0)
                {
                    _currentLogFile = logFiles[0];
                    _cancellationTokenSource = new CancellationTokenSource();
                    _logReadingTask = Task.Run(() => ReadLogFileAsync(_currentLogFile, _cancellationTokenSource.Token));
                }

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
                Log.Error(ex, "[!] Failed to start Roblox activity watcher: {0}", ex.Message);
            }
        }

        public void StopWatching()
        {
            try
            {
                _cancellationTokenSource?.Cancel();

                if (_logReadingTask != null)
                {
                    _logReadingTask.Wait(TimeSpan.FromSeconds(5));

                    _logReadingTask = null;
                }

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
                Log.Error(ex, "[!] Error stopping Roblox activity watcher: {0}", ex.Message);
            }
        }

        public string CurrentServerLocation => _currentServerLocation ?? "UNKNOWN";
    }
}
