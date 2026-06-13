namespace Sirstrap.Core.Cleaner
{
    public sealed class CleanupOrchestrator(IEnumerable<ICleanupStep> steps, IStatusLine statusLine) : ICleanupOrchestrator
    {
        public void Run()
        {
            var stepList = steps.ToList();
            int failedSteps = 0;

            for (int i = 0; i < stepList.Count; i++)
            {
                ICleanupStep step = stepList[i];

                statusLine.SetStatus($"[{i + 1}/{stepList.Count}] {step.Name}…");

                Log.Information("[*] Step {StepNumber}/{StepCount} — {StepName}...", i + 1, stepList.Count, step.Name);

                try
                {
                    step.Execute();

                    Log.Information("[*] Step {StepNumber}/{StepCount} — {StepName}: completed.", i + 1, stepList.Count, step.Name);
                }
                catch (Exception ex)
                {
                    failedSteps++;

                    Log.Error(ex, "[!] Step {StepNumber}/{StepCount} — {StepName}: failed, continuing with the next step.", i + 1, stepList.Count, step.Name);
                }
            }

            statusLine.Clear();

            if (failedSteps == 0)
                Log.Information("[*] Cleanup finished: all {StepCount} steps completed.", stepList.Count);
            else
                Log.Warning("[!] Cleanup finished: {FailedCount} of {StepCount} steps failed.", failedSteps, stepList.Count);
        }
    }
}
