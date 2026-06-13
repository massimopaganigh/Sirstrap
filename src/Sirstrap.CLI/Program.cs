namespace Sirstrap.CLI
{
    internal sealed class Program
    {
        private Program()
        {
        }

        public static async Task Main(string[] args)
        {
            using ServiceProvider serviceProvider = new ServiceCollection()
                .AddSirstrapCore()
                .BuildServiceProvider();

            var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
            var pathManager = serviceProvider.GetRequiredService<IPathManager>();
            var sirHurtService = serviceProvider.GetRequiredService<ISirHurtService>();
            var sirstrapVersion = serviceProvider.GetRequiredService<ISirstrapVersion>();
            var lastLogSink = serviceProvider.GetRequiredService<ILastLogSink>();
            var ipcService = serviceProvider.GetRequiredService<IIpcService>();

            try
            {
                var logsDirectory = pathManager.GetLogsPath();

                if (!Directory.Exists(logsDirectory))
                    Directory.CreateDirectory(logsDirectory);

                pathManager.PurgeOldLogs();

                var appGuid = Guid.NewGuid().ToString("N");

                settingsService.LoadSettings();

                var loggerConfig = new LoggerConfiguration()
                    .Enrich.WithThreadId()
                    .Enrich.WithThreadName()
                    .Enrich.WithProperty("SirHurtUser", sirHurtService.GetSirHurtUser())
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] [User: {SirHurtUser}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(Path.Combine(logsDirectory, $"SirstrapLog{appGuid}.txt"), outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] [User: {SirHurtUser}] {Message:lj}{NewLine}{Exception}", fileSizeLimitBytes: 1_048_576, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
                    .WriteTo.File(Path.Combine(logsDirectory, $"SirstrapErrorsLog{appGuid}.txt"), restrictedToMinimumLevel: LogEventLevel.Error, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] [User: {SirHurtUser}] {Message:lj}{NewLine}{Exception}", fileSizeLimitBytes: 1_048_576, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
                    .WriteTo.LastLog(lastLogSink);

#if !DEBUG
                if (serviceProvider.GetRequiredService<SirstrapConfiguration>().Telemetry)
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
                    ███    ███                            ███    ███ by SirHurt CSR Team", sirstrapVersion.GetFullVersion(), AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName, Environment.OSVersion);
                settingsService.LoadSettings();
                settingsService.EmitSettingsMetrics();

                pathManager.PurgePreviousInstallationPath();

                await ipcService.StartAsync("SirstrapIpc");

                serviceProvider.GetRequiredService<IProtocolHandlerRegistrar>().RegisterProtocolHandler("roblox-player", args);

#if !DEBUG
                await serviceProvider.GetRequiredService<IRobloxDownloader>().ExecuteAsync(args, SirstrapType.CLI);
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
                await ipcService.StopAsync();
                await Log.CloseAndFlushAsync();

                Environment.Exit(Environment.ExitCode);
#endif
            }
        }
    }
}
