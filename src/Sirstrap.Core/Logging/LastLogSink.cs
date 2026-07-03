namespace Sirstrap.Core.Logging
{
    public sealed class LastLogSink : ILastLogSink
    {
        private readonly object _gate = new();
        private readonly List<string> _sessionLog = [];

        public string LastLog { get; private set; } = string.Empty;

        public LogEventLevel? LastLogLevel { get; private set; }

        public DateTimeOffset? LastLogTimestamp { get; private set; }

        public int SessionLogCount
        {
            get
            {
                lock (_gate)
                    return _sessionLog.Count;
            }
        }

        public IReadOnlyList<string> GetSessionLog()
        {
            lock (_gate)
                return [.. _sessionLog];
        }

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage();

            LastLog = message;
            LastLogTimestamp = logEvent.Timestamp;
            LastLogLevel = logEvent.Level;

            var entry = $"[{logEvent.Timestamp:HH:mm:ss} {GetLevelMoniker(logEvent.Level)}] {message}";

            if (logEvent.Exception != null)
                entry = $"{entry}{Environment.NewLine}{logEvent.Exception}";

            lock (_gate)
                _sessionLog.Add(entry);
        }

        private static string GetLevelMoniker(LogEventLevel level) => level switch
        {
            LogEventLevel.Verbose => "VRB",
            LogEventLevel.Debug => "DBG",
            LogEventLevel.Information => "INF",
            LogEventLevel.Warning => "WRN",
            LogEventLevel.Error => "ERR",
            LogEventLevel.Fatal => "FTL",
            _ => level.ToString().ToUpperInvariant()
        };
    }
}
