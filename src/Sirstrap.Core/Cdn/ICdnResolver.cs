namespace Sirstrap.Core.Cdn
{
    public interface ICdnResolver
    {
        Task<string> ResolveAsync(Configuration configuration, CancellationToken cancellationToken = default);
    }
}
