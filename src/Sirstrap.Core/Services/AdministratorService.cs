namespace Sirstrap.Core.Services
{
    public class AdministratorService : IAdministratorService
    {
        public bool Handle(Func<bool> op, string[] args, string opDescription)
        {
            try
            {
                bool result = op();

                if (result)
                    return true;

                if (!IsRunningAsAdministrator())
                    if (RunAsAdministrator(args))
                        Environment.Exit(0);

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error handling {0}: {1}", opDescription, ex.Message);

                return false;
            }
        }

        #region PRIVATE METHODS
        private static bool IsRunningAsAdministrator()
        {
            try
            {
                using var windowsIdentity = WindowsIdentity.GetCurrent();

                return new WindowsPrincipal(windowsIdentity).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool RunAsAdministrator(string[] arguments)
        {
            try
            {
                var @this = Process.GetCurrentProcess().MainModule?.FileName;

                if (string.IsNullOrEmpty(@this))
                    return false;

                ProcessStartInfo startInfo = new()
                {
                    FileName = @this,
                    Arguments = string.Join(" ", arguments.Select(arg => $"\"{arg}\"")),
                    UseShellExecute = true,
                    Verb = "runas"
                };

                using var process = new Process
                {
                    StartInfo = startInfo
                };

                process.Start();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}
