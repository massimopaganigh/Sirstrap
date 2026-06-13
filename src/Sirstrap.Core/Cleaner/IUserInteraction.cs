namespace Sirstrap.Core.Cleaner
{
    public interface IUserInteraction
    {
        bool Confirm(string message, bool defaultAnswer = false);
    }
}
