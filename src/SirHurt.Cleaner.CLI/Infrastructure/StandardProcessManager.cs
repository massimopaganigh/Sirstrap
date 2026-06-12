namespace SirHurt.Cleaner.CLI.Infrastructure
{
    public sealed class StandardProcessManager(ILogger logger) : IProcessManager
    {
        public bool IsProcessRunning(string processName)
        {
            Process[]? processes = Process.GetProcessesByName(processName);
            foreach (Process? process in processes) process.Dispose();
            return processes.Length > 0;
        }

        public bool TryKillProcess(string processName)
        {
            try
            {
                Process[]? processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0) return true;
                logger.Information("[*] Closing {InstanceCount} instance(s) of {ProcessName}", processes.Length, processName);
                bool allClosed = true;
                foreach (Process? process in processes)
                    try
                    {
                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(5000);
                        logger.Information("[*] Closed process {ProcessName} (PID {ProcessId})", processName, process.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "[!] Could not close process {ProcessName} (PID {ProcessId})", processName, process.Id);
                        allClosed = false;
                    }
                    finally
                    {
                        process.Dispose();
                    }
                return allClosed;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[!] Error while closing process {ProcessName}", processName);
                return false;
            }
        }
    }
}
