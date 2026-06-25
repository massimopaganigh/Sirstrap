using Serilog;
using Serilog.Events;

namespace Sirstrap.Core.Tests.Logging
{
    public class LastLogSinkTests
    {
        [Fact]
        public void Emit_CapturesRenderedMessageLevelAndTimestamp()
        {
            LastLogSink sink = new();

            ILogger logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Sink(sink)
                .CreateLogger();

            logger.Warning("hello {Name}", "world");

            Assert.Equal("hello \"world\"", sink.LastLog);
            Assert.Equal(LogEventLevel.Warning, sink.LastLogLevel);
            Assert.NotNull(sink.LastLogTimestamp);
        }

        [Fact]
        public void Defaults_AreEmptyBeforeAnyEmit()
        {
            LastLogSink sink = new();

            Assert.Equal(string.Empty, sink.LastLog);
            Assert.Null(sink.LastLogLevel);
            Assert.Null(sink.LastLogTimestamp);
        }

        [Fact]
        public void LastLogExtension_RegistersSink_AndReceivesEvents()
        {
            LastLogSink sink = new();

            ILogger logger = new LoggerConfiguration()
                .WriteTo.LastLog(sink)
                .CreateLogger();

            logger.Information("captured");

            Assert.Equal("captured", sink.LastLog);
        }
    }
}
