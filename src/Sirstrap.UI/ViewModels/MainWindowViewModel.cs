using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;
using Serilog.Events;
using Sirstrap.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Sirstrap.UI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private const int LOG_ACTIVITY_THRESHOLD_SECONDS = 30;
        private const int MAX_POLLING_INTERVAL = 10000;
        private const int MIN_POLLING_INTERVAL = 100;

        [ObservableProperty]
        private string _currentFullVersion = $"Sirstrap {SirstrapUpdateService.GetCurrentFullVersion()}";

        private int _currentPollingInterval = MIN_POLLING_INTERVAL;

        [ObservableProperty]
        private bool _isRobloxRunning;

        [ObservableProperty]
        private LogEventLevel? _lastLogLevel;

        [ObservableProperty]
        private string _lastLogMessage = string.Empty;

        private DateTimeOffset? _lastLogReceived;

        [ObservableProperty]
        private DateTimeOffset? _lastLogTimestamp;

        private readonly Timer _logPollingTimer;

        [ObservableProperty]
        private int _robloxProcessCount;

        public MainWindowViewModel()
        {
            _logPollingTimer = new(_currentPollingInterval);
            _logPollingTimer.Elapsed += (s, e) => UpdateLastLogFromSink();
            _logPollingTimer.Start();

            Task.Run(() => Main(Environment.GetCommandLineArgs()));
        }

        private static async Task Main(string[] arguments)
        {
            try
            {
                string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Logs");

                Directory.CreateDirectory(logsDirectory);

                string logsPath = Path.Combine(logsDirectory, "SirstrapLog.txt");

                Log.Logger = new LoggerConfiguration().WriteTo.File(logsPath).WriteTo.LastLog().CreateLogger();

                RegistryManager.RegisterProtocolHandler("roblox-player");

                string[] fixedArguments = [.. arguments.Skip(1)];

                await new RobloxDownloader().ExecuteAsync(fixedArguments, SirstrapType.UI);
            }
            finally
            {
                await Log.CloseAndFlushAsync();

                Environment.Exit(0);
            }
        }

        private void UpdateLastLogFromSink()
        {
            if (!LastLogMessage.Equals(LastLogSink.LastLog))
            {
                LastLogMessage = LastLogSink.LastLog;
                LastLogTimestamp = LastLogSink.LastLogTimestamp;
                LastLogLevel = LastLogSink.LastLogLevel;

                _lastLogReceived = DateTimeOffset.Now;
            }

            UpdateRobloxProcessCount();
            UpdatePollingInterval();
        }

        private void UpdatePollingInterval()
        {
            bool hasRecentLogActivity = _lastLogReceived.HasValue && (DateTimeOffset.Now - _lastLogReceived.Value).TotalSeconds <= LOG_ACTIVITY_THRESHOLD_SECONDS;

            int newInterval = hasRecentLogActivity ? MIN_POLLING_INTERVAL : MAX_POLLING_INTERVAL;

            if (newInterval != _currentPollingInterval)
            {
                _currentPollingInterval = newInterval;
                _logPollingTimer.Interval = _currentPollingInterval;
            }
        }

        private void UpdateRobloxProcessCount()
        {
            try
            {
                string[] commonRobloxNames = ["RobloxPlayerBeta"];
                int count = Process.GetProcesses().Count(x => commonRobloxNames.Any(y => string.Equals(x.ProcessName, y, StringComparison.OrdinalIgnoreCase)));

                RobloxProcessCount = count;
                IsRobloxRunning = count > 0;
            }
            catch (Exception) { } //ignore
        }

        public void Dispose()
        {
            _logPollingTimer?.Stop();
            _logPollingTimer?.Dispose();
        }
    }
}