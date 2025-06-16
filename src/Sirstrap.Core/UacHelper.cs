namespace Sirstrap.Core
{
    /// <summary>
    /// Utility class that provides functionality for managing User Account Control (UAC) elevation
    /// and handling application restarts with elevated privileges when required.
    /// </summary>
    public static class UacHelper
    {
        /// <summary>
        /// Ensures that an operation requiring elevated privileges is executed with the necessary permissions,
        /// automatically handling the elevation process if required.
        /// </summary>
        /// <param name="operation">The operation delegate that requires elevated privileges.</param>
        /// <param name="arguments">Command-line arguments for the elevated instance if restart is needed.</param>
        /// <param name="operationDescription">A descriptive name of the operation for logging purposes.</param>
        /// <returns>
        /// <c>true</c> if the operation completed successfully;
        /// <c>false</c> if elevation is required or if the operation failed.
        /// </returns>
        public static bool EnsureAdministratorPrivileges(Func<bool> operation, string[] arguments, string operationDescription)
        {
            try
            {
                bool result = operation();

                if (result)
                {
                    Log.Information("[*] Operation '{0}' completed successfully", operationDescription);

                    return true;
                }

                if (!IsRunningAsAdministrator())
                {
                    Log.Information("[*] Operation '{0}' requires elevated privileges. Initiating elevation...", operationDescription);

                    if (RestartAsAdministrator(arguments))
                        Environment.Exit(0);
                    else
                        Log.Error("[!] Failed to elevate privileges for operation '{0}'", operationDescription);
                }
                else
                    Log.Error("[!] Operation '{0}' failed despite having elevated privileges", operationDescription);

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Operation '{0}' failed with error: {1}", operationDescription, ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Determines whether the current application instance is running with elevated administrator privileges.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the application has administrator privileges; otherwise, <c>false</c>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public static bool IsRunningAsAdministrator()
        {
            try
            {
                using WindowsIdentity identity = WindowsIdentity.GetCurrent();

                WindowsPrincipal principal = new(identity);

                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to verify administrator privileges: {0}", ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Initiates a restart of the current application with elevated administrator privileges.
        /// </summary>
        /// <param name="arguments">Command-line arguments to be passed to the elevated instance.</param>
        /// <returns>
        /// <c>true</c> if the restart process was successfully initiated; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method triggers the Windows UAC prompt to request elevation.
        /// The current application instance should terminate upon successful elevation.
        /// If the user denies the UAC prompt, the method returns <c>false</c>.
        /// </remarks>
        public static bool RestartAsAdministrator(string[] arguments)
        {
            try
            {
                string? exePath = Process.GetCurrentProcess().MainModule?.FileName;

                if (string.IsNullOrEmpty(exePath))
                {
                    Log.Error("[!] Failed to retrieve current executable path");

                    return false;
                }

                ProcessStartInfo startInfo = new()
                {
                    FileName = exePath,
                    Arguments = string.Join(" ", arguments.Select(arg => $"\"{arg}\"")),
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Log.Information("[*] Initiating application restart with elevated privileges...");
                Process.Start(startInfo);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to restart with elevated privileges: {0}", ex.Message);

                return false;
            }
        }
    }
}