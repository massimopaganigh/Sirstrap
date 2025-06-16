namespace Sirstrap.Core
{
    public class LastLogSink : ILogEventSink
    {
        public static string LastLog { get; private set; } = string.Empty;

        public static DateTimeOffset? LastLogTimestamp { get; private set; }

        public static LogEventLevel? LastLogLevel { get; private set; }

        public void Emit(LogEvent logEvent)
        {
            LastLog = logEvent.RenderMessage();
            LastLogTimestamp = logEvent.Timestamp;
            LastLogLevel = logEvent.Level;
        }
    }
}