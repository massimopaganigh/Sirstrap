namespace Sirstrap.Core
{
    public static partial class RegistryManager
    {
        private const string DefaultIconSubKey = "DefaultIcon";
        // Registry path constants
        private const string RegistryBasePath = @"Software\Classes";
        private const string ShellOpenCommand = @"shell\open\command";
        private const string UrlProtocolValue = "URL Protocol";

        // Valid protocol name pattern (alphanumeric and hyphens only)
        private static readonly Regex ValidProtocolPattern = ValidProtocolPatternR();

        /// <summary>
        /// Checks if a protocol is already correctly registered and registers it if needed.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown when registry access is denied</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Convalida compatibilità della piattaforma", Justification = "<In sospeso>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Rimuovere l'eliminazione non necessaria", Justification = "<In sospeso>")]
        private static bool EnsureProtocolRegistration(string protocol)
        {
            try
            {
                var expectedCommand = GetExpectedCommand();

                // Check if protocol is already registered correctly
                // Use CurrentUser\Software\Classes path to match where we write
                using var commandKey = OpenRegistrySubKey(Registry.CurrentUser, GetProtocolCommandKeyPath(protocol));

                if (commandKey != null)
                {
                    var currentCommand = GetRegistryStringValue(commandKey, string.Empty);

                    if (string.Equals(currentCommand, expectedCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Information("[*] Protocol {0} is already correctly registered with Sirstrap.", protocol);

                        return true;
                    }

                    Log.Information("[*] Protocol {0} is registered with a different handler: {1}", protocol, currentCommand ?? "null");
                }
                else
                    Log.Information("[*] Protocol {0} is not registered or has incomplete configuration.", protocol);

                // Register or update the protocol (pass expectedCommand to avoid recalculating)
                return RegisterProtocol(protocol, expectedCommand);
            }
            catch (UnauthorizedAccessException)
            {
                // Let UnauthorizedAccessException bubble up to caller for UAC elevation
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error checking protocol registration for {0}: {1}", protocol, ex.Message);

                return false;
            }
        }

        private static string GetExpectedCommand() => $"cmd /c start \"\" \"{AppDomain.CurrentDomain.BaseDirectory}{AppDomain.CurrentDomain.FriendlyName}\" \"%1\"";

        private static string GetProtocolKeyPath(string protocol) => $@"{RegistryBasePath}\{protocol}";

        private static string GetProtocolCommandKeyPath(string protocol) => $@"{GetProtocolKeyPath(protocol)}\{ShellOpenCommand}";

        /// <summary>
        /// Registers a protocol handler in the Windows Registry.
        /// Uses CurrentUser\Software\Classes path which doesn't require admin privileges.
        /// Based on Bloxstrap's registry handling approach.
        /// </summary>
        /// <param name="protocol">The protocol name to register</param>
        /// <param name="expectedCommand">The command to associate with the protocol</param>
        /// <exception cref="UnauthorizedAccessException">Thrown when registry access is denied</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Convalida compatibilità della piattaforma", Justification = "<In sospeso>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Rimuovere l'eliminazione non necessaria", Justification = "<In sospeso>")]
        private static bool RegisterProtocol(string protocol, string expectedCommand)
        {
            try
            {
                Log.Information("[*] Registering protocol {0} with command: {1}", protocol, expectedCommand);

                // Use CurrentUser\Software\Classes instead of ClassesRoot - doesn't require admin privileges
                // CreateSubKey creates the key if it doesn't exist, or opens it with write access if it does
                using var protocolKey = CreateRegistrySubKey(Registry.CurrentUser, GetProtocolKeyPath(protocol), $"protocol {protocol}");
                using var defaultIconKey = CreateRegistrySubKey(protocolKey, DefaultIconSubKey, $"DefaultIcon registry key for protocol {protocol}");
                using var commandKey = CreateRegistrySubKey(protocolKey, ShellOpenCommand, $"command registry key for protocol {protocol}");

                // Set URL Protocol attributes if not already set
                if (GetRegistryStringValue(protocolKey, string.Empty) is null)
                {
                    SetRegistryValue(protocolKey, string.Empty, $"URL: {protocol} Protocol");
                    SetRegistryValue(protocolKey, UrlProtocolValue, string.Empty);
                }

                // Set the default icon path (always ensure it's set correctly)
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);

                SetRegistryStringValueIfDifferent(defaultIconKey, string.Empty, iconPath);

                // Only update command if it's different from expected
                SetRegistryStringValueIfDifferent(commandKey, string.Empty, expectedCommand);

                Log.Information("[*] Protocol {0} registered successfully.", protocol);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                // Let UnauthorizedAccessException bubble up to caller for UAC elevation
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to register protocol {0}: {1}", protocol, ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Validates a protocol name to prevent registry injection attacks.
        /// </summary>
        /// <param name="protocol">The protocol name to validate</param>
        /// <exception cref="ArgumentException">Thrown when the protocol name is invalid</exception>
        private static void ValidateProtocolName(string protocol)
        {
            if (string.IsNullOrWhiteSpace(protocol))
                throw new ArgumentException("Protocol name cannot be null, empty, or whitespace.", nameof(protocol));

            if (!ValidProtocolPattern.IsMatch(protocol))
                throw new ArgumentException($"Protocol name '{protocol}' contains invalid characters. Only alphanumeric characters and hyphens are allowed.", nameof(protocol));
        }

        [GeneratedRegex(@"^[a-zA-Z0-9\-]+$", RegexOptions.Compiled)]
        private static partial Regex ValidProtocolPatternR();

        /// <summary>
        /// Registers a protocol handler in the Windows Registry with UAC elevation fallback.
        /// </summary>
        /// <param name="protocol">The protocol name (e.g., "roblox-player")</param>
        /// <param name="arguments">Command-line arguments for UAC elevation if needed</param>
        /// <returns>True if registration succeeded, false otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when protocol name is invalid</exception>
        public static bool RegisterProtocolHandler(string protocol, string[] arguments)
        {
            // Validate protocol name to prevent registry injection attacks
            ValidateProtocolName(protocol);

            Log.Information("[*] Ensuring protocol {0} registration...", protocol);

            // First try without elevation since CurrentUser\Software\Classes doesn't typically need admin
            try
            {
                return EnsureProtocolRegistration(protocol);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Fall back to requesting admin privileges if necessary
                Log.Warning(ex, "[*] Registry access denied, requesting administrator privileges...");

                return UacHelper.EnsureAdministratorPrivileges(() => EnsureProtocolRegistration(protocol), arguments, $"Protocol registration for {protocol}");
            }
        }

        /// <summary>
        /// Removes a protocol handler registration from HKCU\Software\Classes.
        /// </summary>
        /// <param name="protocol">The protocol name to unregister.</param>
        /// <param name="logger">Optional logger override.</param>
        /// <exception cref="ArgumentException">Thrown when protocol name is invalid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
        public static void UnregisterProtocolHandler(string protocol, ILogger? logger = null)
        {
            ValidateProtocolName(protocol);

            var log = logger ?? Log.Logger;

            try
            {
                DeleteRegistrySubKeyTree(Registry.CurrentUser, GetProtocolKeyPath(protocol), throwOnMissingSubKey: false);

                log.Information("[*] Unregistered protocol: {Protocol}", protocol);
            }
            catch (Exception ex)
            {
                log.Warning(ex, "[!] Failed to unregister protocol {Protocol}: {Message}", protocol, ex.Message);
            }
        }

        public static void UnregisterProtocolHandlers(IEnumerable<string> protocols, ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(protocols);

            foreach (var protocol in protocols)
                UnregisterProtocolHandler(protocol, logger);
        }
    }
}
