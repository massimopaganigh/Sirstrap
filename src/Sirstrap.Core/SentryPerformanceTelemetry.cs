namespace Sirstrap.Core
{
    public sealed class SentryPerformanceTelemetry : IPerformanceTelemetry
    {
        public void RecordCounter(string name, IReadOnlyDictionary<string, object>? tags = null)
        {
            try
            {
                SentrySdk.Metrics.EmitCounter(name, 1, BuildTags(tags));
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[*] Failed to emit counter {0}.", name);
            }
        }

        public void RecordDuration(string operation, TimeSpan elapsed, IReadOnlyDictionary<string, object>? tags = null)
        {
            try
            {
                Dictionary<string, object> enriched = BuildTags(tags);
                enriched["elapsed_ms"] = (long)elapsed.TotalMilliseconds;

                SentrySdk.Metrics.EmitCounter($"{operation}.duration", 1, enriched);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[*] Failed to emit duration for {0}.", operation);
            }
        }

        public ITelemetryScope Measure(string operation, IReadOnlyDictionary<string, object>? tags = null)
        {
            try
            {
                ISpan? current = SentrySdk.GetSpan();

                if (current != null)
                {
                    ISpan child = current.StartChild(operation);

                    ApplyTags(child, tags);

                    return new SentrySpanScope(child);
                }

                ITransactionTracer transaction = SentrySdk.StartTransaction(operation, "task");

                ApplyTags(transaction, tags);

                SentrySdk.ConfigureScope(scope => scope.Transaction = transaction);

                return new SentrySpanScope(transaction);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[*] Failed to start telemetry scope for {0}.", operation);

                return NullPerformanceTelemetry.Instance.Measure(operation, tags);
            }
        }

        private static void ApplyTags(ISpan span, IReadOnlyDictionary<string, object>? tags)
        {
            if (tags == null)
                return;

            foreach (var kvp in tags)
                span.SetTag(kvp.Key, kvp.Value?.ToString() ?? string.Empty);
        }

        private static Dictionary<string, object> BuildTags(IReadOnlyDictionary<string, object>? tags)
            => tags == null ? [] : new Dictionary<string, object>(tags);

        private sealed class SentrySpanScope : ITelemetryScope
        {
            private readonly ISpan _span;
            private bool _failed;
            private bool _disposed;

            public SentrySpanScope(ISpan span) => _span = span;

            public void MarkFailed() => _failed = true;

            public void SetTag(string key, string value) => _span.SetTag(key, value);

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                try
                {
                    _span.Finish(_failed ? SpanStatus.InternalError : SpanStatus.Ok);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "[*] Failed to finish telemetry span.");
                }
            }
        }

    }
}
