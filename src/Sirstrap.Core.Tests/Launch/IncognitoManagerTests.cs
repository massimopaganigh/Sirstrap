namespace Sirstrap.Core.Tests.Launch
{
    public class IncognitoManagerTests
    {
        [Fact]
        public void RestoreRobloxFolderFromCache_ReturnsTrue_WhenNothingMoved()
        {
            IncognitoManager manager = new(new FakeSingletonManager(), new FakeRobloxProcessService());

            Assert.True(manager.RestoreRobloxFolderFromCache());
        }

        [Fact]
        public void OnInstanceTypeChange_AwayFromMaster_DoesNotThrow_WhenNothingMoved()
        {
            FakeSingletonManager singleton = new();
            IncognitoManager manager = new(singleton, new FakeRobloxProcessService());

            var exception = Record.Exception(() => singleton.RaiseInstanceTypeChanged(InstanceType.Slave));

            Assert.Null(exception);
            Assert.True(manager.RestoreRobloxFolderFromCache());
        }
    }
}
