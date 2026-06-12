namespace SirHurt.Cleaner.CLI.Infrastructure
{
    public sealed class SirstrapRegistryCleaner(ILogger logger) : IRegistryCleaner
    {
        public void CleanAllUsers(IEnumerable<string> keyPaths) => RegistryManager.CleanAllUsersRegistry(keyPaths, logger);

        public void CleanCurrentUser(IEnumerable<string> keyPaths) => RegistryManager.CleanCurrentUserRegistry(keyPaths, logger);

        public void CleanLocalMachine(IEnumerable<string> keyPaths) => RegistryManager.CleanLocalMachineRegistry(keyPaths, logger);
    }
}
