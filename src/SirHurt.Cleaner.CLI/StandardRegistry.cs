using Microsoft.Win32;

namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Standard registry implementation
    /// </summary>
    public class StandardRegistry : IRegistry
    {
        public bool KeyExists(RegistryKey hive, string keyPath)
        {
            using var key = hive.OpenSubKey(keyPath);
            return key != null;
        }

        public void DeleteKey(RegistryKey hive, string keyPath)
        {
            hive.DeleteSubKeyTree(keyPath);
        }

        public string[] GetSubKeyNames(RegistryKey key)
        {
            return key.GetSubKeyNames();
        }

        public RegistryKey OpenSubKey(RegistryKey hive, string keyPath, bool writable)
        {
            return hive.OpenSubKey(keyPath, writable);
        }

        public string GetHiveName(RegistryKey hive)
        {
            if (hive == Registry.CurrentUser) return "HKEY_CURRENT_USER";
            if (hive == Registry.Users) return "HKEY_USERS";
            if (hive == Registry.LocalMachine) return "HKEY_LOCAL_MACHINE";
            if (hive == Registry.ClassesRoot) return "HKEY_CLASSES_ROOT";
            if (hive == Registry.CurrentConfig) return "HKEY_CURRENT_CONFIG";
            return hive.ToString();
        }
    }
}