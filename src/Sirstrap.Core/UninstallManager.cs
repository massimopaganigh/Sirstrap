namespace Sirstrap.Core
{
    public static class UninstallManager
    {
        private static readonly string[] _protocols = ["roblox-player"];

        /// <summary>
        /// Removes all Sirstrap protocol handler registrations from HKCU\Software\Classes.
        /// </summary>
        public static void UnregisterProtocols()
        {
            RegistryManager.UnregisterProtocolHandlers(_protocols);
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
