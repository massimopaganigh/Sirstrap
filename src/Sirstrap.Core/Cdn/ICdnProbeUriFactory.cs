namespace Sirstrap.Core.Cdn
{
    public interface ICdnProbeUriFactory
    {
        string Create(Configuration configuration, string baseUri);
    }
}
