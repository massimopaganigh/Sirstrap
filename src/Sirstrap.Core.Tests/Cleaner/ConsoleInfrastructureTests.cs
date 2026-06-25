using Serilog;

namespace Sirstrap.Core.Tests.Cleaner
{
    public class ConsoleInfrastructureTests
    {
        [Fact]
        public void ConsoleStatusLine_SetStatusClearAndHidden_DoNotThrow()
        {
            ConsoleStatusLine statusLine = new();
            int actionRuns = 0;

            var exception = Record.Exception(() =>
            {
                statusLine.SetStatus("working");
                statusLine.InvokeWithStatusHidden(() => actionRuns++);
                statusLine.Clear();
            });

            Assert.Null(exception);
            Assert.Equal(1, actionRuns);
        }

        [Theory]
        [InlineData("y", false, true)]
        [InlineData("yes", false, true)]
        [InlineData("n", true, false)]
        [InlineData("", true, true)]
        [InlineData("", false, false)]
        public void ConsoleUserInteraction_Confirm_HonoursResponseAndDefault(string response, bool defaultAnswer, bool expected)
        {
            TextReader originalIn = Console.In;
            TextWriter originalOut = Console.Out;

            try
            {
                Console.SetIn(new StringReader(response + Environment.NewLine));
                Console.SetOut(new StringWriter());

                ConsoleUserInteraction interaction = new(new FakeStatusLine());

                Assert.Equal(expected, interaction.Confirm("Proceed?", defaultAnswer));
            }
            finally
            {
                Console.SetIn(originalIn);
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void StatusLinePreservingSink_ForwardsEvents_ThroughHiddenStatus()
        {
            RecordingLogSink inner = new();
            FakeStatusLine statusLine = new();
            StatusLinePreservingSink sink = new(inner, statusLine);

            ILogger logger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
            logger.Information("preserved");

            Assert.Contains("preserved", inner.Messages);
            Assert.True(statusLine.HiddenInvocations > 0);

            Assert.Null(Record.Exception(sink.Dispose));
        }
    }
}
