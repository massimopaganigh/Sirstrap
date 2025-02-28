using Serilog;
using System.Diagnostics;

namespace Sirstrap.Core
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
        /// <param name="waitForExit">Whether to wait for the Roblox process to exit.</param>
        /// <returns>
        /// <c>true</c> if the application was successfully launched; 
        /// <c>false</c> if the executable file could not be found.
        /// </returns>
        /// <remarks>
        /// The method determines the executable path based on the version in the configuration,
        /// logs the launch attempt, and starts the process with its working directory set to 
        /// the executable's directory. If a LaunchUrl is specified, it will be passed as an
        /// argument to the Roblox process to launch directly into a specific experience.
        /// </remarks>
        public static bool Launch(DownloadConfiguration downloadConfiguration, bool waitForExit = false)
        {
            var executablePath = Path.Combine(PathManager.GetVersionInstallPath(downloadConfiguration.Version), "RobloxPlayerBeta.exe");

            if (File.Exists(executablePath))
            {
                bool capturedSingleton = waitForExit && SingletonManager.CaptureSingleton();

                Log.Information("[*] Launching {0}...", executablePath);

                ProcessStartInfo startInfo = new()
                {
                    FileName = executablePath,
                    WorkingDirectory = Path.GetDirectoryName(executablePath),
                    UseShellExecute = true
                };

                if (!string.IsNullOrEmpty(downloadConfiguration.LaunchUrl))
                {
                    Log.Information("[*] With launch URL: {0}", downloadConfiguration.LaunchUrl);

                    startInfo.Arguments = downloadConfiguration.LaunchUrl;
                }

                var process = Process.Start(startInfo);

                if (capturedSingleton && process != null)
                {
                    Task.Run(() => WaitForProcessExit(process));
                }

                return true;
            }
            else
            {
                Log.Error("[!] Could not find {0}.", executablePath);

                return false;
            }
        }

        /// <summary>
        /// Waits for a Roblox process to exit and releases the singleton mutex afterward.
        /// </summary>
        /// <param name="process">The Roblox process to monitor.</param>
        private static void WaitForProcessExit(Process process)
        {
            Log.Information("[*] Waiting for Roblox to exit...");

            while (SingletonManager.HasCapturedSingleton && !process.HasExited && Process.GetProcessesByName("RobloxPlayerBeta").Length > 0)
            {
                Thread.Sleep(100);
            }

            SingletonManager.ReleaseSingleton();
            Log.Information("[*] Roblox has exited.");
        }
    }
}