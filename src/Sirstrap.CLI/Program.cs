using Serilog;
using Sirstrap.Core;

namespace Sirstrap.CLI
{
    public static class Program
    {
        private static async Task Main(string[] arguments)
        {
            try
            {
                string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Logs");

                Directory.CreateDirectory(logsDirectory);

                string logsPath = Path.Combine(logsDirectory, "SirstrapLog.txt");

                Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File(logsPath, fileSizeLimitBytes: 5 * 1024 * 1024 /*5 MB*/, rollOnFileSizeLimit: true, retainedFileCountLimit: 10).CreateLogger();

                Console.WriteLine(@"
   ▄████████  ▄█     ▄████████    ▄████████     ███        ▄████████    ▄████████    ▄███████▄ 
  ███    ███ ███    ███    ███   ███    ███ ▀█████████▄   ███    ███   ███    ███   ███    ███ 
  ███    █▀  ███▌   ███    ███   ███    █▀     ▀███▀▀██   ███    ███   ███    ███   ███    ███ 
  ███        ███▌  ▄███▄▄▄▄██▀   ███            ███   ▀  ▄███▄▄▄▄██▀   ███    ███   ███    ███ 
▀███████████ ███▌ ▀▀███▀▀▀▀▀   ▀███████████     ███     ▀▀███▀▀▀▀▀   ▀███████████ ▀█████████▀  
         ███ ███  ▀███████████          ███     ███     ▀███████████   ███    ███   ███        
   ▄█    ███ ███    ███    ███    ▄█    ███     ███       ███    ███   ███    ███   ███        
 ▄████████▀  █▀     ███    ███  ▄████████▀     ▄████▀     ███    ███   ███    █▀   ▄████▀      
                    ███    ███                            ███    ███                           
");

                RegistryManager.RegisterProtocolHandler("roblox-player", arguments);

                await new RobloxDownloader().ExecuteAsync(arguments, SirstrapType.CLI);
            }
            finally
            {
                await Log.CloseAndFlushAsync();

                Environment.Exit(0);
            }
        }
    }
}