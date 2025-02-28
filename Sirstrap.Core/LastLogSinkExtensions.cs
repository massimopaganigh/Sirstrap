using Serilog;
using Serilog.Configuration;

namespace Sirstrap.Core
{
    /// <summary>
    /// Extension methods for adding the LastLogSink to a LoggerConfiguration.
    /// </summary>
    public static class LastLogSinkExtensions
    {
        /// <summary>
        /// Adds a sink that stores the last log message in the LastLogSink.LastLog property.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        public static LoggerConfiguration LastLog(this LoggerSinkConfiguration loggerConfiguration)
        {
            return loggerConfiguration.Sink(new LastLogSink());
        }
    }
}