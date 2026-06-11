namespace Sirstrap.Core
{
    public sealed class NullPerformanceTelemetry : IPerformanceTelemetry
    {
        public static NullPerformanceTelemetry Instance { get; } = new();

        private NullPerformanceTelemetry() { }

        public void RecordCounter(string name, IReadOnlyDictionary<string, object>? tags = null) { }

        public void RecordDuration(string operation, TimeSpan elapsed, IReadOnlyDictionary<string, object>? tags = null) { }

        public ITelemetryScope Measure(string operation, IReadOnlyDictionary<string, object>? tags = null) => NoopScope.Shared;

        private sealed class NoopScope : ITelemetryScope
        {
            public static NoopScope Shared { get; } = new();

            private NoopScope() { }

            public void Dispose() { }

            public void MarkFailed() { }

            public void SetTag(string key, string value) { }
        }
    }
}
