using Serilog;

namespace Sirstrap.Core
{
    /// <summary>
    /// Orchestrates the complete Roblox application deployment process, including 
    /// version determination, downloading, package processing, installation, and launching.
    /// </summary>
    public class RobloxDownloader
    {
        private readonly VersionManager _versionManager;
        private readonly PackageManager _packageManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RobloxDownloader"/> class.
        /// </summary>
        /// <remarks>
        /// Creates a shared HttpClient instance and initializes the version and package managers.
        /// </remarks>
        public RobloxDownloader()
        {
            var httpClient = new HttpClient();

            _versionManager = new VersionManager(httpClient);
            _packageManager = new PackageManager(httpClient);
        }

        /// <summary>
        /// Executes the complete Roblox download, installation, and launch process based on command-line arguments.
        /// </summary>
        /// <param name="args">Command-line arguments that configure the download and installation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// The execution workflow:
        /// 1. Parses command-line arguments and creates a download configuration
        /// 2. Initializes the download process, including version determination
        /// 3. Checks if the requested version is already installed
        /// 4. If already installed, launches the application
        /// 5. Otherwise, downloads and processes the necessary files
        /// 6. Installs and launches the application
        /// 
        /// Exceptions during any part of the process are caught, logged, and will halt execution.
        /// </remarks>
        public async Task ExecuteAsync(string[] args)
        {
            try
            {
                var downloadConfiguration = ConfigurationManager.CreateDownloadConfiguration(CommandLineParser.Parse(args));

                if (!await InitializeDownloadAsync(downloadConfiguration).ConfigureAwait(false))
                {
                    return;
                }

                if (IsAlreadyInstalled(downloadConfiguration))
                {
                    Log.Information("[*] Version {0} is already installed.", downloadConfiguration.Version);

                    if (LaunchApplication(downloadConfiguration, string.IsNullOrEmpty(downloadConfiguration.LaunchUrl)))
                    {
                        return;
                    }
                }

                await DownloadAndProcessFilesAsync(downloadConfiguration).ConfigureAwait(false);

                InstallAndLaunchApplication(downloadConfiguration);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Initializes the download configuration by determining the appropriate version.
        /// </summary>
        /// <param name="downloadConfiguration">The download configuration to initialize.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is <c>true</c> if
        /// initialization succeeded; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// If the version is not specified in the configuration, this method attempts to
        /// retrieve the latest version from the version manager. It also ensures the version
        /// string is in the normalized format.
        /// </remarks>
        private async Task<bool> InitializeDownloadAsync(DownloadConfiguration downloadConfiguration)
        {
            if (string.IsNullOrEmpty(downloadConfiguration.Version))
            {
                downloadConfiguration.Version = await _versionManager.GetLatestVersionAsync(downloadConfiguration.BinaryType).ConfigureAwait(false);

                if (string.IsNullOrEmpty(downloadConfiguration.Version))
                {
                    return false;
                }
            }

            downloadConfiguration.Version = VersionManager.NormalizeVersion(downloadConfiguration.Version);

            return true;
        }

        /// <summary>
        /// Determines whether the specified version is already installed.
        /// </summary>
        /// <param name="downloadConfiguration">The download configuration containing the version to check.</param>
        /// <returns>
        /// <c>true</c> if the version is already installed; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Currently only checks for Windows Player installations by verifying if the
        /// version directory exists.
        /// </remarks>
        private static bool IsAlreadyInstalled(DownloadConfiguration downloadConfiguration)
        {
            return downloadConfiguration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase) && Directory.Exists(PathManager.GetVersionInstallPath(downloadConfiguration.Version));
        }

        /// <summary>
        /// Attempts to launch the Roblox application.
        /// </summary>
        /// <param name="downloadConfiguration">The download configuration containing the version to launch.</param>
        /// <param name="waitForExit">Whether to wait for the Roblox process to exit.</param>
        /// <returns>
        /// <c>true</c> if the application was successfully launched; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Currently only supports launching the Windows Player version.
        /// If a LaunchUrl is specified, Roblox will be launched directly into that experience.
        /// </remarks>
        private static bool LaunchApplication(DownloadConfiguration downloadConfiguration, bool waitForExit = false)
        {
            return downloadConfiguration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase) && ApplicationLauncher.Launch(downloadConfiguration, waitForExit);
        }

        /// <summary>
        /// Downloads and processes the necessary files based on the binary type.
        /// </summary>
        /// <param name="downloadConfiguration">The download configuration specifying what to download.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// For macOS binaries, downloads the complete ZIP archive directly.
        /// For other platforms, downloads the manifest and processes the individual packages.
        /// </remarks>
        private async Task DownloadAndProcessFilesAsync(DownloadConfiguration downloadConfiguration)
        {
            if (downloadConfiguration.IsMacBinary())
            {
                await _packageManager.DownloadMacBinaryAsync(downloadConfiguration).ConfigureAwait(false);
            }
            else
            {
                var manifest = await _packageManager.DownloadManifestAsync(downloadConfiguration).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(manifest))
                {
                    await _packageManager.ProcessManifestAsync(manifest, downloadConfiguration).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Installs and launches the Roblox application.
        /// </summary>
        /// <param name="downloadConfiguration">The download configuration containing installation details.</param>
        /// <remarks>
        /// Currently only installs and launches the Windows Player version.
        /// The application is installed from the downloaded ZIP archive and then launched.
        /// If a LaunchUrl is specified, Roblox will be launched directly into that experience.
        /// </remarks>
        private static void InstallAndLaunchApplication(DownloadConfiguration downloadConfiguration)
        {
            if (!downloadConfiguration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            ApplicationInstaller.Install(downloadConfiguration);

            bool hasLaunchUrl = !string.IsNullOrEmpty(downloadConfiguration.LaunchUrl);

            LaunchApplication(downloadConfiguration, !hasLaunchUrl);
        }
    }
}