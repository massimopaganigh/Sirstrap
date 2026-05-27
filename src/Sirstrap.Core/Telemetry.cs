namespace Sirstrap.Core
{
    public static class Telemetry
    {
        private static IPerformanceTelemetry _performance = new SentryPerformanceTelemetry();
        private static ICdnTelemetry _cdn = new SentryCdnTelemetry();

        public static IPerformanceTelemetry Performance
        {
            get => _performance;
            set => _performance = value ?? NullPerformanceTelemetry.Instance;
        }

        public static ICdnTelemetry Cdn
        {
            get => _cdn;
            set => _cdn = value ?? NullCdnTelemetry.Instance;
        }
    }
}
