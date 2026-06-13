namespace Sirstrap.Core.Tests.Activity
{
    public class RobloxActivityWatcherTests
    {
        [Fact]
        public void CurrentServerLocation_DefaultsToUnknown()
        {
            RobloxActivityWatcher watcher = new(new FakeServerLocationService());

            Assert.Equal("UNKNOWN", watcher.CurrentServerLocation);
        }

        [Fact]
        public void StartAndStopWatching_DoNotThrow()
        {
            RobloxActivityWatcher watcher = new(new FakeServerLocationService());

            var exception = Record.Exception(() =>
            {
                watcher.StartWatching();
                watcher.StopWatching();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void StopWatching_IsSafe_WhenNeverStarted()
        {
            RobloxActivityWatcher watcher = new(new FakeServerLocationService());

            Assert.Null(Record.Exception(watcher.StopWatching));
        }

        [Fact]
        public void ServerLocationChanged_CanBeSubscribedAndUnsubscribed()
        {
            RobloxActivityWatcher watcher = new(new FakeServerLocationService());

            var exception = Record.Exception(() =>
            {
                EventHandler<string> handler = (_, _) => { };

                watcher.ServerLocationChanged += handler;
                watcher.ServerLocationChanged -= handler;
            });

            Assert.Null(exception);
            Assert.Equal("UNKNOWN", watcher.CurrentServerLocation);
        }
    }
}
