namespace Sirstrap.CLI
{
    internal class Program
    {
        private static readonly string _appGuid = Guid.NewGuid().ToString("N");
        private static readonly string? _appVersion =
#if DEBUG
            AppGuid;
#else
            Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0];
#endif
        private static readonly IpcService _ipcService = new();

        public static async Task Main(string[] args)
        {
            try
            {
                var logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Logs");

                if (!Directory.Exists(logsDirectory))
                    Directory.CreateDirectory(logsDirectory);

                Log.Logger = new LoggerConfiguration()
                    .Enrich.WithThreadId()
                    .Enrich.WithThreadName()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(Path.Combine(logsDirectory, $"SirstrapLog{AppGuid}.txt"), outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] {Message:lj}{NewLine}{Exception}", fileSizeLimitBytes: 1_048_576, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
                    .WriteTo.File(Path.Combine(logsDirectory, $"SirstrapErrorsLog{AppGuid}.txt"), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] {Message:lj}{NewLine}{Exception}", fileSizeLimitBytes: 1_048_576, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
                    .WriteTo.LastLog().CreateLogger();

                Console.WriteLine($@"
   ▄████████  ▄█     ▄████████    ▄████████     ███        ▄████████    ▄████████    ▄███████▄
  ███    ███ ███    ███    ███   ███    ███ ▀█████████▄   ███    ███   ███    ███   ███    ███
  ███    █▀  ███▌   ███    ███   ███    █▀     ▀███▀▀██   ███    ███   ███    ███   ███    ███
  ███        ███▌  ▄███▄▄▄▄██▀   ███            ███   ▀  ▄███▄▄▄▄██▀   ███    ███   ███    ███
▀███████████ ███▌ ▀▀███▀▀▀▀▀   ▀███████████     ███     ▀▀███▀▀▀▀▀   ▀███████████ ▀█████████▀
         ███ ███  ▀███████████          ███     ███     ▀███████████   ███    ███   ███ {AppVersion}
   ▄█    ███ ███    ███    ███    ▄█    ███     ███       ███    ███   ███    ███   ███ {AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName}
 ▄████████▀  █▀     ███    ███  ▄████████▀     ▄████▀     ███    ███   ███    █▀   ▄████▀ {Environment.OSVersion}
                    ███    ███                            ███    ███ by SirHurt CSR Team
");
                SirstrapConfigurationService.LoadSettings();

                await _ipcService.StartAsync("SirstrapIpc");

                RegistryManager.RegisterProtocolHandler("roblox-player", args);

                await new RobloxDownloader().ExecuteAsync(args, SirstrapType.CLI);

                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(Main));
                Environment.ExitCode = 1;
            }
            finally
            {
                await _ipcService.StopAsync();
                await Log.CloseAndFlushAsync();

                Environment.Exit(Environment.ExitCode);
            }
        }

        public static string AppGuid => _appGuid;

        public static string? AppVersion => _appVersion;
    }
}
