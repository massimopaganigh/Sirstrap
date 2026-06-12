using Serilog.Sinks.SystemConsole.Themes;
using System.Text;

namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Entry point and composition root for the SirHurt cleanup utility.
    /// Removes Roblox and SirHurt-related folders and registry keys.
    /// </summary>
    internal static class Program
    {
        private static async Task Main()
        {
            TryEnableUtf8Output();

            var statusLine = new ConsoleStatusLine();

            Log.Logger = CreateLogger(statusLine);

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

                Log.Information("[*] SirHurt Cleaner starting up");

                IUserInteraction userInteraction = new ConsoleUserInteraction(statusLine);

                var config = new CleanerConfig
                {
                    CleanTempFolders = userInteraction.Confirm("Would you like to clean temporary folders?", defaultAnswer: true)
                };

                BuildOrchestrator(config, userInteraction, statusLine).Run();

                if (!Console.IsInputRedirected)
                {
                    Log.Information("[*] Press any key to exit...");
                    Console.ReadKey(intercept: true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Unhandled error — cleanup aborted");
                Environment.ExitCode = 1;
            }
            finally
            {
                statusLine.Clear();
                await Log.CloseAndFlushAsync();
            }
        }

        private static void TryEnableUtf8Output()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch (Exception)
            {
                // Emoji rendering degrades gracefully when the encoding cannot be changed.
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

        private static CleanupOrchestrator BuildOrchestrator(CleanerConfig config, IUserInteraction userInteraction, IStatusLine statusLine)
        {
            var logger = Log.Logger;
            IFileSystem fileSystem = new StandardFileSystem();
            IProcessManager processManager = new StandardProcessManager(logger);
            IUserProfileProvider userProfileProvider = new WindowsUserProfileProvider(fileSystem);
            IRegistryCleaner registryCleaner = new SirstrapRegistryCleaner(logger);
            IFolderDeleter folderDeleter = new FolderDeleter(logger, fileSystem);
            ISelectiveFolderCleaner selectiveFolderCleaner = new SelectiveFolderCleaner(logger, fileSystem, userInteraction, folderDeleter, config);

            var steps = new List<ICleanupStep>
            {
                new ProcessCloser(logger, processManager, userInteraction, config),
                new SystemFoldersCleanupStep(logger, folderDeleter, config),
                new UserFoldersCleanupStep(logger, selectiveFolderCleaner, userProfileProvider, config),
                new RegistryCleanupStep(logger, registryCleaner, config)
            };

            if (config.CleanTempFolders)
                steps.Add(new TempFolderCleaner(logger, fileSystem, folderDeleter, userProfileProvider, config));
            else
                logger.Information("[*] Temporary folder cleanup skipped (disabled by user)");

            return new CleanupOrchestrator(logger, steps, statusLine);
        }
    }
}
