namespace Sirstrap.Core.Tests.Cleaner
{
    public class CleanupOrchestratorTests
    {
        [Fact]
        public void Run_ExecutesEveryStep_AndClearsStatus()
        {
            FakeStatusLine statusLine = new();
            FakeCleanupStep first = new("Step One");
            FakeCleanupStep second = new("Step Two");

            new CleanupOrchestrator([first, second], statusLine).Run();

            Assert.Equal(1, first.Executions);
            Assert.Equal(1, second.Executions);
            Assert.Equal(2, statusLine.Statuses.Count);
            Assert.True(statusLine.ClearCalls > 0);
        }

        [Fact]
        public void Run_ContinuesAfterFailingStep()
        {
            FakeStatusLine statusLine = new();
            FakeCleanupStep failing = new("Failing", () => throw new InvalidOperationException("boom"));
            FakeCleanupStep following = new("Following");

            new CleanupOrchestrator([failing, following], statusLine).Run();

            Assert.Equal(1, failing.Executions);
            Assert.Equal(1, following.Executions);
            Assert.True(statusLine.ClearCalls > 0);
        }

        [Fact]
        public void Run_HandlesEmptyStepList()
        {
            FakeStatusLine statusLine = new();

            new CleanupOrchestrator([], statusLine).Run();

            Assert.True(statusLine.ClearCalls > 0);
        }
    }
}
