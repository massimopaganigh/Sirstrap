namespace Sirstrap.Core
{
    public static class LastLogSinkExtensions
    {
        public static LoggerConfiguration LastLog(this LoggerSinkConfiguration loggerConfiguration) => loggerConfiguration.Sink(new LastLogSink());
    }
}
