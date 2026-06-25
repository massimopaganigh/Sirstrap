namespace Sirstrap.Core.Logging
{
    public interface ILastLogSink : ILogEventSink
    {
        string LastLog { get; }

        LogEventLevel? LastLogLevel { get; }

        DateTimeOffset? LastLogTimestamp { get; }
    }
}
