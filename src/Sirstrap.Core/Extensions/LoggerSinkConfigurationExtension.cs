namespace Sirstrap.Core.Extensions
{
    public static class LoggerSinkConfigurationExtension
    {
        public static LoggerConfiguration LastLog(this LoggerSinkConfiguration loggerSinkConfiguration) => loggerSinkConfiguration.Sink(new LastLogSink());
    }
}
