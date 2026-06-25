namespace Sirstrap.Core.Cdn
{
    public interface ICdnTelemetry
    {
        void RecordProbe(string baseUri, bool success, TimeSpan elapsed);

        void RecordResolved(string baseUri, CdnResolutionSource source);
    }
}
