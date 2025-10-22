namespace Sirstrap.Core
{
    public static class UacHelper
    {
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
                Log.Warning(ex, "[*] Failed to verify administrator privileges: {0}", ex.Message);

                return false;
            }
        }

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
