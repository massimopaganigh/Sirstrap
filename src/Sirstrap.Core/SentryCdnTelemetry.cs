namespace Sirstrap.Core
{
    public sealed class SentryCdnTelemetry : ICdnTelemetry
    {
        public void RecordResolved(string baseUri, CdnResolutionSource source)
        {
            try
            {
                SentrySdk.Metrics.EmitCounter("cdn.resolved", 1, new Dictionary<string, object>
                {
                    ["baseUri"] = baseUri,
                    ["source"] = source.ToString()
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[*] Failed to emit CDN resolution telemetry for {0}.", baseUri);
            }
        }

        public void RecordProbe(string baseUri, bool success, TimeSpan elapsed)
        {
            try
            {
                SentrySdk.Metrics.EmitCounter("cdn.probe", 1, new Dictionary<string, object>
                {
                    ["baseUri"] = baseUri,
                    ["success"] = success,
                    ["elapsed_ms"] = (long)elapsed.TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[*] Failed to emit CDN probe telemetry for {0}.", baseUri);
            }
        }
    }
}
