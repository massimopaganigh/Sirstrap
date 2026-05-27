namespace Sirstrap.Core
{
    public interface ITelemetryScope : IDisposable
    {
        void MarkFailed();

        void SetTag(string key, string value);
    }
}
