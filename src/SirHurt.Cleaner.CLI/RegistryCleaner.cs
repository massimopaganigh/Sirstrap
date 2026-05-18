using Microsoft.Win32;
using Serilog;
using System;
using System.Linq;

namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Manages registry cleaning operations
    /// </summary>
    public class RegistryCleaner
    {
        private readonly ILogger _logger;
        private readonly IRegistry _registry;
        private readonly CleanerConfig _config;

        public RegistryCleaner(
            ILogger logger,
            IRegistry registry,
            CleanerConfig config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Cleans registry keys for the current user
        /// </summary>
        public void CleanCurrentUserRegistry()
        {
            foreach (var keyPath in _config.RegistryKeys)
            {
                DeleteRegistryKey(Registry.CurrentUser, keyPath);
            }
        }

        /// <summary>
        /// Cleans registry keys for all users
        /// </summary>
        public void CleanAllUsersRegistry()
        {
            try
            {
                using var usersKey = Registry.Users;

                if (usersKey == null)
                {
                    _logger.Warning("Unable to access HKEY_USERS registry hive");
                    return;
                }

                var subKeyNames = _registry.GetSubKeyNames(usersKey);

                _logger.Information("Found {Count} user registry hives", subKeyNames.Length);

                foreach (var sid in subKeyNames)
                {
                    if (sid == ".DEFAULT" || sid.EndsWith("_Classes"))
                    {
                        continue;
                    }

                    try
                    {
                        _logger.Information("Checking registry for user SID: {Sid}", sid);

                        foreach (var keyPath in _config.RegistryKeys)
                        {
                            DeleteRegistryKey(usersKey, sid + @"\" + keyPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "Error accessing registry for user SID: {Sid}", sid);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to process user registry hives");
            }
        }

        /// <summary>
        /// Cleans registry keys from the local machine hive
        /// </summary>
        public void CleanLocalMachineRegistry()
        {
            foreach (var keyPath in _config.LocalMachineRegistryKeys)
            {
                DeleteRegistryKey(Registry.LocalMachine, keyPath);
            }
        }

        /// <summary>
        /// Deletes a registry key from the specified registry hive.
        /// </summary>
        /// <param name="registryHive">Registry hive containing the key</param>
        /// <param name="keyPath">Registry key path to delete</param>
        private void DeleteRegistryKey(RegistryKey registryHive, string keyPath)
        {
            try
            {
                string hiveName = _registry.GetHiveName(registryHive);
                _logger.Information("Checking registry key: {RegistryHive}\\{KeyPath}", hiveName, keyPath);

                if (!_registry.KeyExists(registryHive, keyPath))
                {
                    _logger.Debug("Registry key does not exist: {RegistryHive}\\{KeyPath}", hiveName, keyPath);
                    return;
                }

                _registry.DeleteKey(registryHive, keyPath);
                _logger.Information("Registry key deleted: {RegistryHive}\\{KeyPath}", hiveName, keyPath);
            }
            catch (UnauthorizedAccessException)
            {
                string hiveName = _registry.GetHiveName(registryHive);
                _logger.Warning("Access denied to registry key: {RegistryHive}\\{KeyPath}. Administrative privileges may be required.", hiveName, keyPath);
            }
            catch (Exception ex)
            {
                string hiveName = _registry.GetHiveName(registryHive);
                _logger.Error(ex, "Unable to delete registry key {RegistryHive}\\{KeyPath}", hiveName, keyPath);
            }
        }
    }
}