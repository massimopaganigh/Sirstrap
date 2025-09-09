namespace Sirstrap.UI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private const int LOG_ACTIVITY_THRESHOLD_SECONDS = 30;
        private const int MAX_POLLING_INTERVAL = 10000;
        private const int MIN_POLLING_INTERVAL = 100;

        [ObservableProperty]
        private string _currentFullVersion = SirstrapUpdateService.GetCurrentFullVersion();

        private int _currentPollingInterval = MIN_POLLING_INTERVAL;

        private bool _isMinimized = false;

        [ObservableProperty]
        private bool _isRobloxRunning;

        [ObservableProperty]
        private LogEventLevel? _lastLogLevel;

        [ObservableProperty]
        private string _lastLogMessage = "...";

        private DateTimeOffset? _lastLogReceived;

        [ObservableProperty]
        private DateTimeOffset? _lastLogTimestamp;

        private readonly Timer _logPollingTimer;

        private Window? _mw;

        [ObservableProperty]
        private int _robloxProcessCount;

        public MainWindowViewModel()
        {
            _logPollingTimer = new(_currentPollingInterval);
            _logPollingTimer.Elapsed += (s, e) => GetLastLogFromSink();
            _logPollingTimer.Start();

#if !DEBUG
            Task.Run(InitializeAsync);
#endif
        }

        #region PRIVATE METHODS
        private void GetLastLogFromSink()
        {
            try
            {
                if (!string.Equals(LastLogMessage, LastLogSink.LastLog)
                    && !string.IsNullOrWhiteSpace(LastLogSink.LastLog))
                {
                    LastLogMessage = LastLogSink.LastLog;
                    LastLogTimestamp = LastLogSink.LastLogTimestamp;
                    LastLogLevel = LastLogSink.LastLogLevel;

                    _lastLogReceived = DateTimeOffset.Now;
                }

                GetRobloxProcessCount();
                GetPollingInterval();
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetLastLogFromSink));

                //Environment.ExitCode = 1;
            }
        }

        private void GetPollingInterval()
        {
            try
            {
                bool hasRecentLogActivity = _lastLogReceived.HasValue && (DateTimeOffset.Now - _lastLogReceived.Value).TotalSeconds <= LOG_ACTIVITY_THRESHOLD_SECONDS;
                int newInterval = hasRecentLogActivity ? MIN_POLLING_INTERVAL : MAX_POLLING_INTERVAL;

                if (newInterval != _currentPollingInterval)
                {
                    _currentPollingInterval = newInterval;
                    _logPollingTimer.Interval = _currentPollingInterval;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetPollingInterval));

                //Environment.ExitCode = 1;
            }
        }

        private void GetRobloxProcessCount()
        {
            try
            {
                string[] robloxProcessNames =
                [
                    "RobloxPlayerBeta"
                ];
                int count = Process.GetProcesses().Count(x => robloxProcessNames.Any(y => string.Equals(x.ProcessName, y, StringComparison.OrdinalIgnoreCase)));

                RobloxProcessCount = count;
                IsRobloxRunning = count > 0 && SirstrapConfiguration.MultiInstance;

                if (_mw != null
                    && IsRobloxRunning
                    && _isMinimized == false)
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _mw.WindowState = WindowState.Minimized;
                    });

                    _isMinimized = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetRobloxProcessCount));

                //Environment.ExitCode = 1;
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                string logsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Logs");

                if (!Directory.Exists(logsDir))
                    Directory.CreateDirectory(logsDir);

                string logsPath = Path.Combine(logsDir, "SirstrapLog.txt");

                Log.Logger = new LoggerConfiguration().WriteTo.File(logsPath, fileSizeLimitBytes: 5 * 1024 * 1024, rollOnFileSizeLimit: true, retainedFileCountLimit: 5).WriteTo.LastLog().CreateLogger();

                SirstrapConfigurationService.LoadConfiguration();

                string[] args = Program.Args ?? [];

                RegistryManager.RegisterProtocolHandler("roblox-player", args);

                await new RobloxDownloader().ExecuteAsync(args, SirstrapType.UI);

                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(InitializeAsync));

                Environment.ExitCode = 1;
            }
            finally
            {
                await Log.CloseAndFlushAsync();

                Environment.Exit(Environment.ExitCode);
            }
        }

        [RelayCommand]
        private void OpenGitHub()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/massimopaganigh/Sirstrap",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(OpenGitHub));

                //Environment.ExitCode = 1;
            }
        }

        [RelayCommand]
        private void OpenSettings()
        {
            try
            {
                string settingsPath = SirstrapConfigurationService.GetConfigurationPath();

                if (File.Exists(settingsPath))
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = settingsPath,
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(OpenSettings));

                //Environment.ExitCode = 1;
            }
        }
        #endregion

        public void Dispose()
        {
            _logPollingTimer?.Stop();
            _logPollingTimer?.Dispose();
        }

        public void SetMainWindow(Window mw) => _mw = mw;
    }
}