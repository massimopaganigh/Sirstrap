namespace Sirstrap.Core.Tests.Support
{
    public sealed class RecordingPerformanceTelemetry : IPerformanceTelemetry
    {
        public List<(string Name, IReadOnlyDictionary<string, object>? Tags)> Counters { get; } = [];

        public List<(string Operation, TimeSpan Elapsed, IReadOnlyDictionary<string, object>? Tags)> Durations { get; } = [];

        public List<RecordingScope> Scopes { get; } = [];

        public void RecordCounter(string name, IReadOnlyDictionary<string, object>? tags = null) => Counters.Add((name, tags));

        public void RecordDuration(string operation, TimeSpan elapsed, IReadOnlyDictionary<string, object>? tags = null) => Durations.Add((operation, elapsed, tags));

        public ITelemetryScope Measure(string operation, IReadOnlyDictionary<string, object>? tags = null)
        {
            RecordingScope scope = new(operation, tags);

            Scopes.Add(scope);

            return scope;
        }

        public sealed class RecordingScope(string operation, IReadOnlyDictionary<string, object>? tags) : ITelemetryScope
        {
            public string Operation { get; } = operation;

            public IReadOnlyDictionary<string, object>? Tags { get; } = tags;

            public Dictionary<string, string> SetTags { get; } = [];

            public bool Failed { get; private set; }

            public bool Disposed { get; private set; }

            public void Dispose() => Disposed = true;

            public void MarkFailed() => Failed = true;

            public void SetTag(string key, string value) => SetTags[key] = value;
        }
    }
}
