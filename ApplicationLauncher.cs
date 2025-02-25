using Serilog;
using System.Diagnostics;

namespace Sirstrap
{
    /// <summary>
    /// Provides functionality to launch the Roblox application executable based on 
    /// specified version configuration.
    /// </summary>
    public static class ApplicationLauncher
    {
        /// <summary>
        /// Attempts to launch the Roblox Player application for the specified version.
        /// </summary>
        /// <param name="downloadConfiguration">Configuration containing the version information 
        /// for the Roblox executable to launch.</param>
        /// <returns>
        /// <c>true</c> if the application was successfully launched; 
        /// <c>false</c> if the executable file could not be found.
        /// </returns>
        /// <remarks>
        /// The method determines the executable path based on the version in the configuration,
        /// logs the launch attempt, and starts the process with its working directory set to 
        /// the executable's directory.
        /// </remarks>
        public static bool Launch(DownloadConfiguration downloadConfiguration)
        {
            var executablePath = Path.Combine(PathManager.GetVersionInstallPath(downloadConfiguration.Version), "RobloxPlayerBeta.exe");

            if (File.Exists(executablePath))
            {
                Log.Information("[*] Launching {0}...", executablePath);
                Process.Start(new ProcessStartInfo { FileName = executablePath, WorkingDirectory = Path.GetDirectoryName(executablePath), UseShellExecute = true });

                return true;
            }
            else
            {
                Log.Error("[!] Could not find {0}.", executablePath);

                return false;
            }
        }
    }
}