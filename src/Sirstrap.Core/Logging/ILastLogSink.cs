namespace Sirstrap.Core.Logging
{
    public interface ILastLogSink : ILogEventSink
    {
        string LastLog { get; }

        LogEventLevel? LastLogLevel { get; }

        DateTimeOffset? LastLogTimestamp { get; }

        int SessionLogCount { get; }

        IReadOnlyList<string> GetSessionLog();
    }
}
