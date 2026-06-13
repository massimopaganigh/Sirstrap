namespace Sirstrap.Core.Tests.Telemetry
{
    public class PerformanceTelemetryTests
    {
        [Fact]
        public void NullPerformanceTelemetry_IsSingleton_AndScopeIsUsable()
        {
            Assert.Same(NullPerformanceTelemetry.Instance, NullPerformanceTelemetry.Instance);

            var exception = Record.Exception(() =>
            {
                NullPerformanceTelemetry.Instance.RecordCounter("counter");
                NullPerformanceTelemetry.Instance.RecordDuration("op", TimeSpan.FromSeconds(1));

                using ITelemetryScope scope = NullPerformanceTelemetry.Instance.Measure("op");
                scope.SetTag("k", "v");
                scope.MarkFailed();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void SentryPerformanceTelemetry_DoesNotThrow_WhenSentryNotInitialized()
        {
            SentryPerformanceTelemetry telemetry = new();

            var exception = Record.Exception(() =>
            {
                telemetry.RecordCounter("counter", new Dictionary<string, object> { ["a"] = 1 });
                telemetry.RecordDuration("op", TimeSpan.FromMilliseconds(5), new Dictionary<string, object> { ["a"] = 1 });

                using ITelemetryScope scope = telemetry.Measure("op", new Dictionary<string, object> { ["a"] = 1 });
                scope.SetTag("k", "v");
                scope.MarkFailed();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void SentryPerformanceTelemetry_Measure_ReturnsScope_WithoutTags()
        {
            SentryPerformanceTelemetry telemetry = new();

            using ITelemetryScope scope = telemetry.Measure("op");

            Assert.NotNull(scope);
        }
    }
}
