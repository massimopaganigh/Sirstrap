namespace Sirstrap.Core.Tests.Cleaner
{
    public class UserProfileProviderTests
    {
        [Fact]
        public void GetOtherUserProfileDirectories_ReturnsEmpty_WhenUsersFolderMissing()
        {
            FakeFileSystem fs = new();
            UserProfileProvider provider = new(fs);

            Assert.Empty(provider.GetOtherUserProfileDirectories());
        }

        [Fact]
        public void GetOtherUserProfileDirectories_ExcludesCurrentUserAndSpecialProfiles()
        {
            string systemDrive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
            string usersFolder = Path.Combine(systemDrive + Path.DirectorySeparatorChar, "Users");
            string currentUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            FakeFileSystem fs = new();
            fs.AddDirectory(usersFolder);
            fs.AddDirectory(currentUser);
            fs.AddDirectory(Path.Combine(usersFolder, "Public"));
            fs.AddDirectory(Path.Combine(usersFolder, "Default"));
            fs.AddDirectory(Path.Combine(usersFolder, "Alice"));
            fs.AddDirectory(Path.Combine(usersFolder, "Bob"));

            UserProfileProvider provider = new(fs);

            var profiles = provider.GetOtherUserProfileDirectories().ToList();

            Assert.Contains(Path.Combine(usersFolder, "Alice"), profiles);
            Assert.Contains(Path.Combine(usersFolder, "Bob"), profiles);
            Assert.DoesNotContain(Path.Combine(usersFolder, "Public"), profiles);
            Assert.DoesNotContain(Path.Combine(usersFolder, "Default"), profiles);
            Assert.DoesNotContain(currentUser, profiles);
        }
    }
}
