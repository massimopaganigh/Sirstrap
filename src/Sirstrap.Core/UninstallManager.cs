namespace Sirstrap.Core
{
    public static class UninstallManager
    {
        private static readonly string[] _protocols = ["roblox-player"];
        private const string RegistryBasePath = @"Software\Classes";

        /// <summary>
        /// Removes all Sirstrap protocol handler registrations from HKCU\Software\Classes.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Convalida compatibilità della piattaforma", Justification = "Windows-only application")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Rimuovere l'eliminazione non necessaria", Justification = "Cross-platform suppression")]
        public static void UnregisterProtocols()
        {
            foreach (var protocol in _protocols)
            {
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree($@"{RegistryBasePath}\{protocol}", throwOnMissingSubKey: false);

                    Log.Information("[*] Unregistered protocol: {0}", protocol);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "[!] Failed to unregister protocol {0}: {1}", protocol, ex.Message);
                }
            }
        }

        /// <summary>
        /// Schedules post-exit deletion of the Sirstrap data folder and executable
        /// via a temporary batch script that runs after the process exits.
        /// </summary>
        public static void ScheduleCleanup()
        {
            var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap");
            var exePath = Environment.ProcessPath ?? string.Empty;
            var scriptPath = Path.Combine(Path.GetTempPath(), $"sirstrap_uninstall_{Guid.NewGuid():N}.cmd");

            var script = new StringBuilder();
            script.AppendLine("@echo off");
            script.AppendLine("timeout /t 3 /nobreak >nul");

            if (Directory.Exists(dataPath))
                script.AppendLine($"rmdir /s /q \"{dataPath}\"");

            if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                script.AppendLine($"del /f /q \"{exePath}\"");

            script.AppendLine("del /f /q \"%~f0\"");

            File.WriteAllText(scriptPath, script.ToString());

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{scriptPath}\"",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Performs a full Sirstrap uninstall: removes registry protocol entries and
        /// schedules deletion of the data folder and executable after the process exits.
        /// </summary>
        public static void Uninstall()
        {
            Log.Information("[*] Starting Sirstrap uninstall...");

            UnregisterProtocols();
            ScheduleCleanup();

            Log.Information("[*] Sirstrap uninstall complete. Application will close now.");
        }
    }
}
