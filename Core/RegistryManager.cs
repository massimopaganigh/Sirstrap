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
                string executablePath = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName;

                Log.Information("[*] Registering Sirstrap as handler for {0} protocol.", protocol);
                Log.Information("[*] Executable path: {0}", executablePath);

                using RegistryKey key = Registry.ClassesRoot.OpenSubKey($"{protocol}\\shell\\open\\command", true);

                if (key == null)
                {
                    Log.Error("[!] Registry key not found: {0}\\shell\\open\\command", protocol);

                    return false;
                }

                key.SetValue(string.Empty, $"\"{executablePath}\" %1");

                Log.Information("[*] Successfully registered as {0} protocol handler.", protocol);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error registering protocol handler: {0}", ex.Message);

                return false;
            }
        }
    }
}