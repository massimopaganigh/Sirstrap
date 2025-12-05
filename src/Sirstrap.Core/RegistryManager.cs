namespace Sirstrap.Core
{
    public static class RegistryManager
    {
        /// <summary>
        /// Checks if a protocol is already correctly registered and registers it if needed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Convalida compatibilità della piattaforma", Justification = "<In sospeso>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Rimuovere l'eliminazione non necessaria", Justification = "<In sospeso>")]
        private static bool EnsureProtocolRegistration(string protocol)
        {
            try
            {
                var expectedCommand = GetExpectedCommand();

                // Check if protocol is already registered correctly
                // Use CurrentUser\Software\Classes path to match where we write
                using var commandKey = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{protocol}\shell\open\command");

                if (commandKey != null)
                {
                    var currentCommand = commandKey.GetValue(string.Empty)?.ToString();

                    if (string.Equals(currentCommand, expectedCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Information("[*] Protocol {0} is already correctly registered with Sirstrap.", protocol);

                        return true;
                    }

                    Log.Information("[*] Protocol {0} is registered with a different handler: {1}", protocol, currentCommand ?? "null");
                }
                else
                    Log.Information("[*] Protocol {0} is not registered or has incomplete configuration.", protocol);

                // Register or update the protocol
                return RegisterProtocol(protocol);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error checking protocol registration for {0}: {1}", protocol, ex.Message);

                return false;
            }
        }

        private static string GetExpectedCommand() => $"cmd /c start \"\" \"{AppDomain.CurrentDomain.BaseDirectory}{AppDomain.CurrentDomain.FriendlyName}\" \"%1\"";
        /// <summary>
        /// Registers a protocol handler in the Windows Registry.
        /// Uses CurrentUser\Software\Classes path which doesn't require admin privileges.
        /// Based on Bloxstrap's registry handling approach.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Convalida compatibilità della piattaforma", Justification = "<In sospeso>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Rimuovere l'eliminazione non necessaria", Justification = "<In sospeso>")]
        private static bool RegisterProtocol(string protocol)
        {
            try
            {
                var expectedCommand = GetExpectedCommand();

                Log.Information("[*] Registering protocol {0} with command: {1}", protocol, expectedCommand);

                // Use CurrentUser\Software\Classes instead of ClassesRoot - doesn't require admin privileges
                // CreateSubKey creates the key if it doesn't exist, or opens it with write access if it does
                using var protocolKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{protocol}");
                using var defaultIconKey = protocolKey.CreateSubKey("DefaultIcon");
                using var commandKey = protocolKey.CreateSubKey(@"shell\open\command");

                // Set URL Protocol attributes if not already set
                if (protocolKey.GetValue(string.Empty) is null)
                {
                    SetValueSafe(protocolKey, string.Empty, $"URL: {protocol} Protocol");
                    SetValueSafe(protocolKey, "URL Protocol", string.Empty);
                }

                // Set the default icon path (always ensure it's set correctly)
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);
                var currentIcon = defaultIconKey.GetValue(string.Empty)?.ToString();

                if (!string.Equals(currentIcon, iconPath, StringComparison.OrdinalIgnoreCase))
                    SetValueSafe(defaultIconKey, string.Empty, iconPath);

                // Only update command if it's different from expected
                var currentCommand = commandKey.GetValue(string.Empty)?.ToString();

                if (!string.Equals(currentCommand, expectedCommand, StringComparison.OrdinalIgnoreCase))
                    SetValueSafe(commandKey, string.Empty, expectedCommand);

                Log.Information("[*] Protocol {0} registered successfully.", protocol);

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex, "[!] Unauthorized access when registering protocol {0}. Registry write permissions may be restricted.", protocol);

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to register protocol {0}: {1}", protocol, ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Sets a registry value safely with proper error handling.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Convalida compatibilità della piattaforma", Justification = "<In sospeso>")]
        private static void SetValueSafe(RegistryKey registryKey, string? name, object value)
        {
            try
            {
                Log.Debug("[*] Writing '{0}' to {1}\\{2}", value, registryKey.Name, name ?? "(Default)");

                registryKey.SetValue(name, value);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex, "[!] Unauthorized access when writing to registry: {0}\\{1}", registryKey.Name, name ?? "(Default)");

                throw;
            }
        }

        public static bool RegisterProtocolHandler(string protocol, string[] arguments)
        {
            Log.Information("[*] Ensuring protocol {0} registration...", protocol);

            // First try without elevation since CurrentUser\Software\Classes doesn't typically need admin
            try
            {
                return EnsureProtocolRegistration(protocol);
            }
            catch (UnauthorizedAccessException)
            {
                // Fall back to requesting admin privileges if necessary
                Log.Warning("[*] Registry access denied, requesting administrator privileges...");

                return UacHelper.EnsureAdministratorPrivileges(() => EnsureProtocolRegistration(protocol), arguments, $"Protocol registration for {protocol}");
            }
        }
    }
}
