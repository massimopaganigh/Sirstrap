using Serilog.Core;
using Serilog.Events;

namespace Sirstrap.Core
{
    /// <summary>
    /// A custom Serilog sink that stores the last log message emitted by the application.
    /// </summary>
    public class LastLogSink : ILogEventSink
    {
        /// <summary>
        /// Gets the last log message emitted by the application.
        /// </summary>
        /// <value>
        /// A string containing the most recent log message, or an empty string if no logs have been emitted.
        /// </value>
        public static string LastLog { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the timestamp of the last log message.
        /// </summary>
        /// <value>
        /// The timestamp when the last log was recorded, or null if no logs have been emitted.
        /// </value>
        public static DateTimeOffset? LastLogTimestamp { get; private set; }

        /// <summary>
        /// Gets the level of the last log message.
        /// </summary>
        /// <value>
        /// The log level of the most recent message, or null if no logs have been emitted.
        /// </value>
        public static LogEventLevel? LastLogLevel { get; private set; }

        /// <summary>
        /// Processes a log event by storing the rendered message and its metadata.
        /// </summary>
        /// <param name="logEvent">The log event to process.</param>
        public void Emit(LogEvent logEvent)
        {
            LastLog = logEvent.RenderMessage();
            LastLogTimestamp = logEvent.Timestamp;
            LastLogLevel = logEvent.Level;
        }
    }
}