namespace SirHurt.Cleaner.CLI.Services
{
    /// <summary>
    /// Removes Roblox and SirHurt registry keys from the user and machine hives.
    /// </summary>
    public sealed class RegistryCleanupStep(ILogger logger, IRegistryCleaner registryCleaner, CleanerConfig config) : ICleanupStep
    {
        public string Name => "Clean registry";

        public void Execute()
        {
            logger.Information("[*] Removing registry keys for the current user");
            registryCleaner.CleanCurrentUser(config.RegistryKeys);
            logger.Information("[*] Removing registry keys for all user hives");
            registryCleaner.CleanAllUsers(config.RegistryKeys);
            logger.Information("[*] Removing machine-wide registry keys");
            registryCleaner.CleanLocalMachine(config.LocalMachineRegistryKeys);
        }
    }
}
