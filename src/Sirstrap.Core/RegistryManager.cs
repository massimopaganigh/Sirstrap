using Microsoft.Win32;
using Serilog;

namespace Sirstrap.Core
{
    /// <summary>
    /// Provides functionality to register Sirstrap as the handler for Roblox protocol URLs.
    /// </summary>
    public static class RegistryManager
    {
        /// <summary>
        /// Registers Sirstrap as the handler for the specified protocol.
        /// </summary>
        /// <param name="protocol">The protocol to register (e.g., "roblox-player").</param>
        /// <returns>
        /// <c>true</c> if the registration was successful; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method modifies the Windows registry to associate the specified protocol
        /// with Sirstrap, allowing web links to launch Sirstrap with the appropriate arguments.
        /// This requires administrative privileges.
        /// </remarks>
        public static bool RegisterProtocolHandler(string protocol)
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
    }
}