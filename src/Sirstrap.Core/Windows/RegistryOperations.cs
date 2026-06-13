namespace Sirstrap.Core.Windows
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
    public static class RegistryOperations
    {
        public static RegistryKey CreateRegistrySubKey(RegistryKey registryKey, string keyPath, string description)
        {
            ArgumentNullException.ThrowIfNull(registryKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(keyPath);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);

            var subKey = registryKey.CreateSubKey(keyPath);

            if (subKey != null)
                return subKey;

            Log.Error("[!] Failed to create or open the registry key for {Description}, the access may be denied.", description);

            throw new UnauthorizedAccessException($"Could not create registry key for {description}");
        }

        public static void DeleteRegistrySubKeyTree(RegistryKey hive, string keyPath, bool throwOnMissingSubKey = true)
        {
            ArgumentNullException.ThrowIfNull(hive);
            ArgumentException.ThrowIfNullOrWhiteSpace(keyPath);

            hive.DeleteSubKeyTree(keyPath, throwOnMissingSubKey);
        }

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

        public static string? GetRegistryStringValue(RegistryKey registryKey, string? name)
        {
            ArgumentNullException.ThrowIfNull(registryKey);

            return registryKey.GetValue(name)?.ToString();
        }

        public static string[] GetSubKeyNames(RegistryKey key)
        {
            ArgumentNullException.ThrowIfNull(key);

            return key.GetSubKeyNames();
        }

        public static RegistryKey? OpenRegistrySubKey(RegistryKey hive, string keyPath, bool writable = false)
        {
            ArgumentNullException.ThrowIfNull(hive);
            ArgumentException.ThrowIfNullOrWhiteSpace(keyPath);

            return hive.OpenSubKey(keyPath, writable);
        }

        public static bool RegistryKeyExists(RegistryKey hive, string keyPath)
        {
            ArgumentNullException.ThrowIfNull(hive);

            using var key = OpenRegistrySubKey(hive, keyPath);

            return key != null;
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

        public static void SetRegistryValue(RegistryKey registryKey, string? name, object value)
        {
            ArgumentNullException.ThrowIfNull(registryKey);
            ArgumentNullException.ThrowIfNull(value);

            try
            {
                Log.Debug("[*] Writing {Value} to {RegistryKey}\\{ValueName}...", value, registryKey.Name, name ?? "(Default)");

                registryKey.SetValue(name, value);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"Unauthorized access when writing to registry: {registryKey.Name}\\{name ?? "(Default)"}", ex);
            }
        }
    }
}
