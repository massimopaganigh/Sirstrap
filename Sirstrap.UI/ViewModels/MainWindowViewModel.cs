using Serilog;
using Sirstrap.Core;
using System;
using System.Threading.Tasks;

namespace Sirstrap.UI.ViewModels
{
    /// <summary>
    /// Main view model that initializes the application and handles the Roblox protocol integration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class serves as the entry point for the Sirstrap application. It is responsible for:
    /// - Configuring and initializing the Serilog logger
    /// - Registering the Roblox protocol handler
    /// - Initiating the Roblox download process
    /// </para>
    /// <para>
    /// The view model starts these operations asynchronously in a background thread upon instantiation.
    /// </para>
    /// </remarks>
    public partial class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor launches the main application logic asynchronously in a background thread
        /// to avoid blocking the UI thread during initialization.
        /// </remarks>
        public MainWindowViewModel()
        {
            Task.Run(() => Main(Environment.GetCommandLineArgs()));
        }

        /// <summary>
        /// Executes the main application logic.
        /// </summary>
        /// <param name="args">
        /// Command line arguments passed to the application, obtained from <see cref="Environment.GetCommandLineArgs"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method performs the following operations in sequence:
        /// 1. Configures Serilog to write logs to a file named "SirstrapLog.txt"
        /// 2. Registers "roblox-player" as a protocol handler using <see cref="RegistryManager"/>
        /// 3. Executes the Roblox download process with the provided command line arguments
        /// 4. Ensures all log messages are properly flushed before the application exits
        /// </para>
        /// <para>
        /// The method is designed to be called only once during the application lifecycle.
        /// </para>
        /// </remarks>
        private static async Task Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration().WriteTo.File("SirstrapLog.txt").CreateLogger();
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