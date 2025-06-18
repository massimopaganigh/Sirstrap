namespace Sirstrap.Core
{
    /// <summary>
    /// Coordinates the download and assembly of Roblox application packages, handling 
    /// manifest processing, package downloads, and final ZIP archive creation.
    /// </summary>
    public class PackageManager(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        /// <summary>
        /// Downloads a macOS binary as a single ZIP archive file.
        /// </summary>
        /// <param name="configuration">Configuration specifying the binary type, version, and other download parameters.</param>
        /// <returns>A task representing the asynchronous download operation.</returns>
        /// <remarks>
        /// The binary type determines which file is downloaded:
        /// - "MacPlayer" downloads RobloxPlayer.zip
        /// - Other types download RobloxStudioApp.zip
        /// The file is saved directly to the configured output location.
        /// </remarks>
        public async Task DownloadMacBinaryAsync(Configuration configuration)
        {
            var zipFileName = configuration.BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) ? "RobloxPlayer.zip" : "RobloxStudioApp.zip";

            Log.Information("[*] Downloading ZIP archive for {0} ({1})...", configuration.BinaryType, zipFileName);

            var bytes = await BetterHttpClient.GetByteArrayAsync(_httpClient, UrlBuilder.GetBinaryUrl(configuration, zipFileName)).ConfigureAwait(false);

            await File.WriteAllBytesAsync(configuration.GetOutputPath(), bytes!).ConfigureAwait(false);

            Log.Information("[*] File downloaded: {0}", configuration.GetOutputPath());
        }

        /// <summary>
        /// Downloads the package manifest file for the specified version.
        /// </summary>
        /// <param name="configuration">Configuration specifying the version, channel, and other download parameters.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the manifest content as a string.</returns>
        public async Task<string?> DownloadManifestAsync(Configuration configuration)
        {
            return await BetterHttpClient.GetStringAsync(_httpClient, UrlBuilder.GetManifestUrl(configuration)).ConfigureAwait(false);
        }

        /// <summary>
        /// Processes the manifest content to download and assemble all required packages.
        /// </summary>
        /// <param name="manifestContent">The raw manifest content as a string.</param>
        /// <param name="configuration">Configuration specifying the version, channel, and other download parameters.</param>
        /// <returns>A task representing the asynchronous processing operation.</returns>
        /// <remarks>
        /// This method:
        /// 1. Parses the manifest content
        /// 2. Validates that the manifest format is correct
        /// 3. Initiates the download and assembly of all packages listed in the manifest
        /// 
        /// The operation will terminate early if the manifest is invalid.
        /// </remarks>
        public async Task ProcessManifestAsync(string manifestContent, Configuration configuration)
        {
            var manifest = ManifestParser.Parse(manifestContent);

            if (!manifest.IsValid)
            {
                Log.Error("[!] Error: Invalid manifest version or format.");
                return;
            }

            await AssemblePackagesAsync(manifest, configuration).ConfigureAwait(false);
        }

        /// <summary>
        /// Assembles the final ZIP archive by processing all packages from the manifest.
        /// </summary>
        /// <param name="manifest">The parsed manifest containing the list of packages to process.</param>
        /// <param name="configuration">Configuration specifying the version and output file parameters.</param>
        /// <returns>A task representing the asynchronous assembly operation.</returns>
        /// <remarks>
        /// Creates a new ZIP archive at the specified output location, adds default settings,
        /// then downloads and processes all packages specified in the manifest.
        /// </remarks>
        private async Task AssemblePackagesAsync(Manifest manifest, Configuration configuration)
        {
            using (var finalZip = ZipFile.Open(configuration.GetOutputPath(), ZipArchiveMode.Create))
            {
                AddDefaultSettings(finalZip);

                await DownloadAndProcessPackagesAsync(manifest, finalZip, configuration).ConfigureAwait(false);
            }

            Log.Information("[*] Archive assembled: {0}", configuration.GetOutputPath());
        }

        /// <summary>
        /// Adds default application settings to the ZIP archive.
        /// </summary>
        /// <param name="finalZip">The ZIP archive to add settings to.</param>
        /// <remarks>
        /// Creates an AppSettings.xml file with base configuration for content folder
        /// and base URL settings.
        /// </remarks>
        private static void AddDefaultSettings(ZipArchive finalZip)
        {
            const string settings = """<?xml version="1.0" encoding="UTF-8"?><Settings><ContentFolder>content</ContentFolder><BaseUrl>http://www.roblox.com</BaseUrl></Settings>""";

            PackageExtractor.ExtractPackageContent(settings, "AppSettings.xml", finalZip);
        }

        /// <summary>
        /// Initiates the parallel download and processing of all packages from the manifest.
        /// </summary>
        /// <param name="manifest">The parsed manifest containing the list of packages to process.</param>
        /// <param name="finalZip">The ZIP archive where processed packages will be integrated.</param>
        /// <param name="configuration">Configuration specifying download parameters.</param>
        /// <returns>A task representing the parallel download and processing operations.</returns>
        /// <remarks>
        /// Uses Task.WhenAll to download and process multiple packages concurrently,
        /// improving overall download performance.
        /// </remarks>
        private async Task DownloadAndProcessPackagesAsync(Manifest manifest, ZipArchive finalZip, Configuration configuration)
        {
            await Task.WhenAll(manifest.Packages!.Select(package => DownloadAndProcessPackageAsync(package, finalZip, configuration)).ToList()).ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads and processes a single package from the manifest.
        /// </summary>
        /// <param name="package">The name of the package to download and process.</param>
        /// <param name="finalZip">The ZIP archive where the processed package will be integrated.</param>
        /// <param name="configuration">Configuration specifying download parameters.</param>
        /// <returns>A task representing the asynchronous download and processing operation.</returns>
        /// <remarks>
        /// For each package, this method:
        /// 1. Downloads the package from the CDN
        /// 2. Delegates to PackageExtractor to extract and integrate the package contents
        ///    into the final ZIP archive
        /// </remarks>
        private async Task DownloadAndProcessPackageAsync(string package, ZipArchive finalZip, Configuration configuration)
        {
            Log.Information("[*] Downloading package {0}...", package);

            var bytes = await BetterHttpClient.GetByteArrayAsync(_httpClient, UrlBuilder.GetPackageUrl(configuration, package)).ConfigureAwait(false);

            await PackageExtractor.ExtractPackageBytesAsync(bytes, package, finalZip).ConfigureAwait(false);
        }
    }
}