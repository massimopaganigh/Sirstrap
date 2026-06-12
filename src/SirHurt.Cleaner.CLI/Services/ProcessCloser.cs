namespace SirHurt.Cleaner.CLI.Services
{
    public sealed class ProcessCloser(ILogger logger, IProcessManager processManager, IUserInteraction userInteraction, CleanerConfig config) : ICleanupStep
    {
        private bool EnsureApplicationsClosed()
        {
            var runningProcesses = config.ProcessesToClose.Where(processManager.IsProcessRunning).ToList();
            if (runningProcesses.Count == 0)
            {
                logger.Information("[*] No Roblox or SirHurt processes are running");
                return true;
            }
            foreach (var processName in runningProcesses) logger.Information("[*] Process is running and must be closed: {ProcessName}", processName);
            if (!userInteraction.Confirm("Roblox and/or SirHurt applications need to be closed before cleaning. Close them now?"))
            {
                logger.Warning("[!] User declined to close running applications — cleanup may be incomplete");
                return false;
            }
            var notClosed = runningProcesses.Where(processName => !processManager.TryKillProcess(processName)).ToList();
            foreach (var processName in notClosed) logger.Warning("[!] Could not close every instance of {ProcessName}", processName);
            return notClosed.Count == 0;
        }

        public void Execute()
        {
            if (!EnsureApplicationsClosed()) logger.Warning("[!] Some applications are still running — locked files may not be removed");
        }

        public string Name => "Close running applications";
    }
}
