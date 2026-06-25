namespace Sirstrap.Core.Cleaner
{
    public sealed class ProcessManager : IProcessManager
    {
        public bool IsProcessRunning(string processName)
        {
            var processes = Process.GetProcessesByName(processName);

            foreach (var process in processes)
                process.Dispose();

            return processes.Length > 0;
        }

        public bool TryKillProcess(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                    return true;

                Log.Information("[*] Closing {InstanceCount} instance(s) of {ProcessName}...", processes.Length, processName);

                var allClosed = true;

                foreach (var process in processes)
                    try
                    {
                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(5000);
                        Log.Information("[*] Closed the process {ProcessName} (PID {ProcessId}).", processName, process.Id);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[!] Failed to close the process {ProcessName} (PID {ProcessId}).", processName, process.Id);

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
                Log.Error(ex, "[!] Failed to close the process {ProcessName}.", processName);

                return false;
            }
        }
    }
}
