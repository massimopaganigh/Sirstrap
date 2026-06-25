namespace Sirstrap.Core.Windows
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
    public sealed class RegistryManager : IRegistryManager
    {
        public void CleanAllUsersRegistry(IEnumerable<string> keyPaths, ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(keyPaths);

            var log = logger ?? Log.Logger;
            var keyPathList = keyPaths.ToArray();

            try
            {
                var subKeyNames = RegistryOperations.GetSubKeyNames(Registry.Users);

                log.Information("[*] Found {Count} user registry hives.", subKeyNames.Length);

                foreach (var sid in subKeyNames)
                {
                    if (ShouldSkipUserRegistryHive(sid))
                        continue;

                    try
                    {
                        log.Information("[*] Checking the registry for the user SID {Sid}...", sid);

                        foreach (var keyPath in keyPathList)
                            DeleteRegistryKey(Registry.Users, $@"{sid}\{keyPath}", log);
                    }
                    catch (Exception ex)
                    {
                        log.Warning(ex, "[!] Failed to access the registry for the user SID {Sid}.", sid);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "[!] Failed to process the user registry hives.");
            }
        }

        public void CleanCurrentUserRegistry(IEnumerable<string> keyPaths, ILogger? logger = null)
        {
            CleanRegistryKeys(Registry.CurrentUser, keyPaths, logger);
        }

        public void CleanLocalMachineRegistry(IEnumerable<string> keyPaths, ILogger? logger = null)
        {
            CleanRegistryKeys(Registry.LocalMachine, keyPaths, logger);
        }

        public void CleanRegistryKeys(RegistryKey registryHive, IEnumerable<string> keyPaths, ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(registryHive);
            ArgumentNullException.ThrowIfNull(keyPaths);

            foreach (var keyPath in keyPaths)
                DeleteRegistryKey(registryHive, keyPath, logger);
        }

        public void DeleteRegistryKey(RegistryKey registryHive, string keyPath, ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(registryHive);
            ArgumentException.ThrowIfNullOrWhiteSpace(keyPath);

            var log = logger ?? Log.Logger;
            var hiveName = RegistryOperations.GetHiveName(registryHive);

            try
            {
                log.Information("[*] Checking the registry key {RegistryHive}\\{KeyPath}...", hiveName, keyPath);

                if (!RegistryOperations.RegistryKeyExists(registryHive, keyPath))
                {
                    log.Debug("[*] The registry key {RegistryHive}\\{KeyPath} does not exist.", hiveName, keyPath);
                    return;
                }

                RegistryOperations.DeleteRegistrySubKeyTree(registryHive, keyPath);
                log.Information("[*] Deleted the registry key {RegistryHive}\\{KeyPath}.", hiveName, keyPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                log.Warning(ex, "[!] The access to the registry key {RegistryHive}\\{KeyPath} was denied, administrative privileges may be required.", hiveName, keyPath);
            }
            catch (Exception ex)
            {
                log.Error(ex, "[!] Failed to delete the registry key {RegistryHive}\\{KeyPath}.", hiveName, keyPath);
            }
        }

        private static bool ShouldSkipUserRegistryHive(string sid) =>
            string.Equals(sid, ".DEFAULT", StringComparison.OrdinalIgnoreCase) ||
            sid.EndsWith("_Classes", StringComparison.OrdinalIgnoreCase);
    }
}
