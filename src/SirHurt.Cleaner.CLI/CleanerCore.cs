using Serilog;
using System.Diagnostics;

namespace SirHurt.Cleaner.CLI
{
    /// <summary>
    /// Core implementation for the system cleaner operations
    /// </summary>
    public class CleanerCore
    {
        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IUserInteraction _userInteraction;
        private readonly IProcessManager _processManager;
        private readonly CleanerConfig _config;

        public CleanerCore(
            ILogger logger,
            IFileSystem fileSystem,
            IUserInteraction userInteraction,
            IProcessManager processManager,
            CleanerConfig config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _userInteraction = userInteraction ?? throw new ArgumentNullException(nameof(userInteraction));
            _processManager = processManager ?? throw new ArgumentNullException(nameof(processManager));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Ensures required processes are closed before cleaning
        /// </summary>
        /// <returns>True if all necessary processes were closed or not running, false otherwise</returns>
        public bool EnsureApplicationsClosed()
        {
            var processesToCheck = _config.ProcessesToClose;
            bool allClosed = true;
            bool anyRunning = false;

            // First check which processes are running
            foreach (var processName in processesToCheck.Where(_processManager.IsProcessRunning))
            {
                _logger.Information("{ProcessName} is currently running", processName);
                anyRunning = true;
            }

            if (!anyRunning)
            {
                _logger.Information("No processes that need to be closed are running");
                return true;
            }

            // Ask for confirmation to close all running processes
            bool confirmed = _userInteraction.ConfirmAction(
                "Roblox and/or SirHurt applications need to be closed before cleaning. Close them now?");

            if (!confirmed)
            {
                _logger.Warning("User declined to close applications. Cleaning may be incomplete");
                return false;
            }

            // Try to close each running process
            foreach (var processName in processesToCheck.Where(_processManager.IsProcessRunning))
            {
                bool processClosed = _processManager.TryKillProcess(processName);
                if (!processClosed)
                {
                    _logger.Warning("Failed to close all instances of {ProcessName}", processName);
                    allClosed = false;
                }
            }

            return allClosed;
        }

        /// <summary>
        /// Deletes a folder and all its contents with error handling.
        /// </summary>
        /// <param name="path">Full path to the folder</param>
        public void DeleteFolder(string path)
        {
            if (!_fileSystem.DirectoryExists(path))
            {
                _logger.Debug("Folder does not exist: {FolderPath}", path);
                return;
            }

            try
            {
                _logger.Information("Deleting folder: {FolderPath}", path);
                _fileSystem.DeleteDirectory(path, recursive: true);
                _logger.Information("Folder deleted: {FolderPath}", path);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Warning(ex, "Access denied to folder: {FolderPath}. Attempting with elevated permissions.", path);
                TryDeleteWithElevatedPermissions(path);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to delete folder {FolderPath}", path);
            }
        }

        /// <summary>
        /// Attempts to delete a folder using PowerShell with elevated permissions.
        /// </summary>
        /// <param name="path">Path to the folder</param>
        private void TryDeleteWithElevatedPermissions(string path)
        {
            try
            {
                _logger.Information("Attempting to delete folder using PowerShell: {FolderPath}", path);

                string command = $"Remove-Item -Path \"{path}\" -Force -Recurse -ErrorAction SilentlyContinue";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(startInfo);

                if (process == null)
                {
                    _logger.Error("Failed to start PowerShell process");
                    return;
                }

                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    _logger.Warning("PowerShell reported an error: {Error}", error);
                }

                if (_fileSystem.DirectoryExists(path))
                {
                    _logger.Warning("PowerShell command completed but folder still exists: {FolderPath}", path);
                }
                else
                {
                    _logger.Information("Successfully deleted folder with PowerShell: {FolderPath}", path);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete folder using PowerShell: {FolderPath}", path);
            }
        }

        /// <summary>
        /// Deletes folder contents file by file, asking for confirmation for specific files.
        /// </summary>
        /// <param name="folderPath">Path to the folder to process</param>
        public void DeleteFolderContentsWithConfirmation(string folderPath)
        {
            if (!_fileSystem.DirectoryExists(folderPath))
            {
                _logger.Debug("Folder does not exist: {FolderPath}", folderPath);
                return;
            }

            _logger.Information("Processing folder contents: {FolderPath}", folderPath);

            try
            {
                // Process files in the root directory
                foreach (var filePath in _fileSystem.GetFiles(folderPath))
                {
                    string fileName = _fileSystem.GetFileName(filePath);
                    if (_config.ExcludedFiles.Contains(fileName))
                    {
                        _logger.Debug("Skipping excluded file: {FilePath}", filePath);
                        continue;
                    }

                    DeleteFileWithConfirmationIfNeeded(filePath, folderPath);
                }

                // Process subdirectories
                foreach (var subDir in _fileSystem.GetDirectories(folderPath))
                {
                    // Get the relative path from the base folder to check against FilesRequiringConfirmation and FoldersRequiringConfirmation
                    string relativePath = _fileSystem.GetRelativePath(folderPath, subDir);
                    string subDirName = _fileSystem.GetFileName(subDir);

                    // Skip Roblox Studio-specific folders and numeric build-version folders
                    if (_config.ExcludedSubFolders.Contains(subDirName) || IsRobloxStudioBuildFolder(subDirName))
                    {
                        _logger.Debug("Skipping excluded folder: {SubDir}", subDir);
                        continue;
                    }

                    // Check if this folder requires confirmation before deletion
                    bool folderRequiresConfirmation = _config.FoldersRequiringConfirmation.Contains(relativePath);

                    if (folderRequiresConfirmation)
                    {
                        DeleteFolderWithConfirmationIfNeeded(subDir, folderPath);
                    }
                    else
                    {
                        // Check if this is a directory that might contain files requiring confirmation
                        bool containsSpecialFiles = _config.FilesRequiringConfirmation.Any(f =>
                            _fileSystem.GetDirectoryName(f) == subDirName);

                        if (containsSpecialFiles)
                        {
                            // Process this directory file by file
                            foreach (var filePath in _fileSystem.GetFiles(subDir))
                            {
                                DeleteFileWithConfirmationIfNeeded(filePath, folderPath);
                            }

                            // If the directory is now empty, delete it
                            if (!_fileSystem.GetFileSystemEntries(subDir).Any())
                            {
                                try
                                {
                                    _fileSystem.DeleteDirectory(subDir, recursive: false);
                                    _logger.Information("Deleted empty directory: {DirPath}", subDir);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex, "Failed to delete directory: {DirPath}", subDir);
                                }
                            }
                        }
                        else
                        {
                            // This directory doesn't contain special files, delete it completely
                            DeleteFolder(subDir);
                        }
                    }
                }

                // If the main folder is now empty, delete it
                if (_fileSystem.DirectoryExists(folderPath) && !_fileSystem.GetFileSystemEntries(folderPath).Any())
                {
                    try
                    {
                        _fileSystem.DeleteDirectory(folderPath, recursive: false);
                        _logger.Information("Deleted empty folder: {FolderPath}", folderPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failed to delete empty folder: {FolderPath}", folderPath);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Warning(ex, "Access denied to folder: {FolderPath}", folderPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing folder: {FolderPath}", folderPath);
            }
        }

        /// <summary>
        /// Deletes a folder, asking for confirmation if it's one of the specified folders.
        /// </summary>
        /// <param name="folderPath">Full path to the folder</param>
        /// <param name="baseFolderPath">Base folder path for relative path calculation</param>
        private void DeleteFolderWithConfirmationIfNeeded(string folderPath, string baseFolderPath)
        {
            try
            {
                // Get the relative path from the base folder
                string relativePath = _fileSystem.GetRelativePath(baseFolderPath, folderPath);

                // Check if this folder requires confirmation
                bool requiresConfirmation = _config.FoldersRequiringConfirmation.Contains(relativePath);

                if (requiresConfirmation)
                {
                    _logger.Information("Configuration or data folder found: {FolderPath}", folderPath);

                    bool confirmed = _userInteraction.ConfirmAction(
                        $"The folder {folderPath} contains configuration or authentication data.");

                    if (confirmed)
                    {
                        DeleteFolder(folderPath);
                        _logger.Information("Configuration or data folder deleted: {FolderPath}", folderPath);
                    }
                    else
                    {
                        _logger.Information("Configuration or data folder kept: {FolderPath}", folderPath);
                    }
                }
                else
                {
                    // Delete the folder without confirmation
                    DeleteFolder(folderPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to process folder: {FolderPath}", folderPath);
            }
        }

        /// <summary>
        /// Deletes a file, asking for confirmation if it's one of the specified files.
        /// </summary>
        /// <param name="filePath">Full path to the file</param>
        /// <param name="baseFolderPath">Base folder path for relative path calculation</param>
        private void DeleteFileWithConfirmationIfNeeded(string filePath, string baseFolderPath)
        {
            try
            {
                // Get the relative path from the base folder
                string relativePath = _fileSystem.GetRelativePath(baseFolderPath, filePath);

                // Check if this file requires confirmation
                bool requiresConfirmation = _config.FilesRequiringConfirmation.Contains(relativePath);

                if (requiresConfirmation)
                {
                    _logger.Information("Authentication or configuration file found: {FilePath}", filePath);

                    bool confirmed = _userInteraction.ConfirmAction(
                        $"The file {filePath} is used for authentication or configuration.");

                    if (confirmed)
                    {
                        _fileSystem.DeleteFile(filePath);
                        _logger.Information("Authentication or configuration file deleted: {FilePath}", filePath);
                    }
                    else
                    {
                        _logger.Information("Authentication or configuration file kept: {FilePath}", filePath);
                    }
                }
                else
                {
                    // Delete the file without confirmation
                    _fileSystem.DeleteFile(filePath);
                    _logger.Debug("File deleted: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to process file: {FilePath}", filePath);
            }
        }
        /// <summary>
        /// Returns true if the folder name consists entirely of digits.
        /// Roblox Studio stores build-version folders with purely numeric names (e.g. "5082874847").
        /// This check is intentionally scoped to the Roblox folder cleanup path.
        /// </summary>
        private static bool IsRobloxStudioBuildFolder(string name) =>
            !string.IsNullOrEmpty(name) && name.All(char.IsDigit);
    }
}