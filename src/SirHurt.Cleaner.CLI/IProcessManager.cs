using System;
using System.Diagnostics;
using System.Linq;

namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Interface for process management operations
    /// </summary>
    public interface IProcessManager
    {
        bool IsProcessRunning(string processName);
        bool TryKillProcess(string processName);
        IEnumerable<Process> GetRunningProcesses(string processName);
    }
}