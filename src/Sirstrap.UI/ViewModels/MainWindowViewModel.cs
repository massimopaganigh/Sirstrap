namespace Sirstrap.UI.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly SirstrapConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IIpcService _ipcService;
        private readonly ILastLogSink _lastLogSink;
        private readonly IPathManager _pathManager;
        private readonly IProtocolHandlerRegistrar _protocolHandlerRegistrar;
        private readonly RobloxActivityWatcher _robloxActivityWatcher;
        private readonly ISettingsService _settingsService;
        private readonly ISirHurtService _sirHurtService;
        private readonly IWeaoService _weaoService;

        [ObservableProperty]
        private string _accountName = string.Empty;

        [ObservableProperty]
        private string _announcement = string.Empty;

        [ObservableProperty]
        private string _currentFullVersion = string.Empty;

        private int _currentPollingInterval = 100;

        [ObservableProperty]
        private bool _isLoggedIn = false;

        private bool _isMinimized;

        [ObservableProperty]
        private string _lastLogMessage = "...";

        private DateTimeOffset? _lastLogReceived;
        private readonly Timer _logPollingTimer;

        [ObservableProperty]
        private string _serverLocation = string.Empty;

        [ObservableProperty]
        private bool _showAnnouncement = false;

        [ObservableProperty]
        private bool _showLaunchButton = Program.Args == null || Program.Args.Length == 0;

        [ObservableProperty]
        private bool _showServerLocation = false;

        [ObservableProperty]
        private ObservableCollection<VersionSourceOption> _versionSources = [];

        [ObservableProperty]
        private VersionSourceOption? _selectedVersionSourceMain;

        private bool _wasRobloxRunning;

        public MainWindowViewModel(
            SirstrapConfiguration configuration,
            HttpClient httpClient,
            IIpcService ipcService,
            ILastLogSink lastLogSink,
            IPathManager pathManager,
            IProtocolHandlerRegistrar protocolHandlerRegistrar,
            RobloxActivityWatcher robloxActivityWatcher,
            ISettingsService settingsService,
            ISirHurtService sirHurtService,
            ISirstrapVersion sirstrapVersion,
            IWeaoService weaoService)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _ipcService = ipcService;
            _lastLogSink = lastLogSink;
            _pathManager = pathManager;
            _protocolHandlerRegistrar = protocolHandlerRegistrar;
            _robloxActivityWatcher = robloxActivityWatcher;
            _settingsService = settingsService;
            _sirHurtService = sirHurtService;
            _weaoService = weaoService;

            CurrentFullVersion = sirstrapVersion.GetFullVersion();

            _ = LoadVersionSourcesAsync();

            _logPollingTimer = new(_currentPollingInterval);

            _logPollingTimer.Elapsed += (s, e) => GetLastLogMessageFromLastLogSink();

            _logPollingTimer.Start();

            _robloxActivityWatcher.ServerLocationChanged += OnServerLocationChanged;

            Task.Run(SomethingAsync);

            if (!ShowLaunchButton)
                Task.Run(RunAsync);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        private async Task LoadVersionSourcesAsync()
        {
            try
            {
                var options = await VersionSourceCatalog.BuildAsync(_weaoService);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    VersionSources = new ObservableCollection<VersionSourceOption>(options);
                    SelectedVersionSourceMain = VersionSources.FirstOrDefault(option => string.Equals(option.Value, _configuration.RobloxVersionSource, StringComparison.OrdinalIgnoreCase))
                        ?? VersionSources.FirstOrDefault();
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(LoadVersionSourcesAsync));
            }
        }

        partial void OnSelectedVersionSourceMainChanged(VersionSourceOption? value)
        {
            if (value == null
                || string.Equals(value.Value, _configuration.RobloxVersionSource, StringComparison.OrdinalIgnoreCase))
                return;

            _configuration.RobloxVersionSource = value.Value;
            _settingsService.SaveSettings();

            Log.Information("[*] Roblox version source set to {VersionSource} from the launch menu.", value.Value);
        }

        private void FindRoblox()
        {
            try
            {
                var processNames = new[] { "RobloxPlayerBeta", "RobloxPlayerBeta.exe" };
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
                    if (_configuration.SirstrapTrayMode == TrayMode.OnRoblox)
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            mainWindow.Hide();

                            App.SetTrayIconVisible(true);
                        });
                    else
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            mainWindow.WindowState = WindowState.Minimized;
                        });

                    _isMinimized = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to track the Roblox process.");
            }
        }

        private void GetLastLogMessageFromLastLogSink()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_lastLogSink.LastLog)
                    && !string.Equals(LastLogMessage, _lastLogSink.LastLog))
                {
                    LastLogMessage = _lastLogSink.LastLog;
                    _lastLogReceived = _lastLogSink.LastLogTimestamp;
                }

                FindRoblox();
                GetPollingInterval();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to read the last log message.");
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
                Log.Error(ex, "[!] Failed to adjust the log polling interval.");
            }
        }

        private async Task LoadAnnouncementAsync()
        {
            try
            {
#pragma warning disable S1075 // remote announcements endpoint, not a local path
                var announcement = await HttpClientExtension.GetStringAsync(_httpClient, "https://raw.githubusercontent.com/massimopaganigh/Sirstrap/main/announcements.txt");
#pragma warning restore S1075

                if (!string.IsNullOrWhiteSpace(announcement))
                {
                    Announcement = announcement.Trim();
                    ShowAnnouncement = true;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to load the announcement.");
            }
        }

        [RelayCommand]
        private void Logout()
        {
            try
            {
                if (_sirHurtService.Logout())
                {
                    AccountName = string.Empty;
                    IsLoggedIn = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to log out from SirHurt.");
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
                Sentry.SentrySdk.Metrics.EmitCounter(nameof(OpenGitHub), 1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to open the GitHub repository.");
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
                Sentry.SentrySdk.Metrics.EmitCounter(nameof(OpenIssue), 1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to open the GitHub issue page.");
            }
        }

        [RelayCommand]
        private void OpenSettings()
        {
            try
            {
                new SettingsWindow { DataContext = Program.Services.GetRequiredService<SettingsWindowViewModel>() }.ShowDialog(GetMainWindow()!);
                Sentry.SentrySdk.Metrics.EmitCounter(nameof(OpenSettings), 1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to open the settings window.");
            }
        }

        [RelayCommand]
        private async Task RunAsync()
        {
            try
            {
                ShowLaunchButton = false;

#if !DEBUG
                await Program.Services.GetRequiredService<IRobloxDownloader>().ExecuteAsync(Program.Args ?? [], SirstrapType.UI);
#endif

                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to run Sirstrap.");
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

        private async Task SomethingAsync()
        {
#if DEBUG
            AllocConsole();
#endif

            var logsDirectory = _pathManager.GetLogsPath();

            if (!Directory.Exists(logsDirectory))
                Directory.CreateDirectory(logsDirectory);

            _pathManager.PurgeOldLogs();

            var appGuid = Guid.NewGuid().ToString("N");

            _settingsService.LoadSettings();

            var loggerConfig = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .Enrich.WithProperty("SirHurtUser", _sirHurtService.GetSirHurtUser())
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] [User: {SirHurtUser}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(Path.Combine(logsDirectory, $"SirstrapLog{appGuid}.txt"), outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] [User: {SirHurtUser}] {Message:lj}{NewLine}{Exception}", fileSizeLimitBytes: 1_048_576, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
                .WriteTo.File(Path.Combine(logsDirectory, $"SirstrapErrorsLog{appGuid}.txt"), restrictedToMinimumLevel: LogEventLevel.Error, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] [User: {SirHurtUser}] {Message:lj}{NewLine}{Exception}", fileSizeLimitBytes: 1_048_576, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
                .WriteTo.LastLog(_lastLogSink);

#if !DEBUG
            if (_configuration.Telemetry)
                loggerConfig = loggerConfig.WriteTo.Sentry(x =>
                {
                    x.Dsn = "https://0cd56ab3e5eac300ecf1380dd6ad0a92@o4510907426471936.ingest.de.sentry.io/4510907479490640";
                    x.AutoSessionTracking = true;
                    x.EnableLogs = true;

                    x.TracesSampleRate = 0.5;
                    x.ProfilesSampleRate = 0.5;
                    x.AddIntegration(new Sentry.Profiling.ProfilingIntegration());
                });
#endif

            Log.Logger = loggerConfig.CreateLogger();

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
            _settingsService.LoadSettings();
            _settingsService.EmitSettingsMetrics();
            _pathManager.PurgePreviousInstallationPath();

            await _ipcService.StartAsync("SirstrapIpc");

            _protocolHandlerRegistrar.RegisterProtocolHandler("roblox-player", Program.Args ?? []);

            var user = _sirHurtService.GetSirHurtUser();

            AccountName = user;
            IsLoggedIn = !string.IsNullOrWhiteSpace(user);

            await LoadAnnouncementAsync();
        }
    }
}
