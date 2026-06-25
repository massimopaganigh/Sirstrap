namespace Sirstrap.Core.Ipc
{
    public interface IIpcService
    {
        event EventHandler<string>? MessageReceived;

        Task StartAsync(string pipeName, CancellationToken cancellationToken = default);

        Task StopAsync();
    }
}
