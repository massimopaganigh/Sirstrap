namespace Sirstrap.Core.Windows
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap registry operations target Windows.")]
    public sealed partial class ProtocolHandlerRegistrar(IUacService uacService) : IProtocolHandlerRegistrar
    {
        private const string DefaultIconSubKey = "DefaultIcon";
        private const string RegistryBasePath = @"Software\Classes";
        private const string ShellOpenCommand = @"shell\open\command";
        private const string UrlProtocolValue = "URL Protocol";

        public bool RegisterProtocolHandler(string protocol, string[] arguments)
        {
            ValidateProtocolName(protocol);

            Log.Information("[*] Ensuring the registration of the protocol {Protocol}...", protocol);

            try
            {
                return EnsureProtocolRegistration(protocol);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning(ex, "[!] The registry access was denied, requesting administrator privileges...");

                return uacService.EnsureAdministratorPrivileges(() => EnsureProtocolRegistration(protocol), arguments, $"Protocol registration for {protocol}");
            }
        }

        public void UnregisterProtocolHandler(string protocol, ILogger? logger = null)
        {
            ValidateProtocolName(protocol);

            var log = logger ?? Log.Logger;

            try
            {
                RegistryOperations.DeleteRegistrySubKeyTree(Registry.CurrentUser, GetProtocolKeyPath(protocol), throwOnMissingSubKey: false);

                log.Information("[*] Unregistered the protocol {Protocol}.", protocol);
            }
            catch (Exception ex)
            {
                log.Warning(ex, "[!] Failed to unregister the protocol {Protocol}.", protocol);
            }
        }

        public void UnregisterProtocolHandlers(IEnumerable<string> protocols, ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(protocols);

            foreach (var protocol in protocols)
                UnregisterProtocolHandler(protocol, logger);
        }

        private static bool EnsureProtocolRegistration(string protocol)
        {
            try
            {
                var expectedCommand = GetExpectedCommand();

                using var commandKey = RegistryOperations.OpenRegistrySubKey(Registry.CurrentUser, GetProtocolCommandKeyPath(protocol));

                if (commandKey != null)
                {
                    var currentCommand = RegistryOperations.GetRegistryStringValue(commandKey, string.Empty);

                    if (string.Equals(currentCommand, expectedCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Information("[*] The protocol {Protocol} is already correctly registered with Sirstrap.", protocol);

                        return true;
                    }

                    Log.Information("[*] The protocol {Protocol} is registered with the different handler {Handler}.", protocol, currentCommand ?? "null");
                }
                else
                    Log.Information("[*] The protocol {Protocol} is not registered or has an incomplete configuration.", protocol);

                return RegisterProtocol(protocol, expectedCommand);
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to check the registration of the protocol {Protocol}.", protocol);

                return false;
            }
        }

        private static string GetExpectedCommand() => $"cmd /c start \"\" \"{AppDomain.CurrentDomain.BaseDirectory}{AppDomain.CurrentDomain.FriendlyName}\" \"%1\"";

        private static string GetProtocolCommandKeyPath(string protocol) => $@"{GetProtocolKeyPath(protocol)}\{ShellOpenCommand}";

        private static string GetProtocolKeyPath(string protocol) => $@"{RegistryBasePath}\{protocol}";

        private static bool RegisterProtocol(string protocol, string expectedCommand)
        {
            try
            {
                Log.Information("[*] Registering the protocol {Protocol} with the command {Command}...", protocol, expectedCommand);

                using var protocolKey = RegistryOperations.CreateRegistrySubKey(Registry.CurrentUser, GetProtocolKeyPath(protocol), $"protocol {protocol}");
                using var defaultIconKey = RegistryOperations.CreateRegistrySubKey(protocolKey, DefaultIconSubKey, $"DefaultIcon registry key for protocol {protocol}");
                using var commandKey = RegistryOperations.CreateRegistrySubKey(protocolKey, ShellOpenCommand, $"command registry key for protocol {protocol}");

                if (RegistryOperations.GetRegistryStringValue(protocolKey, string.Empty) is null)
                {
                    RegistryOperations.SetRegistryValue(protocolKey, string.Empty, $"URL: {protocol} Protocol");
                    RegistryOperations.SetRegistryValue(protocolKey, UrlProtocolValue, string.Empty);
                }

                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);

                RegistryOperations.SetRegistryStringValueIfDifferent(defaultIconKey, string.Empty, iconPath);
                RegistryOperations.SetRegistryStringValueIfDifferent(commandKey, string.Empty, expectedCommand);

                Log.Information("[*] Registered the protocol {Protocol}.", protocol);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to register the protocol {Protocol}.", protocol);

                return false;
            }
        }

        private static void ValidateProtocolName(string protocol)
        {
            if (string.IsNullOrWhiteSpace(protocol))
                throw new ArgumentException("Protocol name cannot be null, empty, or whitespace.", nameof(protocol));

            if (!ValidProtocolPattern().IsMatch(protocol))
                throw new ArgumentException($"Protocol name '{protocol}' contains invalid characters. Only alphanumeric characters and hyphens are allowed.", nameof(protocol));
        }

        [GeneratedRegex(@"^[a-zA-Z0-9\-]+$", RegexOptions.Compiled)]
        private static partial Regex ValidProtocolPattern();
    }
}
