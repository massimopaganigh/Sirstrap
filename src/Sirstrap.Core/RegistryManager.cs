namespace Sirstrap.Core
{
    /// <summary>
    /// Manages Windows registry operations for registering Sirstrap as a protocol handler.
    /// Provides functionality to register Sirstrap as the default handler for custom protocol URLs.
    /// </summary>
    public static class RegistryManager
    {
        /// <summary>
        /// Checks if Sirstrap is already registered as the handler for the specified protocol.
        /// </summary>
        /// <param name="protocol">The protocol scheme to check in the Windows registry.</param>
        /// <returns>
        /// <c>true</c> if Sirstrap is already registered as the handler for the protocol; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="Exception">Thrown when registry access fails or other errors occur.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Convalida compatibilità della piattaforma", Justification = "<In sospeso>")]
        private static bool IsProtocolAlreadyRegistered(string protocol)
        {
            try
            {
                string expectedExePath = $"{AppDomain.CurrentDomain.BaseDirectory}{AppDomain.CurrentDomain.FriendlyName}";

                using RegistryKey? protocolKey = Registry.ClassesRoot.OpenSubKey(protocol);

                if (protocolKey == null)
                {
                    Log.Information("[*] Protocol {0} is not registered in the registry.", protocol);

                    return false;
                }

                using RegistryKey? shellKey = protocolKey.OpenSubKey("shell");

                if (shellKey == null)
                {
                    Log.Information("[*] Protocol {0} exists but has no shell configuration.", protocol);

                    return false;
                }

                using RegistryKey? openKey = shellKey.OpenSubKey("open");

                if (openKey == null)
                {
                    Log.Information("[*] Protocol {0} exists but has no open command configuration.", protocol);

                    return false;
                }

                using RegistryKey? commandKey = openKey.OpenSubKey("command");

                if (commandKey == null)
                {
                    Log.Information("[*] Protocol {0} exists but has no command configuration.", protocol);

                    return false;
                }

                string? currentCommand = commandKey.GetValue(string.Empty)?.ToString();
                string expectedCommand = $"\"{expectedExePath}\" %1";
                bool isRegistered = string.Equals(currentCommand, expectedCommand, StringComparison.OrdinalIgnoreCase);
                
                if (isRegistered)
                    Log.Information("[*] Protocol {0} is already correctly registered with Sirstrap ({1}).", protocol, expectedExePath);
                else
                    Log.Information("[*] Protocol {0} is registered with a different handler: {1}", protocol, currentCommand ?? "null");

                return isRegistered;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error checking protocol registration for {0}: {1}", protocol, ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Executes the low-level registry operations to register the protocol handler.
        /// Creates the necessary registry keys and values in HKEY_CLASSES_ROOT.
        /// </summary>
        /// <param name="protocol">The protocol scheme to register in the Windows registry.</param>
        /// <returns>
        /// <c>true</c> if all registry operations completed successfully; otherwise, <c>false</c> if an exception occurred.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when insufficient privileges to modify the registry.</exception>
        /// <exception cref="System.Security.SecurityException">Thrown when registry access is denied by system policy.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Convalida compatibilità della piattaforma", Justification = "<In sospeso>")]
        private static bool PerformRegistration(string protocol)
        {
            try
            {
                string exePath = $"{AppDomain.CurrentDomain.BaseDirectory}{AppDomain.CurrentDomain.FriendlyName}";

                Log.Information("[*] Registration of Sirstrap ({0}) as a handler of the {1} protocol.", exePath, protocol);

                using RegistryKey protocolKey = Registry.ClassesRoot.CreateSubKey(protocol);
                using RegistryKey shellKey = protocolKey.CreateSubKey("shell");
                using RegistryKey openKey = shellKey.CreateSubKey("open");
                using RegistryKey commandKey = openKey.CreateSubKey("command");

                commandKey.SetValue(string.Empty, $"\"{exePath}\" %1");

                Log.Information("[*] Registration successfully completed.");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Registration ended with exception: {0}.", ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Registers Sirstrap as the default handler for the specified protocol scheme.
        /// Checks if Sirstrap is already registered before requesting administrator privileges.
        /// </summary>
        /// <param name="protocol">The protocol scheme to register (e.g., "roblox-player", "steam").</param>
        /// <param name="arguments">The command-line arguments to pass when restarting the application with elevated privileges.</param>
        /// <returns>
        /// <c>true</c> if the protocol registration completed successfully or was already registered; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method first checks if Sirstrap is already registered as the handler for the specified protocol.
        /// If it is already correctly registered, no action is taken and the method returns <c>true</c>.
        /// </para>
        /// <para>
        /// Only if registration is required will the method request administrator privileges and modify
        /// the Windows registry under HKEY_CLASSES_ROOT to associate the specified protocol with Sirstrap.
        /// When users click protocol links in browsers or other applications, Windows will launch Sirstrap
        /// with the URL as a command-line argument.
        /// </para>
        /// <para>
        /// Administrative privileges are required for registry modifications. If the current process lacks these
        /// privileges, the application will automatically restart with elevation using UAC.
        /// </para>
        /// </remarks>
        public static bool RegisterProtocolHandler(string protocol, string[] arguments)
        {
            if (IsProtocolAlreadyRegistered(protocol))
            {
                Log.Information("[*] Protocol {0} is already registered with Sirstrap. No action required.", protocol);

                return true;
            }

            Log.Information("[*] Protocol {0} requires registration. Requesting administrator privileges...", protocol);
            
            return UacHelper.EnsureAdministratorPrivileges(() => PerformRegistration(protocol), arguments, $"Protocol registration for {protocol}");
        }
    }
}