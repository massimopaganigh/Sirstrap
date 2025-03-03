using Serilog;
using Sirstrap.Core;

namespace Sirstrap
{
    /// <summary>
    /// Main entry point for the Sirstrap application.
    /// Initializes logging and executes the Roblox download process.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Configures Serilog for console and file logging, initiates the Roblox download process,
        /// and ensures logs are properly flushed before application exit.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task Main(string[] args)
        {
            try
            {
                string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Logs", "Sirstrap");

                Directory.CreateDirectory(logDirectory);

                string logFilePath = Path.Combine(logDirectory, "SirstrapLog.txt");

                Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File(logFilePath).CreateLogger();
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

                await new RobloxDownloader().ExecuteAsync(args).ConfigureAwait(false);
            }
            finally
            {
                await Log.CloseAndFlushAsync().ConfigureAwait(false);

                Environment.Exit(0);
            }
        }
    }
}