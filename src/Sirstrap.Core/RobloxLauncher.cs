using Serilog;
using System.Diagnostics;

namespace Sirstrap.Core
{
    public static class RobloxLauncher
    {
        private const string ROBLOX_PLAYER_BETA_EXE = "RobloxPlayerBeta.exe";

        public static bool Launch(DownloadConfiguration downloadConfiguration)
        {
            string robloxPlayerBetaExePath = Path.Combine(PathManager.GetVersionInstallPath(downloadConfiguration.Version!), ROBLOX_PLAYER_BETA_EXE);

            if (!File.Exists(robloxPlayerBetaExePath))
            {
                Log.Warning("[*] Roblox not found in: {0}.", robloxPlayerBetaExePath);

                return false;
            }

            bool multiInstance = SettingsManager.GetSettings().MultiInstance;
            bool singletonCaptured = false;

            try
            {
                if (multiInstance)
                    singletonCaptured = SingletonManager.CaptureSingleton();

                ProcessStartInfo robloxPlayerBetaExeStartInfo = new()
                {
                    FileName = robloxPlayerBetaExePath,
                    UseShellExecute = true
                };

                string? launchUri = downloadConfiguration.LaunchUrl;

                if (!string.IsNullOrEmpty(launchUri))
                {
                    Log.Information("[*] Launching Roblox with URI: {0}...", launchUri);

                    robloxPlayerBetaExeStartInfo.Arguments = launchUri;
                }
                else
                    Log.Information("[*] Launching Roblox...");

                Process? robloxPlayerBetaExeProcess = Process.Start(robloxPlayerBetaExeStartInfo);

                Log.Information("[*] Waiting for input idle...");

                robloxPlayerBetaExeProcess?.WaitForInputIdle();

                if (singletonCaptured)
                {
                    Log.Information("[*] Waiting for exit (MultiInstance enabled)...");

                    while (SingletonManager.HasCapturedSingleton
                        && !robloxPlayerBetaExeProcess!.HasExited
                        && Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ROBLOX_PLAYER_BETA_EXE)).Length > 0)
                        Thread.Sleep(100);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Exception while launching Roblox: {0}.", ex.Message);

                return false;
            }
            finally
            {
                if (singletonCaptured)
                    SingletonManager.ReleaseSingleton();
            }
        }
    }
}