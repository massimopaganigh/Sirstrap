namespace Sirstrap.Core.Windows
{
    public sealed class UninstallService(IProtocolHandlerRegistrar protocolHandlerRegistrar) : IUninstallService
    {
        private static readonly string[] _protocols = ["roblox-player"];

        public void ScheduleCleanup()
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

        public void Uninstall()
        {
            Log.Information("[*] Uninstalling Sirstrap...");

            UnregisterProtocols();
            ScheduleCleanup();

            Log.Information("[*] Uninstalled Sirstrap, the application will close now.");
        }

        public void UnregisterProtocols()
        {
            protocolHandlerRegistrar.UnregisterProtocolHandlers(_protocols);
        }
    }
}
