namespace Sirstrap.Core.Cleaner
{
    public sealed class GuiStatusLine : IStatusLine
    {
        public void Clear() { }

        public void InvokeWithStatusHidden(Action action) => action();

        public void SetStatus(string status)
        {
            Log.Information("[Cleaner] {Status}", status);
        }
    }
}
