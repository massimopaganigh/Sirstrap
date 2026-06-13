namespace Sirstrap.Core.Windows
{
    public sealed class UacService : IUacService
    {
        public bool EnsureAdministratorPrivileges(Func<bool> operation, string[] arguments, string operationDescription)
        {
            try
            {
                bool result = operation();

                if (result)
                {
                    Log.Information("[*] Completed the operation {Operation}.", operationDescription);

                    return true;
                }

                if (!IsRunningAsAdministrator())
                {
                    Log.Information("[*] The operation {Operation} requires elevated privileges, initiating the elevation...", operationDescription);

                    if (RestartAsAdministrator(arguments))
                        Environment.Exit(0);
                    else
                        Log.Error("[!] Failed to elevate the privileges for the operation {Operation}.", operationDescription);
                }
                else
                    Log.Error("[!] The operation {Operation} failed despite having elevated privileges.", operationDescription);

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] The operation {Operation} failed.", operationDescription);

                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Sirstrap UAC operations target Windows.")]
        public bool IsRunningAsAdministrator()
        {
            try
            {
                using WindowsIdentity identity = WindowsIdentity.GetCurrent();

                WindowsPrincipal principal = new(identity);

                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to verify the administrator privileges.");

                return false;
            }
        }

        public bool RestartAsAdministrator(string[] arguments)
        {
            try
            {
                string? exePath = Process.GetCurrentProcess().MainModule?.FileName;

                if (string.IsNullOrEmpty(exePath))
                {
                    Log.Error("[!] Failed to retrieve the current executable path.");

                    return false;
                }

                ProcessStartInfo startInfo = new()
                {
                    FileName = exePath,
                    Arguments = string.Join(" ", arguments.Select(arg => $"\"{arg}\"")),
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Log.Information("[*] Restarting the application with elevated privileges...");
                Process.Start(startInfo);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to restart the application with elevated privileges.");

                return false;
            }
        }
    }
}
