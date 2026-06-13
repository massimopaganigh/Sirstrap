namespace Sirstrap.Core.Windows
{
    public interface IRegistryManager
    {
        void CleanAllUsersRegistry(IEnumerable<string> keyPaths, ILogger? logger = null);

        void CleanCurrentUserRegistry(IEnumerable<string> keyPaths, ILogger? logger = null);

        void CleanLocalMachineRegistry(IEnumerable<string> keyPaths, ILogger? logger = null);

        void CleanRegistryKeys(RegistryKey registryHive, IEnumerable<string> keyPaths, ILogger? logger = null);

        void DeleteRegistryKey(RegistryKey registryHive, string keyPath, ILogger? logger = null);
    }
}
