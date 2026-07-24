namespace Sirstrap.Core.Cleaner
{
    public sealed class GuiUserInteraction(CleanerConfig config) : IUserInteraction
    {
        public bool Confirm(string message, bool defaultAnswer = false)
        {
            return config.CleanProtectedFiles;
        }
    }
}
