namespace SirHurt.Cleaner.CLI.Infrastructure
{
    public sealed class WindowsUserProfileProvider(IFileSystem fileSystem) : IUserProfileProvider
    {
        private static readonly string[] SpecialProfileNames = ["Public", "Default", "Default User", "All Users"];

        public IEnumerable<string> GetOtherUserProfileDirectories()
        {
            string systemDrive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
            string usersFolder = Path.Combine(systemDrive + Path.DirectorySeparatorChar, "Users");
            if (!fileSystem.DirectoryExists(usersFolder)) return [];
            string currentUserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return fileSystem.GetDirectories(usersFolder).Where(dir => !dir.Equals(currentUserProfile, StringComparison.OrdinalIgnoreCase) && !SpecialProfileNames.Contains(Path.GetFileName(dir), StringComparer.OrdinalIgnoreCase));
        }
    }
}
