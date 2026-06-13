namespace Sirstrap.Core.Cleaner.Steps
{
    public sealed class ProcessCloser(IProcessManager processManager, IUserInteraction userInteraction, CleanerConfig config) : ICleanupStep
    {
        public string Name => "Close running applications";

        public void Execute()
        {
            if (!EnsureApplicationsClosed())
                Log.Warning("[!] Some applications are still running, locked files may not be removed.");
        }

        private bool EnsureApplicationsClosed()
        {
            var runningProcesses = config.ProcessesToClose.Where(processManager.IsProcessRunning).ToList();

            if (runningProcesses.Count == 0)
            {
                Log.Information("[*] No Roblox or SirHurt processes are running.");

                return true;
            }

            foreach (var processName in runningProcesses)
                Log.Information("[*] The process {ProcessName} is running and must be closed.", processName);

            if (!userInteraction.Confirm("Roblox and/or SirHurt applications need to be closed before cleaning. Close them now?"))
            {
                Log.Warning("[!] The user declined to close the running applications, the cleanup may be incomplete.");

                return false;
            }

            var notClosed = runningProcesses.Where(processName => !processManager.TryKillProcess(processName)).ToList();

            foreach (var processName in notClosed)
                Log.Warning("[!] Failed to close every instance of {ProcessName}.", processName);

            return notClosed.Count == 0;
        }
    }
}
