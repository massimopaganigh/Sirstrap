namespace Sirstrap.Core.Tests.Support
{
    public sealed class FakeWeaoService : IWeaoService
    {
        public string? CurrentWindowsVersion { get; set; }

        public string? ExecutorVersion { get; set; }

        public IReadOnlyList<WeaoExecutor> Executors { get; set; } = [];

        public WeaoWindowsVersions WindowsVersions { get; set; } = new(null, null, null);

        public Task<string?> GetCurrentWindowsVersionAsync(CancellationToken cancellationToken = default) => Task.FromResult(CurrentWindowsVersion);

        public Task<string?> GetExecutorVersionAsync(string title, CancellationToken cancellationToken = default) => Task.FromResult(ExecutorVersion);

        public Task<IReadOnlyList<WeaoExecutor>> GetExecutorsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Executors);

        public Task<WeaoWindowsVersions> GetWindowsVersionsAsync(CancellationToken cancellationToken = default) => Task.FromResult(WindowsVersions);
    }
}
