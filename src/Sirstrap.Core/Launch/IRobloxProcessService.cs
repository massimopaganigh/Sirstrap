namespace Sirstrap.Core.Launch
{
    public interface IRobloxProcessService
    {
        List<Process> FindNewGameProcesses(HashSet<int> knownIds, int attempts = 20);

        int GetRunningGameProcessCount();

        void KillAll();

        void LogRunningGameProcesses();

        HashSet<int> SnapshotGameProcessIds();

        bool WaitForExit(int timeoutMs = 5000);
    }
}
