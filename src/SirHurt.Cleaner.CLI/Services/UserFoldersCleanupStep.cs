namespace SirHurt.Cleaner.CLI.Services
{
    /// <summary>
    /// Removes application folders from the current user profile and from all other user profiles.
    /// </summary>
    public sealed class UserFoldersCleanupStep(
        ILogger logger,
        ISelectiveFolderCleaner selectiveFolderCleaner,
        IUserProfileProvider userProfileProvider,
        CleanerConfig config) : ICleanupStep
    {
        public string Name => "Clean user folders";

        public void Execute()
        {
            logger.Information("[*] Cleaning application folders for current user: {Username}", Environment.UserName);
            CleanProfileFolders(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            logger.Information("[*] Scanning other user profiles for application folders");

            foreach (var userProfile in userProfileProvider.GetOtherUserProfileDirectories())
            {
                logger.Information("[*] Cleaning application folders for user: {Username}", Path.GetFileName(userProfile));

                try
                {
                    CleanProfileFolders(userProfile);
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.Warning(ex, "[!] Access denied to user profile: {ProfilePath}", userProfile);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "[!] Unexpected error while cleaning user profile: {ProfilePath}", userProfile);
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
                    logger.Error(ex, "[!] Unexpected error while cleaning folder {UserFolder} in profile {ProfilePath}", userFolder, profilePath);
                }
            }
        }
    }
}
