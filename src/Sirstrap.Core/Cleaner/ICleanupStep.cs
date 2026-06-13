namespace Sirstrap.Core.Cleaner
{
    public interface ICleanupStep
    {
        string Name { get; }

        void Execute();
    }
}
