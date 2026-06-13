namespace Sirstrap.Core.Cdn
{
    public interface ICdnUriNormalizer
    {
        string Normalize(string? cdnUriOverride);
    }
}
