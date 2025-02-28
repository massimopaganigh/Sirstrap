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
            Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("SirstrapLog.txt").CreateLogger();
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
            await Log.CloseAndFlushAsync().ConfigureAwait(false);
        }
    }
}