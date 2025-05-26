using Serilog;
using System.Diagnostics;

namespace Sirstrap.Core
{
    /// <summary>
    /// Provides functionality to launch the Roblox application with specific configurations.
    /// </summary>
    public static class ApplicationLauncher
    {
        private const string ROBLOX_PLAYER_BETA_EXE = "RobloxPlayerBeta.exe";
        private const int PROCESS_POLLING_INTERVAL_MS = 100;

        /// <summary>
        /// Launches the Roblox application with the specified download configuration.
        /// </summary>
        /// <param name="downloadConfiguration">Configuration containing version and launch parameters.</param>
        /// <returns>
        /// <c>true</c> if the application was successfully launched; otherwise, <c>false</c>.
        /// </returns>
        public static bool Launch(DownloadConfiguration downloadConfiguration)
        {
            var robloxPlayerBetaExePath = Path.Combine(PathManager.GetVersionInstallPath(downloadConfiguration.Version), ROBLOX_PLAYER_BETA_EXE);

            if (!File.Exists(robloxPlayerBetaExePath))
            {
                Log.Warning("[*] Roblox has not been found in: {0}.", robloxPlayerBetaExePath);

                return false;
            }

            var capturedSingleton = false;
            var multiInstanceEnabled = SettingsManager.GetSettings().MultiInstance;

            try
            {
                if (multiInstanceEnabled)
                {
                    capturedSingleton = SingletonManager.CaptureSingleton();
                }

                ProcessStartInfo robloxPlayerBetaExeStartInfo = new()
                {
                    FileName = robloxPlayerBetaExePath,
                    WorkingDirectory = Path.GetDirectoryName(robloxPlayerBetaExePath),
                    UseShellExecute = true
                };

                if (!string.IsNullOrEmpty(downloadConfiguration.LaunchUrl))
                {
                    robloxPlayerBetaExeStartInfo.Arguments = downloadConfiguration.LaunchUrl;

                    Log.Information("[*] Launch url: {0}.", robloxPlayerBetaExeStartInfo.Arguments);
                }

                Log.Information("[*] Launching Roblox ({0})...", robloxPlayerBetaExePath);

                var robloxPlayerBetaExeProcess = Process.Start(robloxPlayerBetaExeStartInfo) ?? throw new Exception();

                Log.Information("[*] Waiting for Roblox process input idle...");

                robloxPlayerBetaExeProcess.WaitForInputIdle();

                if (capturedSingleton)
                {
                    Log.Information("[*][MultiInstance] Waiting for Roblox process exit...");

                    while (SingletonManager.HasCapturedSingleton && !robloxPlayerBetaExeProcess.HasExited && Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ROBLOX_PLAYER_BETA_EXE)).Length > 0)
                    {
                        Thread.Sleep(PROCESS_POLLING_INTERVAL_MS);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error during the launch of Roblox: {0}.", ex.Message);

                return false;
            }
            finally
            {
                if (capturedSingleton)
                {
                    SingletonManager.ReleaseSingleton();
                }
            }
        }
    }
}