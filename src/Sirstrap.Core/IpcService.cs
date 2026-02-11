namespace Sirstrap.Core
{
    public class IpcService
    {
        private bool _isRunning;
        private CancellationTokenSource? _serverCts;
        private Task? _serverTask;

        public event EventHandler<string>? MessageReceived;

        private async Task HandleClientAsync(NamedPipeServerStream pipeServer)
        {
            try
            {
                Log.Debug("[{0}] Handling client (PipeServer: {1})...", nameof(HandleClientAsync), pipeServer);

                var lengthBuffer = new byte[4];
                var bytesRead = await pipeServer.ReadAsync(lengthBuffer.AsMemory(0, 4));

                if (bytesRead != 4)
                    return;

                var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                if (messageLength <= 0
                    || messageLength > 1048576)
                    return;

                var messageBuffer = new byte[messageLength];
                var totalBytesRead = 0;

                while (totalBytesRead < messageLength)
                {
                    bytesRead = await pipeServer.ReadAsync(messageBuffer.AsMemory(totalBytesRead, messageLength - totalBytesRead));

                    if (bytesRead == 0)
                        return;

                    totalBytesRead += bytesRead;
                }

                if (totalBytesRead == messageLength)
                {
                    var message = Encoding.UTF8.GetString(messageBuffer);

                    Log.Debug("[{0}] Message received: {1}", nameof(HandleClientAsync), message);

                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(HandleClientAsync));
            }
            finally
            {
                await pipeServer.DisposeAsync();
            }
        }

        private async Task ListenForConnectionsAsync(string pipeName, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                NamedPipeServerStream? pipeServer = null;

                try
                {
                    Log.Debug("[{0}] Listening for connections (PipeName: {1}, CancellationToken: {2})...", nameof(ListenForConnectionsAsync), pipeName, cancellationToken);

                    pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    await pipeServer.WaitForConnectionAsync(cancellationToken);

                    var connectedPipe = pipeServer;

                    pipeServer = null;

                    _ = Task.Run(async () => await HandleClientAsync(connectedPipe), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    if (pipeServer != null)
                        await pipeServer.DisposeAsync();

                    break;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, nameof(ListenForConnectionsAsync));

                    if (pipeServer != null)
                        await pipeServer.DisposeAsync();

                    await Task.Delay(5000, cancellationToken);
                }
            }
        }

        public static async Task<bool> SendMessageAsync(string pipeName, string message, CancellationToken cancellationToken = default)
        {
            NamedPipeClientStream? pipeClient = null;

            try
            {
                Log.Debug("[{0}] Sending message (PipeName: {1}, Message: {2}, CancellationToken: {3})...", nameof(SendMessageAsync), pipeName, message, cancellationToken);

                pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);

                await pipeClient.ConnectAsync(5000, cancellationToken);

                if (!pipeClient.IsConnected)
                    return false;

                var messageBytes = Encoding.UTF8.GetBytes(message);

                await pipeClient.WriteAsync(BitConverter.GetBytes(messageBytes.Length), cancellationToken);
                await pipeClient.WriteAsync(messageBytes, cancellationToken);
                await pipeClient.FlushAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(SendMessageAsync));

                return false;
            }
            finally
            {
                if (pipeClient != null)
                    await pipeClient.DisposeAsync();
            }
        }

        public async Task StartAsync(string pipeName, CancellationToken cancellationToken = default)
        {
            try
            {
                Log.Debug("[{0}] Starting (PipeName: {1}, CancellationToken: {2})...", nameof(StartAsync), pipeName, cancellationToken);

                if (_isRunning)
                    return;

                _serverCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _isRunning = true;
                _serverTask = Task.Run(async () => await ListenForConnectionsAsync(pipeName, _serverCts.Token), _serverCts.Token);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(StartAsync));

                _isRunning = false;
            }
        }

        public async Task StopAsync()
        {
            try
            {
                Log.Debug("[{0}] Stopping...", nameof(StopAsync));

                if (!_isRunning)
                    return;

                if (_serverCts != null)
                    await _serverCts.CancelAsync();

                if (_serverTask != null)
                    await _serverTask;

                _serverCts?.Dispose();
                _serverCts = null;
                _serverTask = null;
                _isRunning = false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(StopAsync));

                _isRunning = true;
            }
        }
    }
}
