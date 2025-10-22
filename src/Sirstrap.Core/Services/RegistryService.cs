namespace Sirstrap.Core.Services
{
    public class RegistryService(IAdministratorService administratorService) : IRegistryService
    {
        #region PRIVATE METHODS
        private static string GetExpectedCommand() => $"\"{$"{AppDomain.CurrentDomain.BaseDirectory}{AppDomain.CurrentDomain.FriendlyName}"}\" %1";

        private static bool GetProtocolRegistration(string protocol)
        {
            try
            {
                using var protocolKey = Registry.ClassesRoot.OpenSubKey(protocol);

                if (protocolKey == null)
                    return SetProtocolRegistration(protocol);

                using var shellKey = protocolKey.OpenSubKey("shell");

                if (shellKey == null)
                    return SetProtocolRegistration(protocol);

                using var openKey = shellKey.OpenSubKey("open");

                if (openKey == null)
                    return SetProtocolRegistration(protocol);

                using var commandKey = openKey.OpenSubKey("command");

                if (commandKey == null)
                    return SetProtocolRegistration(protocol);

                if (string.Equals(GetExpectedCommand(), commandKey.GetValue(string.Empty)!.ToString(), StringComparison.OrdinalIgnoreCase))
                    return true;

                return SetProtocolRegistration(protocol);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool SetProtocolRegistration(string protocol)
        {
            try
            {
                using var protocolKey = Registry.ClassesRoot.CreateSubKey(protocol);
                using var shellKey = protocolKey.CreateSubKey("shell");
                using var openKey = shellKey.CreateSubKey("open");
                using var commandKey = openKey.CreateSubKey("command");

                commandKey.SetValue(string.Empty, GetExpectedCommand());

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        public bool RegisterProtocolHandler(string protocol, string[] arguments) => administratorService.Handle(() => GetProtocolRegistration(protocol), arguments, nameof(GetProtocolRegistration));
    }
}
