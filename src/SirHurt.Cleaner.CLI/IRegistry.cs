using Microsoft.Win32;
using System;
using System.Linq;

namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Interface for registry operations
    /// </summary>
    public interface IRegistry
    {
        bool KeyExists(RegistryKey hive, string keyPath);
        void DeleteKey(RegistryKey hive, string keyPath);
        string[] GetSubKeyNames(RegistryKey key);
        RegistryKey OpenSubKey(RegistryKey hive, string keyPath, bool writable);
        string GetHiveName(RegistryKey hive);
    }
}