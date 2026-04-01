using Serilog;
using System;
using System.Diagnostics;
using System.Linq;

namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Standard process manager implementation
    /// </summary>
    public class StandardProcessManager : IProcessManager
    {
        private readonly ILogger _logger;

        public StandardProcessManager(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsProcessRunning(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        public IEnumerable<Process> GetRunningProcesses(string processName)
        {
            return Process.GetProcessesByName(processName);
        }

        public bool TryKillProcess(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0)
                {
                    return true; // Process is not running, so consider this a success
                }

                _logger.Information("Attempting to close {Count} instances of {ProcessName}", processes.Length, processName);

                bool allClosed = true;
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(5000); // Wait up to 5 seconds for the process to exit
                        _logger.Information("Successfully closed process {ProcessName} (PID: {PID})", processName, process.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failed to close process {ProcessName} (PID: {PID})", processName, process.Id);
                        allClosed = false;
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }

                return allClosed;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while attempting to close process {ProcessName}", processName);
                return false;
            }
        }
    }
}