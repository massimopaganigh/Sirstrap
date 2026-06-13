namespace Sirstrap.Core.Telemetry
{
    public interface ITelemetryScope : IDisposable
    {
        void MarkFailed();

        void SetTag(string key, string value);
    }
}
