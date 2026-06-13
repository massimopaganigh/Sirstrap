namespace Sirstrap.Core.Tests.Cleaner
{
    public class ProcessManagerTests
    {
        private readonly Sirstrap.Core.Cleaner.ProcessManager _manager = new();

        [Fact]
        public void IsProcessRunning_ReturnsFalse_ForNonExistentProcess()
        {
            Assert.False(_manager.IsProcessRunning($"sirstrap-nope-{Guid.NewGuid():N}"));
        }

        [Fact]
        public void TryKillProcess_ReturnsTrue_WhenNoInstancesRunning()
        {
            Assert.True(_manager.TryKillProcess($"sirstrap-nope-{Guid.NewGuid():N}"));
        }
    }
}
