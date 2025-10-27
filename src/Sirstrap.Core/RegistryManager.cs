namespace Sirstrap.Core
{
    public static class RegistryManager
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Convalida compatibilità della piattaforma", Justification = "<In sospeso>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Rimuovere l'eliminazione non necessaria", Justification = "<In sospeso>")]
        private static bool CreateProtocolRegistration(string protocol)
        {
            try
            {
                string expectedCommand = GetExpectedCommand();

                Log.Information("[*] Registering protocol {0} with command: {1}", protocol, expectedCommand);

                using RegistryKey protocolKey = Registry.ClassesRoot.CreateSubKey(protocol);
                using RegistryKey shellKey = protocolKey.CreateSubKey("shell");
                using RegistryKey openKey = shellKey.CreateSubKey("open");
                using RegistryKey commandKey = openKey.CreateSubKey("command");

                commandKey.SetValue(string.Empty, expectedCommand);

                Log.Information("[*] Protocol {0} registered successfully.", protocol);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to register protocol {0}: {1}", protocol, ex.Message);

                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Convalida compatibilità della piattaforma", Justification = "<In sospeso>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Rimuovere l'eliminazione non necessaria", Justification = "<In sospeso>")]
        private static bool EnsureProtocolRegistration(string protocol)
        {
            try
            {
                string expectedCommand = GetExpectedCommand();

                using RegistryKey? protocolKey = Registry.ClassesRoot.OpenSubKey(protocol);

                if (protocolKey == null)
                {
                    Log.Information("[*] Protocol {0} is not registered in the registry.", protocol);

                    return CreateProtocolRegistration(protocol);
                }

                using RegistryKey? shellKey = protocolKey.OpenSubKey("shell");

                if (shellKey == null)
                {
                    Log.Information("[*] Protocol {0} exists but has no shell configuration.", protocol);

                    return CreateProtocolRegistration(protocol);
                }

                using RegistryKey? openKey = shellKey.OpenSubKey("open");

                if (openKey == null)
                {
                    Log.Information("[*] Protocol {0} exists but has no open command configuration.", protocol);

                    return CreateProtocolRegistration(protocol);
                }

                using RegistryKey? commandKey = openKey.OpenSubKey("command");

                if (commandKey == null)
                {
                    Log.Information("[*] Protocol {0} exists but has no command configuration.", protocol);

                    return CreateProtocolRegistration(protocol);
                }

                string? currentCommand = commandKey.GetValue(string.Empty)?.ToString();
                bool isCorrectlyRegistered = string.Equals(currentCommand, expectedCommand, StringComparison.OrdinalIgnoreCase);

                if (isCorrectlyRegistered)
                {
                    Log.Information("[*] Protocol {0} is already correctly registered with Sirstrap.", protocol);

                    return true;
                }

                Log.Information("[*] Protocol {0} is registered with a different handler: {1}", protocol, currentCommand ?? "null");

                return CreateProtocolRegistration(protocol);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error checking protocol registration for {0}: {1}", protocol, ex.Message);

                return false;
            }
        }

        private static string GetExpectedCommand()
        {
            string exePath = $"{AppDomain.CurrentDomain.BaseDirectory}{AppDomain.CurrentDomain.FriendlyName}";

            // Use cmd /c start to create an independent process, avoiding browser subprocess issues
            // The empty "" after start is the window title (required when the executable path is quoted)
            // This ensures Sirstrap is not a child process of the browser when launched via protocol handler
            return $"cmd /c start \"\" \"{exePath}\" \"%1\"";
        }

        public static bool RegisterProtocolHandler(string protocol, string[] arguments)
        {
            Log.Information("[*] Ensuring protocol {0} registration...", protocol);

            return UacHelper.EnsureAdministratorPrivileges(() => EnsureProtocolRegistration(protocol), arguments, $"Protocol registration for {protocol}");
        }
    }
}
