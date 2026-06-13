namespace Sirstrap.Core.Tests.Cdn
{
    public class CdnTelemetryTests
    {
        [Fact]
        public void NullCdnTelemetry_IsSingleton_AndDoesNotThrow()
        {
            Assert.Same(NullCdnTelemetry.Instance, NullCdnTelemetry.Instance);

            var exception = Record.Exception(() =>
            {
                NullCdnTelemetry.Instance.RecordResolved("https://a", CdnResolutionSource.Probe);
                NullCdnTelemetry.Instance.RecordProbe("https://a", true, TimeSpan.Zero);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void SentryCdnTelemetry_DoesNotThrow_WhenSentryNotInitialized()
        {
            SentryCdnTelemetry telemetry = new();

            var exception = Record.Exception(() =>
            {
                telemetry.RecordResolved("https://a.example.com", CdnResolutionSource.Fallback);
                telemetry.RecordProbe("https://a.example.com", success: false, TimeSpan.FromMilliseconds(10));
            });

            Assert.Null(exception);
        }
    }
}
