namespace Sirstrap.Core.Tests.Ipc
{
    public class IpcServiceTests
    {
        private static string NewPipeName() => $"sirstrap-test-{Guid.NewGuid():N}";

        [Fact]
        public async Task SendMessage_IsReceivedByServer()
        {
            string pipeName = NewPipeName();
            IpcService service = new();
            TaskCompletionSource<string> received = new(TaskCreationOptions.RunContinuationsAsynchronously);

            service.MessageReceived += (_, message) => received.TrySetResult(message);

            await service.StartAsync(pipeName, TestContext.Current.CancellationToken);

            try
            {
                bool sent = await IpcService.SendMessageAsync(pipeName, "hello-ipc", TestContext.Current.CancellationToken);
                Assert.True(sent);

                var completed = await Task.WhenAny(received.Task, Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

                Assert.Same(received.Task, completed);
                Assert.Equal("hello-ipc", await received.Task);
            }
            finally
            {
                await service.StopAsync();
            }
        }

        [Fact]
        public async Task StartAsync_IsIdempotent()
        {
            string pipeName = NewPipeName();
            IpcService service = new();

            var exception = await Record.ExceptionAsync(async () =>
            {
                await service.StartAsync(pipeName, TestContext.Current.CancellationToken);
                await service.StartAsync(pipeName, TestContext.Current.CancellationToken);
                await service.StopAsync();
            });

            Assert.Null(exception);
        }

        [Fact]
        public async Task StopAsync_IsSafe_WhenNotStarted()
        {
            IpcService service = new();

            Assert.Null(await Record.ExceptionAsync(service.StopAsync));
        }

        [Fact]
        public async Task SendMessageAsync_ReturnsFalse_WhenNoServerListening()
        {
            bool sent = await IpcService.SendMessageAsync(NewPipeName(), "nobody-home", TestContext.Current.CancellationToken);

            Assert.False(sent);
        }
    }
}
