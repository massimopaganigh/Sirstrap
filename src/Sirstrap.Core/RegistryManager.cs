using Microsoft.Win32;
using Serilog;

namespace Sirstrap.Core
{
    /// <summary>
    /// Manages Windows registry operations for registering Sirstrap as a protocol handler.
    /// Provides functionality to register Sirstrap as the default handler for custom protocol URLs.
    /// </summary>
    public static class RegistryManager
    {
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
        /// Ensures the operation runs with administrator privileges, restarting the application if necessary.
        /// </summary>
        /// <param name="protocol">The protocol scheme to register (e.g., "roblox-player", "steam").</param>
        /// <param name="arguments">The command-line arguments to pass when restarting the application with elevated privileges.</param>
        /// <returns>
        /// <c>true</c> if the protocol registration completed successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method modifies the Windows registry under HKEY_CLASSES_ROOT to associate the specified protocol
        /// with Sirstrap as the default handler. When users click protocol links in browsers or other applications,
        /// Windows will launch Sirstrap with the URL as a command-line argument.
        /// </para>
        /// <para>
        /// Administrative privileges are required for registry modifications. If the current process lacks these
        /// privileges, the application will automatically restart with elevation using UAC.
        /// </para>
        /// </remarks>
        public static bool RegisterProtocolHandler(string protocol, string[] arguments) => UacHelper.EnsureAdministratorPrivileges(() => PerformRegistration(protocol), arguments, $"Protocol registration for {protocol}");
    }
}