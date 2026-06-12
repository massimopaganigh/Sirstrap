namespace SirHurt.Cleaner.CLI.Abstractions
{
    public interface IUserProfileProvider
    {
        IEnumerable<string> GetOtherUserProfileDirectories();
    }
}
