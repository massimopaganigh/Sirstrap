namespace Sirstrap.Core.Tests.Launch
{
    [Collection("SingletonMutex")]
    public class SingletonManagerTests
    {
        [Fact]
        public void Capture_Release_TransitionsInstanceType_AndRaisesEvents()
        {
            FakeRobloxProcessService processes = new();
            SingletonManager manager = new(processes);
            List<InstanceType> transitions = [];
            manager.InstanceTypeChanged += (_, type) => transitions.Add(type);

            try
            {
                Assert.True(manager.CaptureSingleton());
                Assert.True(manager.HasCapturedSingleton);
                Assert.Equal(InstanceType.Master, manager.CurrentInstanceType);
                Assert.Equal(1, processes.KillAllCalls);

                Assert.True(manager.CaptureSingleton());
            }
            finally
            {
                Assert.True(manager.ReleaseSingleton());
            }

            Assert.False(manager.HasCapturedSingleton);
            Assert.Equal(InstanceType.None, manager.CurrentInstanceType);
            Assert.Contains(InstanceType.Master, transitions);
            Assert.Contains(InstanceType.None, transitions);
        }

        [Fact]
        public void Release_ReturnsFalse_WhenNotCaptured()
        {
            SingletonManager manager = new(new FakeRobloxProcessService());

            Assert.False(manager.ReleaseSingleton());
        }

        [Fact]
        public void InstanceType_HasExpectedMembers()
        {
            Assert.Equal(["None", "Master", "Slave"], Enum.GetNames<InstanceType>());
        }
    }

    [CollectionDefinition("SingletonMutex", DisableParallelization = true)]
    public sealed class SingletonMutexCollection
    {
    }
}
