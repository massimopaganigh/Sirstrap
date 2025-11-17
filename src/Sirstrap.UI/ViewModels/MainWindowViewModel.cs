namespace Sirstrap.UI.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private const int LOG_ACTIVITY_THRESHOLD_SECONDS = 30;
        private const int MAX_POLLING_INTERVAL = 10000;
        private const int MIN_POLLING_INTERVAL = 100;

        [ObservableProperty]
        private string _currentFullVersion = SirstrapUpdateService.GetCurrentFullVersion();

        [ObservableProperty]
        private int _currentPollingInterval = MIN_POLLING_INTERVAL;

        [ObservableProperty]
        private bool _isMinimized;

        [ObservableProperty]
        private bool _isRobloxRunning;

        [ObservableProperty]
        private LogEventLevel? _lastLogLevel;

        [ObservableProperty]
        private string _lastLogMessage = "...";

        [ObservableProperty]
        private DateTimeOffset? _lastLogReceived;

        [ObservableProperty]
        private DateTimeOffset? _lastLogTimestamp;

        [ObservableProperty]
        private Timer _logPollingTimer;

        private readonly RobloxDownloader _robloxDownloader = new();

        [ObservableProperty]
        private int _robloxProcesses;

        public MainWindowViewModel()
        {
            _logPollingTimer = new(_currentPollingInterval);

            _logPollingTimer.Elapsed += (s, e) => GetLastLogFromSink();

            _logPollingTimer.Start();

#if !DEBUG
            Task.Run(RunAsync);
#endif
        }

        private void GetLastLogFromSink()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(LastLogSink.LastLog)
                    && !string.Equals(LastLogMessage, LastLogSink.LastLog))
                {
                    LastLogReceived = DateTimeOffset.Now;
                    LastLogMessage = LastLogSink.LastLog;
                    LastLogTimestamp = LastLogSink.LastLogTimestamp;
                    LastLogLevel = LastLogSink.LastLogLevel;
                }

                GetRobloxProcesses();
                GetPollingInterval();
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetLastLogFromSink));
            }
        }

        private void GetPollingInterval()
        {
            try
            {
                var targetPollingInterval = LastLogReceived.HasValue && (DateTimeOffset.Now - LastLogReceived.Value).TotalSeconds <= LOG_ACTIVITY_THRESHOLD_SECONDS ? MIN_POLLING_INTERVAL : MAX_POLLING_INTERVAL;

                if (targetPollingInterval != CurrentPollingInterval)
                {
                    CurrentPollingInterval = targetPollingInterval;
                    LogPollingTimer.Interval = CurrentPollingInterval;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetPollingInterval));
            }
        }

        private void GetRobloxProcesses()
        {
            try
            {
                RobloxProcesses = Process.GetProcessesByName("RobloxPlayerBeta").Length;
                IsRobloxRunning = RobloxProcesses > 0 && SirstrapConfiguration.MultiInstance;

                var mainWindow = GetMainWindow();

                if (mainWindow != null
                    && IsRobloxRunning
                    && !IsMinimized)
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        mainWindow.WindowState = WindowState.Minimized;
                    });

                    IsMinimized = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetRobloxProcesses));
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
            }
        }

        [RelayCommand]
        private void OpenIssue()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/massimopaganigh/Sirstrap/issues/new",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(OpenIssue));
            }
        }

        [RelayCommand]
        private void OpenSettings()
        {
            try
            {
                //var settingsFilePath = SirstrapConfigurationService.GetConfigurationPath();

                //if (File.Exists(settingsFilePath))
                //    Process.Start(new ProcessStartInfo
                //    {
                //        FileName = settingsFilePath,
                //        UseShellExecute = true
                //    });

                new SettingsWindow { DataContext = new SettingsWindowViewModel() }.ShowDialog(GetMainWindow()!);
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(OpenSettings));
            }
        }

        [RelayCommand]
        private async Task RunAsync()
        {
            try
            {
                var logsDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Logs");

                if (!Directory.Exists(logsDirectoryPath))
                    Directory.CreateDirectory(logsDirectoryPath);

                Log.Logger = new LoggerConfiguration().WriteTo.File(Path.Combine(logsDirectoryPath, "SirstrapLog.txt"), fileSizeLimitBytes: 5 * 1024 * 1024, rollOnFileSizeLimit: true, retainedFileCountLimit: 5).WriteTo.LastLog().CreateLogger();

                SirstrapConfigurationService.LoadSettings();

                var args = Program.Args ?? [];

                RegistryManager.RegisterProtocolHandler("roblox-player", args);

                await _robloxDownloader.ExecuteAsync(args, SirstrapType.UI);

                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(RunAsync));

                Environment.ExitCode = 1;
            }
            finally
            {
                await Log.CloseAndFlushAsync();

                Environment.Exit(Environment.ExitCode);
            }
        }
    }
}
