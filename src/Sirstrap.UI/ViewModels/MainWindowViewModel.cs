namespace Sirstrap.UI.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _currentFullVersion = SirstrapUpdateService.GetCurrentFullVersion();

        private int _currentPollingInterval = 100;
        private readonly IpcService _ipcService = new();
        private bool _isMinimized;

        [ObservableProperty]
        private string _lastLogMessage = "...";

        private DateTimeOffset? _lastLogReceived;
        private Timer _logPollingTimer;
        private readonly RobloxActivityWatcher _robloxActivityWatcher = new();
        private readonly RobloxDownloader _robloxDownloader = new();

        [ObservableProperty]
        private string _serverLocation = string.Empty;

        [ObservableProperty]
        private bool _showServerLocation = false;

        private bool _wasRobloxRunning;

        public MainWindowViewModel()
        {
            _logPollingTimer = new(_currentPollingInterval);

            _logPollingTimer.Elapsed += (s, e) => GetLastLogMessageFromLastLogSink();

            _logPollingTimer.Start();

            _robloxActivityWatcher.ServerLocationChanged += OnServerLocationChanged;

            Task.Run(RunAsync);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        private void FindRoblox()
        {
            try
            {
                var processNames = new[] { "Roblox", "RobloxPlayerBeta" };
                var isRobloxRunning = false;

                foreach (var processName in processNames)
                    if (Process.GetProcessesByName(processName).Length > 0)
                    {
                        isRobloxRunning = true;

                        break;
                    }

                if (isRobloxRunning
                    && !_wasRobloxRunning)
                {
                    _robloxActivityWatcher.StartWatching();

                    _wasRobloxRunning = true;
                }
                else if (!isRobloxRunning
                    && _wasRobloxRunning)
                {
                    _robloxActivityWatcher.StopWatching();

                    ShowServerLocation = false;
                    ServerLocation = string.Empty;
                    _wasRobloxRunning = false;
                }

                var mainWindow = GetMainWindow();

                if (mainWindow != null
                    && isRobloxRunning
                    && !_isMinimized)
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        mainWindow.WindowState = WindowState.Minimized;
                    });

                    _isMinimized = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(FindRoblox));
            }
        }

        private void GetLastLogMessageFromLastLogSink()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(LastLogSink.LastLog)
                    && !string.Equals(LastLogMessage, LastLogSink.LastLog))
                {
                    LastLogMessage = LastLogSink.LastLog;
                    _lastLogReceived = LastLogSink.LastLogTimestamp;
                }

                FindRoblox();
                GetPollingInterval();
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetLastLogMessageFromLastLogSink));
            }
        }

        private void GetPollingInterval()
        {
            try
            {
                var targetPollingInterval = _lastLogReceived.HasValue && (DateTimeOffset.Now - _lastLogReceived.Value).TotalSeconds <= 30 ? 100 : 10000;

                if (targetPollingInterval != _currentPollingInterval)
                {
                    _currentPollingInterval = targetPollingInterval;
                    _logPollingTimer.Interval = _currentPollingInterval;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetPollingInterval));
            }
        }

        private void OnServerLocationChanged(object? sender, string location) => Dispatcher.UIThread.InvokeAsync(() =>
        {
            ServerLocation = location;
            ShowServerLocation = !string.IsNullOrEmpty(ServerLocation);
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

                var logsDirectory = PathManager.GetLogsPath();

                if (!Directory.Exists(logsDirectory))
                    Directory.CreateDirectory(logsDirectory);

                PathManager.PurgeOldLogs();

                var appGuid = Guid.NewGuid().ToString("N");

                Log.Logger = new LoggerConfiguration()
                    .Enrich.WithThreadId()
                    .Enrich.WithThreadName()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(Path.Combine(logsDirectory, $"SirstrapLog{appGuid}.txt"), outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] {Message:lj}{NewLine}{Exception}", fileSizeLimitBytes: 1_048_576, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
                    .WriteTo.File(Path.Combine(logsDirectory, $"SirstrapErrorsLog{appGuid}.txt"), restrictedToMinimumLevel: LogEventLevel.Error, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] {Message:lj}{NewLine}{Exception}", fileSizeLimitBytes: 1_048_576, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
                    .WriteTo.LastLog()
#if !DEBUG
                    .WriteTo.Sentry(x =>
                    {
                        x.Dsn = "https://0cd56ab3e5eac300ecf1380dd6ad0a92@o4510907426471936.ingest.de.sentry.io/4510907479490640";
                        //x.Debug = true;
                        x.AutoSessionTracking = true;
                    })
#endif
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

                RegistryManager.RegisterProtocolHandler("roblox-player", Program.Args ?? []);

#if !DEBUG
                await _robloxDownloader.ExecuteAsync(Program.Args ?? [], SirstrapType.UI);
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
