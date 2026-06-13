using Serilog.Sinks.SystemConsole.Themes;
using System.Text;

namespace SirHurt.Cleaner.CLI
{
    internal static class Program
    {
        private static async Task Main()
        {
            var statusLine = new ConsoleStatusLine();

            Log.Logger = CreateLogger(statusLine);

            TryEnableUtf8Output();

            try
            {
                Log.Information(@"
   ▄████████  ▄█     ▄████████    ▄█    █▄    ███    █▄     ▄████████     ███      ▄████████  ▄█          ▄████████    ▄████████ ███▄▄▄▄      ▄████████    ▄████████
  ███    ███ ███    ███    ███   ███    ███   ███    ███   ███    ███ ▀█████████▄ ███    ███ ███         ███    ███   ███    ███ ███▀▀▀██▄   ███    ███   ███    ███
  ███    █▀  ███▌   ███    ███   ███    ███   ███    ███   ███    ███    ▀███▀▀██ ███    █▀  ███         ███    █▀    ███    ███ ███   ███   ███    █▀    ███    ███
  ███        ███▌  ▄███▄▄▄▄██▀  ▄███▄▄▄▄███▄▄ ███    ███  ▄███▄▄▄▄██▀     ███   ▀ ███        ███        ▄███▄▄▄       ███    ███ ███   ███  ▄███▄▄▄      ▄███▄▄▄▄██▀
▀███████████ ███▌ ▀▀███▀▀▀▀▀   ▀▀███▀▀▀▀███▀  ███    ███ ▀▀███▀▀▀▀▀       ███     ███        ███       ▀▀███▀▀▀     ▀███████████ ███   ███ ▀▀███▀▀▀     ▀▀███▀▀▀▀▀
         ███ ███  ▀███████████   ███    ███   ███    ███ ▀███████████     ███     ███    █▄  ███         ███    █▄    ███    ███ ███   ███   ███    █▄  ▀███████████
   ▄█    ███ ███    ███    ███   ███    ███   ███    ███   ███    ███     ███     ███    ███ ███▌    ▄   ███    ███   ███    ███ ███   ███   ███    ███   ███    ███
 ▄████████▀  █▀     ███    ███   ███    █▀    ████████▀    ███    ███    ▄████▀   ████████▀  █████▄▄██   ██████████   ███    █▀   ▀█   █▀    ██████████   ███    ███
                    ███    ███                             ███    ███                        ▀                                                            ███    ███
");

                Log.Information("[*] SirHurt Cleaner starting up...");

                using ServiceProvider serviceProvider = new ServiceCollection()
                    .AddSingleton<IStatusLine>(statusLine)
                    .AddSirstrapCleaner()
                    .BuildServiceProvider();

                serviceProvider.GetRequiredService<CleanerConfig>().CleanTempFolders =
                    serviceProvider.GetRequiredService<IUserInteraction>().Confirm("Would you like to clean temporary folders?", defaultAnswer: true);

                serviceProvider.GetRequiredService<ICleanupOrchestrator>().Run();

                if (!Console.IsInputRedirected)
                {
                    Log.Information("[*] Press any key to exit...");
                    Console.ReadKey(intercept: true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to run the SirHurt Cleaner, the cleanup was aborted.");

                Environment.ExitCode = 1;
            }
            finally
            {
                statusLine.Clear();

                await Log.CloseAndFlushAsync();
            }
        }

        private static ILogger CreateLogger(IStatusLine statusLine)
        {
            var consoleLogger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Sink(new StatusLinePreservingSink(consoleLogger, statusLine))
                .CreateLogger();
        }

        private static void TryEnableUtf8Output()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "[*] Could not switch the console output to UTF-8, the emoji rendering may degrade.");
            }
        }
    }
}
