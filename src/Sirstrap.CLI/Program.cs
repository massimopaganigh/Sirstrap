using Serilog;
using Sirstrap.Core;

namespace Sirstrap
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

                Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File(logsPath).CreateLogger();

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

                RegistryManager.RegisterProtocolHandler("roblox-player");

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