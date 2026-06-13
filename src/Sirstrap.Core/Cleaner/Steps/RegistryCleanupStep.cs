namespace Sirstrap.Core.Cleaner.Steps
{
    public sealed class RegistryCleanupStep(IRegistryManager registryManager, CleanerConfig config) : ICleanupStep
    {
        public string Name => "Clean registry";

        public void Execute()
        {
            Log.Information("[*] Removing the registry keys for the current user...");

            registryManager.CleanCurrentUserRegistry(config.RegistryKeys);

            Log.Information("[*] Removing the registry keys for all user hives...");

            registryManager.CleanAllUsersRegistry(config.RegistryKeys);

            Log.Information("[*] Removing the machine-wide registry keys...");

            registryManager.CleanLocalMachineRegistry(config.LocalMachineRegistryKeys);
        }
    }
}
