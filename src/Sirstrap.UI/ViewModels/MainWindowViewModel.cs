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

        private readonly IpcService _ipcService = new();

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

        private readonly RobloxActivityWatcher _robloxActivityWatcher = new();
        private readonly RobloxDownloader _robloxDownloader = new();

        [ObservableProperty]
        private int _robloxProcesses;

        [ObservableProperty]
        private string _serverLocation = "...";

        [ObservableProperty]
        private bool _showServerLocation;

        private bool _wasRobloxRunning;

        public MainWindowViewModel()
        {
            _logPollingTimer = new(_currentPollingInterval);

            _logPollingTimer.Elapsed += (s, e) => GetLastLogFromSink();

            _logPollingTimer.Start();

            _robloxActivityWatcher.ServerLocationChanged += OnServerLocationChanged;

            Task.Run(RunAsync);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

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

                var robloxIsActuallyRunning = RobloxProcesses > 0;

                IsRobloxRunning = robloxIsActuallyRunning && SirstrapConfiguration.MultiInstance;
                ShowServerLocation = robloxIsActuallyRunning;

                if (robloxIsActuallyRunning
                    && !_wasRobloxRunning)
                {
                    _robloxActivityWatcher.StartWatching();

                    _wasRobloxRunning = true;
                }
                else if (!robloxIsActuallyRunning
                    && _wasRobloxRunning)
                {
                    _robloxActivityWatcher.StopWatching();

                    ServerLocation = "...";

                    _wasRobloxRunning = false;
                }

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

        private void OnServerLocationChanged(object? sender, string location) => Dispatcher.UIThread.InvokeAsync(() =>
        {
            ServerLocation = location;
        });

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
#if DEBUG
                AllocConsole();
#endif

                var logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Logs");

                if (!Directory.Exists(logsDirectory))
                    Directory.CreateDirectory(logsDirectory);

                var appGuid = Guid.NewGuid().ToString("N");

                Log.Logger = new LoggerConfiguration()
                    .Enrich.WithThreadId()
                    .Enrich.WithThreadName()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(Path.Combine(logsDirectory, $"SirstrapLog{appGuid}.txt"), outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] {Message:lj}{NewLine}{Exception}", fileSizeLimitBytes: 1_048_576, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
                    .WriteTo.File(Path.Combine(logsDirectory, $"SirstrapErrorsLog{appGuid}.txt"), restrictedToMinimumLevel: LogEventLevel.Error, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] {Message:lj}{NewLine}{Exception}", fileSizeLimitBytes: 1_048_576, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
                    .WriteTo.LastLog()
                    .CreateLogger();

                Log.Information(@"
   ▄████████  ▄█     ▄████████    ▄████████     ███        ▄████████    ▄████████    ▄███████▄
  ███    ███ ███    ███    ███   ███    ███ ▀█████████▄   ███    ███   ███    ███   ███    ███
  ███    █▀  ███▌   ███    ███   ███    █▀     ▀███▀▀██   ███    ███   ███    ███   ███    ███
  ███        ███▌  ▄███▄▄▄▄██▀   ███            ███   ▀  ▄███▄▄▄▄██▀   ███    ███   ███    ███
▀███████████ ███▌ ▀▀███▀▀▀▀▀   ▀███████████     ███     ▀▀███▀▀▀▀▀   ▀███████████ ▀█████████▀
         ███ ███  ▀███████████          ███     ███     ▀███████████   ███    ███   ███ {0}
   ▄█    ███ ███    ███    ███    ▄█    ███     ███       ███    ███   ███    ███   ███ {1}
 ▄████████▀  █▀     ███    ███  ▄████████▀     ▄████▀     ███    ███   ███    █▀   ▄████▀ {2}
                    ███    ███                            ███    ███ by SirHurt CSR Team", CurrentFullVersion, AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName, Environment.OSVersion);
                SirstrapConfigurationService.LoadSettings();

                await _ipcService.StartAsync("SirstrapIpc");

                var args = Program.Args ?? [];

                RegistryManager.RegisterProtocolHandler("roblox-player", args);

#if !DEBUG
                await _robloxDownloader.ExecuteAsync(args, SirstrapType.UI);
#endif

                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(RunAsync));
                Environment.ExitCode = 1;
            }
            finally
            {
#if !DEBUG
                await _ipcService.StopAsync();
                await Log.CloseAndFlushAsync();

                Environment.Exit(Environment.ExitCode);
#endif
            }
        }
    }
}
