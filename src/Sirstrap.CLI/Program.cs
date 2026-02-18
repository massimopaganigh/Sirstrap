namespace Sirstrap.CLI
{
    internal sealed class Program
    {
        private static readonly IpcService _ipcService = new();

        private Program()
        {
        }

        public static async Task Main(string[] args)
        {
            try
            {
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
                        x.EnableLogs = true;
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
                    ███    ███                            ███    ███ by SirHurt CSR Team", SirstrapUpdateService.GetCurrentFullVersion(), AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName, Environment.OSVersion);
                SirstrapConfigurationService.LoadSettings();

                await _ipcService.StartAsync("SirstrapIpc");

                RegistryManager.RegisterProtocolHandler("roblox-player", args);

#if !DEBUG
                await new RobloxDownloader().ExecuteAsync(args, SirstrapType.CLI);
#endif

                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(Main));
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
