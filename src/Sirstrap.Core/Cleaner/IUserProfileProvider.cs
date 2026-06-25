namespace Sirstrap.Core.Cleaner
{
    public interface IUserProfileProvider
    {
        IEnumerable<string> GetOtherUserProfileDirectories();
    }
}
