namespace SirHurt.Cleaner.CLI.Abstractions
{
    public interface IRegistryCleaner
    {
        void CleanAllUsers(IEnumerable<string> keyPaths);
        void CleanCurrentUser(IEnumerable<string> keyPaths);
        void CleanLocalMachine(IEnumerable<string> keyPaths);
    }
}
