namespace Sirstrap.Core.Logging
{
    public sealed class LastLogSink : ILastLogSink
    {
        public string LastLog { get; private set; } = string.Empty;

        public LogEventLevel? LastLogLevel { get; private set; }

        public DateTimeOffset? LastLogTimestamp { get; private set; }

        public void Emit(LogEvent logEvent)
        {
            LastLog = logEvent.RenderMessage();
            LastLogTimestamp = logEvent.Timestamp;
            LastLogLevel = logEvent.Level;
        }
    }
}
