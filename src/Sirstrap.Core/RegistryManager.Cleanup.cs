namespace Sirstrap.Core
{
    public static partial class RegistryManager
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static bool RegistryKeyExists(RegistryKey hive, string keyPath)
        {
            ArgumentNullException.ThrowIfNull(hive);

            using var key = OpenRegistrySubKey(hive, keyPath);

            return key != null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static RegistryKey? OpenRegistrySubKey(RegistryKey hive, string keyPath, bool writable = false)
        {
            ArgumentNullException.ThrowIfNull(hive);
            ArgumentException.ThrowIfNullOrWhiteSpace(keyPath);

            return hive.OpenSubKey(keyPath, writable);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static RegistryKey CreateRegistrySubKey(RegistryKey registryKey, string keyPath, string description)
        {
            ArgumentNullException.ThrowIfNull(registryKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(keyPath);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);

            var subKey = registryKey.CreateSubKey(keyPath);

            if (subKey != null)
                return subKey;

            Log.Error("[!] Failed to create or open registry key for {Description}. Access may be denied.", description);

            throw new UnauthorizedAccessException($"Could not create registry key for {description}");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static string[] GetSubKeyNames(RegistryKey key)
        {
            ArgumentNullException.ThrowIfNull(key);

            return key.GetSubKeyNames();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static string? GetRegistryStringValue(RegistryKey registryKey, string? name)
        {
            ArgumentNullException.ThrowIfNull(registryKey);

            return registryKey.GetValue(name)?.ToString();
        }

        /// <summary>
        /// Sets a registry value safely with proper error handling.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static void SetRegistryValue(RegistryKey registryKey, string? name, object value)
        {
            ArgumentNullException.ThrowIfNull(registryKey);
            ArgumentNullException.ThrowIfNull(value);

            try
            {
                Log.Debug("[*] Writing '{Value}' to {RegistryKey}\\{ValueName}", value, registryKey.Name, name ?? "(Default)");

                registryKey.SetValue(name, value);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"Unauthorized access when writing to registry: {registryKey.Name}\\{name ?? "(Default)"}", ex);
            }
        }

        public static bool SetRegistryStringValueIfDifferent(RegistryKey registryKey, string? name, string value)
        {
            ArgumentNullException.ThrowIfNull(registryKey);
            ArgumentNullException.ThrowIfNull(value);

            var currentValue = GetRegistryStringValue(registryKey, name);

            if (string.Equals(currentValue, value, StringComparison.OrdinalIgnoreCase))
                return false;

            SetRegistryValue(registryKey, name, value);

            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static void DeleteRegistrySubKeyTree(RegistryKey hive, string keyPath, bool throwOnMissingSubKey = true)
        {
            ArgumentNullException.ThrowIfNull(hive);
            ArgumentException.ThrowIfNullOrWhiteSpace(keyPath);

            hive.DeleteSubKeyTree(keyPath, throwOnMissingSubKey);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static string GetHiveName(RegistryKey hive)
        {
            ArgumentNullException.ThrowIfNull(hive);

            if (hive == Registry.CurrentUser) return "HKEY_CURRENT_USER";
            if (hive == Registry.Users) return "HKEY_USERS";
            if (hive == Registry.LocalMachine) return "HKEY_LOCAL_MACHINE";
            if (hive == Registry.ClassesRoot) return "HKEY_CLASSES_ROOT";
            if (hive == Registry.CurrentConfig) return "HKEY_CURRENT_CONFIG";

            return hive.ToString();
        }

        public static void CleanRegistryKeys(RegistryKey registryHive, IEnumerable<string> keyPaths, ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(registryHive);
            ArgumentNullException.ThrowIfNull(keyPaths);

            foreach (var keyPath in keyPaths)
                DeleteRegistryKey(registryHive, keyPath, logger);
        }

        /// <summary>
        /// Cleans registry keys for the current user.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static void CleanCurrentUserRegistry(IEnumerable<string> keyPaths, ILogger? logger = null)
        {
            CleanRegistryKeys(Registry.CurrentUser, keyPaths, logger);
        }

        /// <summary>
        /// Cleans registry keys for all loaded user hives.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static void CleanAllUsersRegistry(IEnumerable<string> keyPaths, ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(keyPaths);

            var log = logger ?? Log.Logger;
            var keyPathList = keyPaths.ToArray();

            try
            {
                var subKeyNames = GetSubKeyNames(Registry.Users);

                log.Information("Found {Count} user registry hives", subKeyNames.Length);

                foreach (var sid in subKeyNames)
                {
                    if (ShouldSkipUserRegistryHive(sid))
                        continue;

                    try
                    {
                        log.Information("Checking registry for user SID: {Sid}", sid);

                        foreach (var keyPath in keyPathList)
                            DeleteRegistryKey(Registry.Users, $@"{sid}\{keyPath}", log);
                    }
                    catch (Exception ex)
                    {
                        log.Warning(ex, "Error accessing registry for user SID: {Sid}", sid);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to process user registry hives");
            }
        }

        /// <summary>
        /// Cleans registry keys from the local machine hive.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static void CleanLocalMachineRegistry(IEnumerable<string> keyPaths, ILogger? logger = null)
        {
            CleanRegistryKeys(Registry.LocalMachine, keyPaths, logger);
        }

        /// <summary>
        /// Deletes a registry key from the specified registry hive when it exists.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static void DeleteRegistryKey(RegistryKey registryHive, string keyPath, ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(registryHive);
            ArgumentException.ThrowIfNullOrWhiteSpace(keyPath);

            var log = logger ?? Log.Logger;
            var hiveName = GetHiveName(registryHive);

            try
            {
                log.Information("Checking registry key: {RegistryHive}\\{KeyPath}", hiveName, keyPath);

                if (!RegistryKeyExists(registryHive, keyPath))
                {
                    log.Debug("Registry key does not exist: {RegistryHive}\\{KeyPath}", hiveName, keyPath);
                    return;
                }

                DeleteRegistrySubKeyTree(registryHive, keyPath);
                log.Information("Registry key deleted: {RegistryHive}\\{KeyPath}", hiveName, keyPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                log.Warning(ex, "Access denied to registry key: {RegistryHive}\\{KeyPath}. Administrative privileges may be required.", hiveName, keyPath);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Unable to delete registry key {RegistryHive}\\{KeyPath}", hiveName, keyPath);
            }
        }

        private static bool ShouldSkipUserRegistryHive(string sid) =>
            string.Equals(sid, ".DEFAULT", StringComparison.OrdinalIgnoreCase) ||
            sid.EndsWith("_Classes", StringComparison.OrdinalIgnoreCase);
    }
}
