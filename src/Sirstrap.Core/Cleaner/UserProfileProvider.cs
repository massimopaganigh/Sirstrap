namespace Sirstrap.Core.Cleaner
{
    public sealed class UserProfileProvider(IFileSystem fileSystem) : IUserProfileProvider
    {
        private static readonly string[] _specialProfileNames = ["Public", "Default", "Default User", "All Users"];

        public IEnumerable<string> GetOtherUserProfileDirectories()
        {
            string systemDrive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
            string usersFolder = Path.Combine(systemDrive + Path.DirectorySeparatorChar, "Users");

            if (!fileSystem.DirectoryExists(usersFolder))
                return [];

            string currentUserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return fileSystem.GetDirectories(usersFolder)
                .Where(dir => !dir.Equals(currentUserProfile, StringComparison.OrdinalIgnoreCase)
                    && !_specialProfileNames.Contains(Path.GetFileName(dir), StringComparer.OrdinalIgnoreCase));
        }
    }
}
