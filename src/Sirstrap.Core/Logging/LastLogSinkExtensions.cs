namespace Sirstrap.Core.Logging
{
    public static class LastLogSinkExtensions
    {
        public static LoggerConfiguration LastLog(this LoggerSinkConfiguration loggerConfiguration, ILastLogSink sink) => loggerConfiguration.Sink(sink);
    }
}
