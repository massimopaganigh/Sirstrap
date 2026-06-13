namespace Sirstrap.Core.Cleaner
{
    public interface IProcessManager
    {
        bool IsProcessRunning(string processName);

        bool TryKillProcess(string processName);
    }
}
