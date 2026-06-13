namespace Sirstrap.Core.Cdn
{
    public sealed class NullCdnTelemetry : ICdnTelemetry
    {
        public static NullCdnTelemetry Instance { get; } = new();

        private NullCdnTelemetry() { }

        public void RecordResolved(string baseUri, CdnResolutionSource source) { }

        public void RecordProbe(string baseUri, bool success, TimeSpan elapsed) { }
    }
}
