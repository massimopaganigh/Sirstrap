namespace SirHurt.Cleaner.CLI.Abstractions
{
    public interface IUserInteraction
    {
        bool Confirm(string message, bool defaultAnswer = false);
    }
}
