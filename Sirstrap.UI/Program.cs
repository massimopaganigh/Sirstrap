using Avalonia;
using System;

namespace Sirstrap.UI
{
    /// <summary>
    /// Entry point class for the Avalonia application.
    /// </summary>
    /// <remarks>
    /// This sealed class contains the application's entry point and bootstrapping logic
    /// for initializing the Avalonia UI framework.
    /// </remarks>
    sealed class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command line arguments passed to the application.</param>
        /// <remarks>
        /// Initializes and starts the Avalonia application with the classic desktop lifetime.
        /// The STA (Single-Threaded Apartment) threading model is required for proper UI operation.
        /// </remarks>
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        /// <summary>
        /// Configures and builds the Avalonia application.
        /// </summary>
        /// <returns>An <see cref="AppBuilder"/> instance configured for the application.</returns>
        /// <remarks>
        /// This method:
        /// <list type="bullet">
        ///   <item><description>Configures the application using the <see cref="App"/> class</description></item>
        ///   <item><description>Enables automatic platform detection</description></item>
        ///   <item><description>Sets up Inter font as the default font</description></item>
        ///   <item><description>Configures logging to the trace output</description></item>
        /// </list>
        /// </remarks>
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();
        }
    }
}