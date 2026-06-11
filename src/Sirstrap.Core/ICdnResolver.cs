namespace Sirstrap.Core
{
    public interface ICdnResolver
    {
        Task<string> ResolveAsync(Configuration configuration, CancellationToken cancellationToken = default);
    }
}
