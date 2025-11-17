using Serilog;
using Sirstrap.Core;

namespace Sirstrap.CLI
{
    internal class Program
    {
        private static readonly IpcService _ipcService = new();

        public static async Task Main(string[] args)
        {
            try
            {
                string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Logs");

                if (!Directory.Exists(logsDirectory))
                    Directory.CreateDirectory(logsDirectory);

                string logFilePath = Path.Combine(logsDirectory, "SirstrapLog.txt");

                Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File(logFilePath, fileSizeLimitBytes: 5 * 1024 * 1024, rollOnFileSizeLimit: true, retainedFileCountLimit: 10).WriteTo.LastLog().CreateLogger();

                string? targetFrameworkName = AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName;
                DateTime creationTime = File.GetCreationTime(AppContext.BaseDirectory);
                OperatingSystem oSVersion = Environment.OSVersion;

                Console.WriteLine($@"
   ▄████████  ▄█     ▄████████    ▄████████     ███        ▄████████    ▄████████    ▄███████▄
  ███    ███ ███    ███    ███   ███    ███ ▀█████████▄   ███    ███   ███    ███   ███    ███
  ███    █▀  ███▌   ███    ███   ███    █▀     ▀███▀▀██   ███    ███   ███    ███   ███    ███
  ███        ███▌  ▄███▄▄▄▄██▀   ███            ███   ▀  ▄███▄▄▄▄██▀   ███    ███   ███    ███
▀███████████ ███▌ ▀▀███▀▀▀▀▀   ▀███████████     ███     ▀▀███▀▀▀▀▀   ▀███████████ ▀█████████▀
         ███ ███  ▀███████████          ███     ███     ▀███████████   ███    ███   ███ {targetFrameworkName}
   ▄█    ███ ███    ███    ███    ▄█    ███     ███       ███    ███   ███    ███   ███ {creationTime}
 ▄████████▀  █▀     ███    ███  ▄████████▀     ▄████▀     ███    ███   ███    █▀   ▄████▀ {oSVersion}
                    ███    ███                            ███    ███ by SirHurt CSR Team
");

                await _ipcService.StartAsync("SirstrapIpc");

                SirstrapConfigurationService.LoadSettings();
                RegistryManager.RegisterProtocolHandler("roblox-player", args);

                await new RobloxDownloader().ExecuteAsync(args, SirstrapType.CLI);
            }
            finally
            {
                await _ipcService.StopAsync();

                await Log.CloseAndFlushAsync();

                Environment.Exit(0);
            }
        }
    }
}
