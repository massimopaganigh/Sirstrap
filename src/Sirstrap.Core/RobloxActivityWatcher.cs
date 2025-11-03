namespace Sirstrap.Core
{
    public class RobloxActivityWatcher
    {
        private const string ROBLOX_LOGS_FOLDER = "Roblox\\logs";
        private FileSystemWatcher? _logWatcher;
        private string? _currentLogFile;
        private long _lastPosition;
        private string? _currentServerLocation;

        public event EventHandler<string>? ServerLocationChanged;

        public string CurrentServerLocation => _currentServerLocation ?? "località non disponibile";

        public void StartWatching()
        {
            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var robloxLogsPath = Path.Combine(localAppData, ROBLOX_LOGS_FOLDER);

                if (!Directory.Exists(robloxLogsPath))
                {
                    Log.Warning("[!] Roblox logs directory not found: {0}", robloxLogsPath);
                    return;
                }

                // Find the most recent log file
                var logFiles = Directory.GetFiles(robloxLogsPath, "*.log", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToArray();

                if (logFiles.Length > 0)
                {
                    _currentLogFile = logFiles[0];
                    _lastPosition = new FileInfo(_currentLogFile).Length;
                    Log.Debug("[*] Monitoring Roblox log file: {0}", _currentLogFile);
                }

                // Set up file system watcher for new log entries
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

        private void OnLogFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                _currentLogFile = e.FullPath;
                _lastPosition = 0;
                Log.Debug("[*] New Roblox log file created: {0}", e.FullPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error handling log file creation: {0}", ex.Message);
            }
        }

        private void OnLogFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (_currentLogFile == null || e.FullPath != _currentLogFile)
                    return;

                ProcessNewLogEntries(e.FullPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error processing log file changes: {0}", ex.Message);
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
                    // Look for server connection patterns in Roblox logs
                    // Common patterns: "Game server IP:", "Connected to", "UDMUX", "GameHost"
                    var ipAddress = ExtractServerIp(line);

                    if (!string.IsNullOrEmpty(ipAddress))
                    {
                        Log.Debug("[*] Detected Roblox server IP: {0}", ipAddress);
                        _ = UpdateServerLocationAsync(ipAddress);
                    }
                }

                _lastPosition = fileStream.Position;
            }
            catch (IOException)
            {
                // File may be locked, ignore and try next time
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error reading log file: {0}", ex.Message);
            }
        }

        private string? ExtractServerIp(string logLine)
        {
            try
            {
                // Look for IP address patterns in various log formats
                // Pattern 1: "UDMUX" followed by IP:port
                if (logLine.Contains("UDMUX") || logLine.Contains("GameHost"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        logLine,
                        @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b"
                    );

                    if (match.Success)
                        return match.Groups[1].Value;
                }

                // Pattern 2: Direct IP address mention with context
                if (logLine.Contains("server", StringComparison.OrdinalIgnoreCase) ||
                    logLine.Contains("connect", StringComparison.OrdinalIgnoreCase))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        logLine,
                        @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b"
                    );

                    if (match.Success)
                    {
                        var ip = match.Groups[1].Value;
                        // Validate it's not a local IP
                        if (!ip.StartsWith("127.") && !ip.StartsWith("192.168.") && !ip.StartsWith("10."))
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
    }
}
