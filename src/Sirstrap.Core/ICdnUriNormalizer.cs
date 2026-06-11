namespace Sirstrap.Core
{
    public interface ICdnUriNormalizer
    {
        string Normalize(string? cdnUriOverride);
    }
}
