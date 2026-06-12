namespace SirHurt.Cleaner.CLI.Abstractions
{
    public interface IStatusLine
    {
        void Clear();
        void InvokeWithStatusHidden(Action action);
        void SetStatus(string status);
    }
}
