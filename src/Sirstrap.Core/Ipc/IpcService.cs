namespace Sirstrap.Core.Ipc
{
    public sealed class IpcService : IIpcService
    {
        private bool _isRunning;
        private CancellationTokenSource? _serverCts;
        private Task? _serverTask;

        public event EventHandler<string>? MessageReceived;

        private async Task HandleClientAsync(NamedPipeServerStream pipeServer)
        {
            try
            {
                Log.Debug("[*] Handling an IPC client connection...");

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

                    Log.Debug("[*] Received the IPC message {Message}.", message);

                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to handle an IPC client connection.");
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
                    Log.Debug("[*] Listening for IPC connections on the pipe {PipeName}...", pipeName);

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
                    Log.Error(ex, "[!] Failed to listen for IPC connections on the pipe {PipeName}, retrying in 5 seconds...", pipeName);

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
                Log.Debug("[*] Sending the IPC message {Message} to the pipe {PipeName}...", message, pipeName);

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
                Log.Error(ex, "[!] Failed to send the IPC message to the pipe {PipeName}.", pipeName);

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
                Log.Debug("[*] Starting the IPC server on the pipe {PipeName}...", pipeName);

                if (_isRunning)
                    return;

                _serverCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _isRunning = true;
                _serverTask = Task.Run(async () => await ListenForConnectionsAsync(pipeName, _serverCts.Token), _serverCts.Token);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to start the IPC server on the pipe {PipeName}.", pipeName);

                _isRunning = false;
            }
        }

        public async Task StopAsync()
        {
            try
            {
                Log.Debug("[*] Stopping the IPC server...");

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
                Log.Error(ex, "[!] Failed to stop the IPC server.");

                _isRunning = true;
            }
        }
    }
}
