namespace SirHurt.Cleaner.CLI.Abstractions
{
    public interface IProcessManager
    {
        bool IsProcessRunning(string processName);
        bool TryKillProcess(string processName);
    }
}
