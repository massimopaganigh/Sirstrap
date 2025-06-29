using Serilog;
using Sirstrap.Core;

namespace Sirstrap.CLI
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Logs");

                if (!Directory.Exists(logsDirectory))
                    Directory.CreateDirectory(logsDirectory);

                string logFilePath = Path.Combine(logsDirectory, "SirstrapLog.txt");

                Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File(logFilePath, fileSizeLimitBytes: 5 * 1024 * 1024, rollOnFileSizeLimit: true, retainedFileCountLimit: 10).CreateLogger();

                Console.WriteLine(@"
   ▄████████  ▄█     ▄████████    ▄████████     ███        ▄████████    ▄████████    ▄███████▄
  ███    ███ ███    ███    ███   ███    ███ ▀█████████▄   ███    ███   ███    ███   ███    ███
  ███    █▀  ███▌   ███    ███   ███    █▀     ▀███▀▀██   ███    ███   ███    ███   ███    ███
  ███        ███▌  ▄███▄▄▄▄██▀   ███            ███   ▀  ▄███▄▄▄▄██▀   ███    ███   ███    ███
▀███████████ ███▌ ▀▀███▀▀▀▀▀   ▀███████████     ███     ▀▀███▀▀▀▀▀   ▀███████████ ▀█████████▀
         ███ ███  ▀███████████          ███     ███     ▀███████████   ███    ███   ███
   ▄█    ███ ███    ███    ███    ▄█    ███     ███       ███    ███   ███    ███   ███
 ▄████████▀  █▀     ███    ███  ▄████████▀     ▄████▀     ███    ███   ███    █▀   ▄████▀
                    ███    ███                            ███    ███ by SirHurt CSR Team
");
                SirstrapConfigurationService.LoadConfiguration();
                RegistryManager.RegisterProtocolHandler("roblox-player", args);

                await new RobloxDownloader().ExecuteAsync(args, SirstrapType.CLI);
            }
            finally
            {
                await Log.CloseAndFlushAsync();

                Environment.Exit(0);
            }
        }
    }
}