namespace SirHurt.Cleaner.CLI.Services
{
    public sealed class CleanupOrchestrator(ILogger logger, IReadOnlyList<ICleanupStep> steps, IStatusLine statusLine)
    {
        public void Run()
        {
            int failedSteps = 0;
            for (int i = 0; i < steps.Count; i++)
            {
                ICleanupStep? step = steps[i];
                statusLine.SetStatus($"[{i + 1}/{steps.Count}] {step.Name}…");
                logger.Information("[*] Step {StepNumber}/{StepCount} — {StepName}", i + 1, steps.Count, step.Name);
                try
                {
                    step.Execute();
                    logger.Information("[*] Step {StepNumber}/{StepCount} — {StepName}: completed", i + 1, steps.Count, step.Name);
                }
                catch (Exception ex)
                {
                    failedSteps++;
                    logger.Error(ex, "[!] Step {StepNumber}/{StepCount} — {StepName}: failed, continuing with the next step", i + 1, steps.Count, step.Name);
                }
            }
            statusLine.Clear();
            if (failedSteps == 0) logger.Information("[*] Cleanup finished: all {StepCount} steps completed", steps.Count);
            else logger.Warning("[!] Cleanup finished: {FailedCount} of {StepCount} steps failed", failedSteps, steps.Count);
        }
    }
}
