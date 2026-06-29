namespace Sirstrap.Core.Weao
{
    public sealed record WeaoExecutor(string Title, string? Version, bool UpdateStatus, bool Detected);

    public sealed record WeaoWindowsVersions(string? Current, string? Past, string? Future);

    public interface IWeaoService
    {
        Task<string?> GetCurrentWindowsVersionAsync(CancellationToken cancellationToken = default);

        Task<string?> GetExecutorVersionAsync(string title, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<WeaoExecutor>> GetExecutorsAsync(CancellationToken cancellationToken = default);

        Task<WeaoWindowsVersions> GetWindowsVersionsAsync(CancellationToken cancellationToken = default);
    }
}
