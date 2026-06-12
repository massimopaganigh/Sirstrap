namespace SirHurt.Cleaner.CLI.Infrastructure
{
    public sealed class StatusLinePreservingSink(ILogEventSink innerSink, IStatusLine statusLine) : ILogEventSink, IDisposable
    {
        public void Dispose() => (innerSink as IDisposable)?.Dispose();

        public void Emit(LogEvent logEvent) => statusLine.InvokeWithStatusHidden(() => innerSink.Emit(logEvent));
    }
}
