namespace Sirstrap.Core.Cleaner.Steps
{
    public sealed class UserFoldersCleanupStep(
        ISelectiveFolderCleaner selectiveFolderCleaner,
        IUserProfileProvider userProfileProvider,
        CleanerConfig config) : ICleanupStep
    {
        public string Name => "Clean user folders";

        public void Execute()
        {
            Log.Information("[*] Cleaning the application folders for the current user {Username}...", Environment.UserName);

            CleanProfileFolders(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            Log.Information("[*] Scanning the other user profiles for application folders...");

            foreach (var userProfile in userProfileProvider.GetOtherUserProfileDirectories())
            {
                Log.Information("[*] Cleaning the application folders for the user {Username}...", Path.GetFileName(userProfile));

                try
                {
                    CleanProfileFolders(userProfile);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Warning(ex, "[!] The access to the user profile {ProfilePath} was denied.", userProfile);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Failed to clean the user profile {ProfilePath}.", userProfile);
                }
            }
        }

        private void CleanProfileFolders(string profilePath)
        {
            foreach (var userFolder in config.UserFolders)
            {
                try
                {
                    selectiveFolderCleaner.CleanFolderContents(Path.GetFullPath(Path.Combine(profilePath, userFolder)));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Failed to clean the folder {UserFolder} in the profile {ProfilePath}.", userFolder, profilePath);
                }
            }
        }
    }
}
