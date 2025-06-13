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
        [ObservableProperty]
        private string _currentFullVersion = $"Sirstrap {SirstrapUpdateService.GetCurrentFullVersion()}";

        [ObservableProperty]
        private bool _isRobloxRunning;

        [ObservableProperty]
        private LogEventLevel? _lastLogLevel;

        [ObservableProperty]
        private string _lastLogMessage = string.Empty;

        [ObservableProperty]
        private DateTimeOffset? _lastLogTimestamp;

        private readonly Timer _logPollingTimer;

        [ObservableProperty]
        private int _robloxProcessCount;

        public MainWindowViewModel()
        {
            _logPollingTimer = new(100);
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

                string[] fixedArguments = arguments.Skip(1).ToArray();

                RegistryManager.RegisterProtocolHandler("roblox-player", arguments);

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
            }

            UpdateRobloxProcessCount();
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