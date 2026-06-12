namespace SirHurt.Cleaner.CLI.Abstractions
{
    public interface ICleanupStep
    {
        void Execute();
        string Name { get; }
    }
}
