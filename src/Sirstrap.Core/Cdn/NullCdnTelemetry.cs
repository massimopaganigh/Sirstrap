namespace Sirstrap.Core.Cdn
{
    public sealed class NullCdnTelemetry : ICdnTelemetry
    {
        private NullCdnTelemetry() { }

        public void RecordProbe(string baseUri, bool success, TimeSpan elapsed) { }

        public void RecordResolved(string baseUri, CdnResolutionSource source) { }

        public static NullCdnTelemetry Instance { get; } = new();
    }
}
