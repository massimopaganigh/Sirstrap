namespace Sirstrap.Core.Tests.Launch
{
    public class RobloxProcessServiceTests
    {
        private readonly RobloxProcessService _service = new();

        [Fact]
        public void GetRunningGameProcessCount_IsNonNegative()
        {
            Assert.True(_service.GetRunningGameProcessCount() >= 0);
        }

        [Fact]
        public void SnapshotGameProcessIds_ReturnsSet()
        {
            Assert.NotNull(_service.SnapshotGameProcessIds());
        }

        [Fact]
        public void FindNewGameProcessIds_ReturnsEmpty_WhenNoNewProcesses()
        {
            Assert.Empty(_service.FindNewGameProcessIds(_service.SnapshotGameProcessIds(), attempts: 1));
        }

        [Fact]
        public void AnyGameProcessRunning_ReturnsFalse_ForUnknownIds()
        {
            Assert.False(_service.AnyGameProcessRunning([int.MaxValue]));
        }

        [Fact]
        public void WaitForExit_ReturnsTrue_WhenNoProcessesRunning()
        {
            Assert.True(_service.WaitForExit(timeoutMs: 500));
        }

        [Fact]
        public void KillAll_AndLog_DoNotThrow()
        {
            var exception = Record.Exception(() =>
            {
                _service.KillAll();
                _service.LogRunningGameProcesses();
            });

            Assert.Null(exception);
        }
    }
}
