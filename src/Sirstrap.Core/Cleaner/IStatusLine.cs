namespace Sirstrap.Core.Cleaner
{
    public interface IStatusLine
    {
        void Clear();

        void InvokeWithStatusHidden(Action action);

        void SetStatus(string status);
    }
}
