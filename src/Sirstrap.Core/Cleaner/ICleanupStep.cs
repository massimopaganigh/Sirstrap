namespace Sirstrap.Core.Cleaner
{
    public interface ICleanupStep
    {
        void Execute();

        string Name { get; }
    }
}
