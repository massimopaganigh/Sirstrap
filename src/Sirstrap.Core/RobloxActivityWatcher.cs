namespace Sirstrap.Core
{
    public partial class RobloxActivityWatcher
    {
        private string? _currentLogFile;
        private string? _currentServerLocation;
        private long _lastPosition;
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

        private void OnLogFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (_currentLogFile == null
                    || e.FullPath != _currentLogFile)
                    return;

                ProcessNewLogEntries(e.FullPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error processing log file changes: {0}", ex.Message);
            }
        }

        private void OnLogFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                _currentLogFile = e.FullPath;
                _lastPosition = 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error handling log file creation: {0}", ex.Message);
            }
        }

        private void ProcessNewLogEntries(string logFilePath)
        {
            try
            {
                using var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                fileStream.Seek(_lastPosition, SeekOrigin.Begin);

                using var reader = new StreamReader(fileStream);

                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    var ipAddress = ExtractServerIp(line);

                    if (!string.IsNullOrEmpty(ipAddress))
                        _ = UpdateServerLocationAsync(ipAddress);
                }

                _lastPosition = fileStream.Position;
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
                    _lastPosition = new FileInfo(_currentLogFile).Length;
                }

                _logWatcher = new FileSystemWatcher(robloxLogsPath)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                    Filter = "*.log",
                    EnableRaisingEvents = true
                };

                _logWatcher.Changed += OnLogFileChanged;
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
            if (_logWatcher != null)
            {
                _logWatcher.Changed -= OnLogFileChanged;
                _logWatcher.Created -= OnLogFileCreated;

                _logWatcher.Dispose();

                _logWatcher = null;

                Log.Information("[*] Stopped watching Roblox activity logs.");
            }
        }

        public string CurrentServerLocation => _currentServerLocation ?? "UNKNOWN";
    }
}
