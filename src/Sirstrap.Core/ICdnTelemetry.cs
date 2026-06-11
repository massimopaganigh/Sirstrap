namespace Sirstrap.Core
{
    public interface ICdnTelemetry
    {
        void RecordResolved(string baseUri, CdnResolutionSource source);

        void RecordProbe(string baseUri, bool success, TimeSpan elapsed);
    }
}
