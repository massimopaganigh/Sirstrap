namespace Sirstrap.Core
{
    public interface ICdnProbeUriFactory
    {
        string Create(Configuration configuration, string baseUri);
    }
}
