namespace Sirstrap.Core.Weao
{
    public sealed class WeaoService : IWeaoService, IDisposable
    {
        private readonly WeaoClient _client = new();

        public void Dispose() => _client.Dispose();

        public async Task<string?> GetCurrentWindowsVersionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var versions = await _client.GetCurrentVersionsAsync(cancellationToken).ConfigureAwait(false);

                return string.IsNullOrWhiteSpace(versions.Windows) ? null : versions.Windows;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to retrieve the current Roblox version from WEAO.");

                return null;
            }
        }

        public async Task<string?> GetExecutorVersionAsync(string title, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;

            try
            {
                var exploit = await _client.GetExploitAsync(title, cancellationToken).ConfigureAwait(false);

                return ResolveExploitVersion(exploit);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to retrieve the version supported by the executor {Executor} from WEAO.", title);

                return null;
            }
        }

        public async Task<IReadOnlyList<WeaoExecutor>> GetExecutorsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var exploits = await _client.GetExploitsAsync(cancellationToken).ConfigureAwait(false);

                return
                [
                    .. exploits
                        .Where(exploit => !exploit.Hidden && !string.IsNullOrWhiteSpace(exploit.Title) && IsWindows(exploit.Platform))
                        .Select(exploit => new WeaoExecutor(exploit.Title!, ResolveExploitVersion(exploit), exploit.UpdateStatus, exploit.Detected))
                        .OrderBy(executor => executor.Title, StringComparer.OrdinalIgnoreCase)
                ];
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to retrieve the executor list from WEAO.");

                return [];
            }
        }

        public async Task<WeaoWindowsVersions> GetWindowsVersionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var current = _client.GetCurrentVersionsAsync(cancellationToken);
                var past = _client.GetPastVersionsAsync(cancellationToken);
                var future = _client.GetFutureVersionsAsync(cancellationToken);

                await Task.WhenAll(current, past, future).ConfigureAwait(false);

                return new WeaoWindowsVersions(
                    NullIfBlank(current.Result.Windows),
                    NullIfBlank(past.Result.Windows),
                    NullIfBlank(future.Result.Windows));
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to retrieve the Roblox Windows versions from WEAO.");

                return new WeaoWindowsVersions(null, null, null);
            }
        }

        private static bool IsWindows(string? platform) => string.IsNullOrWhiteSpace(platform) || platform.Equals("windows", StringComparison.OrdinalIgnoreCase);

        private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

        private static string? ResolveExploitVersion(ExploitStatus exploit) => NullIfBlank(exploit.Version) ?? NullIfBlank(exploit.RbxVersion);
    }
}
