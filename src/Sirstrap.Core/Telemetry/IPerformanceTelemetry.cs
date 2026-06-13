namespace Sirstrap.Core.Telemetry
{
    public interface IPerformanceTelemetry
    {
        void RecordCounter(string name, IReadOnlyDictionary<string, object>? tags = null);

        void RecordDuration(string operation, TimeSpan elapsed, IReadOnlyDictionary<string, object>? tags = null);

        ITelemetryScope Measure(string operation, IReadOnlyDictionary<string, object>? tags = null);
    }
}
