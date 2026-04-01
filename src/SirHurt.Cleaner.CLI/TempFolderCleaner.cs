using Serilog;

namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Manages operations for cleaning temporary files and folders
    /// </summary>
    public class TempFolderCleaner
    {
        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;
        private readonly CleanerCore _cleanerCore;

        public TempFolderCleaner(
            ILogger logger,
            IFileSystem fileSystem,
            CleanerCore cleanerCore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _cleanerCore = cleanerCore ?? throw new ArgumentNullException(nameof(cleanerCore));
        }

        /// <summary>
        /// Cleans all temporary folders for the current user
        /// </summary>
        public void CleanCurrentUserTempFolders()
        {
            // Get the main temp folder path from environment
            string tempPath = Path.GetTempPath();
            _logger.Information("Cleaning temporary folder: {TempPath}", tempPath);

            try
            {
                if (!_fileSystem.DirectoryExists(tempPath))
                {
                    _logger.Warning("Temporary folder not found at {TempPath}", tempPath);
                    return;
                }

                // First, try to delete all files directly in the temp folder
                DeleteFilesInFolder(tempPath);

                // Then handle all directories in temp folder
                foreach (var directory in _fileSystem.GetDirectories(tempPath))
                {
                    _cleanerCore.DeleteFolder(directory);
                }

                // Get the number of remaining files
                var remainingFiles = _fileSystem.GetFiles(tempPath).Count();
                var remainingDirs = _fileSystem.GetDirectories(tempPath).Count();

                _logger.Information("Temporary folder cleanup completed. {RemainingFiles} files and {RemainingDirs} directories could not be removed",
                    remainingFiles, remainingDirs);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error cleaning temporary folder {TempPath}", tempPath);
            }
        }

        /// <summary>
        /// Cleans temporary folders for all users on the system
        /// </summary>
        public void CleanAllUsersTempFolders()
        {
            try
            {
                _logger.Information("Cleaning temporary folders for all users");

                string usersFolder = Path.Combine(Environment.GetEnvironmentVariable("SystemDrive") ?? "C:", "Users");

                if (!_fileSystem.DirectoryExists(usersFolder))
                {
                    _logger.Warning("Users folder not found at {UsersFolder}", usersFolder);
                    return;
                }

                var userProfiles = _fileSystem.GetDirectories(usersFolder)
                    .Where(dir =>
                        !dir.EndsWith("Public", StringComparison.OrdinalIgnoreCase) &&
                        !dir.EndsWith("Default", StringComparison.OrdinalIgnoreCase) &&
                        !dir.EndsWith("Default User", StringComparison.OrdinalIgnoreCase) &&
                        !dir.EndsWith("All Users", StringComparison.OrdinalIgnoreCase));

                _logger.Information("Found {Count} user profiles to check for temp folders", userProfiles.Count());

                foreach (var userProfile in userProfiles)
                {
                    string username = _fileSystem.GetFileName(userProfile);
                    _logger.Information("Checking temp folders for user: {Username}", username);

                    // AppData\Local\Temp - main temp folder
                    string tempPath = Path.Combine(userProfile, "AppData", "Local", "Temp");
                    if (_fileSystem.DirectoryExists(tempPath))
                    {
                        _logger.Information("Cleaning temp folder for user {Username}: {TempPath}", username, tempPath);
                        try
                        {
                            DeleteFilesInFolder(tempPath);

                            foreach (var directory in _fileSystem.GetDirectories(tempPath))
                            {
                                _cleanerCore.DeleteFolder(directory);
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            _logger.Warning("Access denied to user's temp folder: {TempPath}", tempPath);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Error cleaning user's temp folder: {TempPath}", tempPath);
                        }
                    }

                    // Other common temp locations for the user could be added here if needed
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to clean temporary folders for all users");
            }
        }

        /// <summary>
        /// Cleans system-wide temporary folders
        /// </summary>
        public void CleanSystemTempFolders()
        {
            try
            {
                _logger.Information("Cleaning system temporary folders");

                // Windows\Temp - system temp folder
                string systemTempPath = Path.Combine(
                    Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows",
                    "Temp");

                if (_fileSystem.DirectoryExists(systemTempPath))
                {
                    _logger.Information("Cleaning system temp folder: {TempPath}", systemTempPath);

                    try
                    {
                        DeleteFilesInFolder(systemTempPath);

                        foreach (var directory in _fileSystem.GetDirectories(systemTempPath))
                        {
                            _cleanerCore.DeleteFolder(directory);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _logger.Warning("Access denied to system temp folder: {TempPath}", systemTempPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error cleaning system temp folder: {TempPath}", systemTempPath);
                    }
                }
                else
                {
                    _logger.Warning("System temp folder not found at {TempPath}", systemTempPath);
                }

                // Add other system-wide temp locations if needed
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to clean system temporary folders");
            }
        }

        /// <summary>
        /// Deletes all files in a folder with error handling for each file
        /// </summary>
        /// <param name="folderPath">The path to the folder whose files should be deleted</param>
        private void DeleteFilesInFolder(string folderPath)
        {
            foreach (var filePath in _fileSystem.GetFiles(folderPath))
            {
                try
                {
                    _fileSystem.DeleteFile(filePath);
                }
                catch (IOException)
                {
                    // File is likely in use, log and continue
                    _logger.Debug("Could not delete file (in use): {FilePath}", filePath);
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.Debug("Access denied to file: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to delete file: {FilePath}", filePath);
                }
            }
        }
    }
}