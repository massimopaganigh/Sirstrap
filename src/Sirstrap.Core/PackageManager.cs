using Serilog;
using System.IO.Compression;

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
        /// <param name="downloadConfiguration">Configuration specifying the binary type, version, and other download parameters.</param>
        /// <returns>A task representing the asynchronous download operation.</returns>
        /// <remarks>
        /// The binary type determines which file is downloaded:
        /// - "MacPlayer" downloads RobloxPlayer.zip
        /// - Other types download RobloxStudioApp.zip
        /// The file is saved directly to the configured output location.
        /// </remarks>
        public async Task DownloadMacBinaryAsync(DownloadConfiguration downloadConfiguration)
        {
            var zipFileName = downloadConfiguration.BinaryType!.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) ? "RobloxPlayer.zip" : "RobloxStudioApp.zip";

            Log.Information("[*] Downloading ZIP archive for {0} ({1})...", downloadConfiguration.BinaryType, zipFileName);

            var bytes = await HttpHelper.GetBytesAsync(_httpClient, UrlBuilder.GetBinaryUrl(downloadConfiguration, zipFileName)).ConfigureAwait(false);

            await File.WriteAllBytesAsync(downloadConfiguration.GetOutputFileName(), bytes).ConfigureAwait(false);

            Log.Information("[*] File downloaded: {0}", downloadConfiguration.GetOutputFileName());
        }

        /// <summary>
        /// Downloads the package manifest file for the specified version.
        /// </summary>
        /// <param name="downloadConfiguration">Configuration specifying the version, channel, and other download parameters.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the manifest content as a string.</returns>
        public async Task<string> DownloadManifestAsync(DownloadConfiguration downloadConfiguration)
        {
            return await HttpHelper.GetStringAsync(_httpClient, UrlBuilder.GetManifestUrl(downloadConfiguration)).ConfigureAwait(false);
        }

        /// <summary>
        /// Processes the manifest content to download and assemble all required packages.
        /// </summary>
        /// <param name="manifestContent">The raw manifest content as a string.</param>
        /// <param name="downloadConfiguration">Configuration specifying the version, channel, and other download parameters.</param>
        /// <returns>A task representing the asynchronous processing operation.</returns>
        /// <remarks>
        /// This method:
        /// 1. Parses the manifest content
        /// 2. Validates that the manifest format is correct
        /// 3. Initiates the download and assembly of all packages listed in the manifest
        /// 
        /// The operation will terminate early if the manifest is invalid.
        /// </remarks>
        public async Task ProcessManifestAsync(string manifestContent, DownloadConfiguration downloadConfiguration)
        {
            var manifest = ManifestParser.Parse(manifestContent);

            if (!manifest.IsValid)
            {
                Log.Error("[!] Error: Invalid manifest version or format.");
                return;
            }

            await AssemblePackagesAsync(manifest, downloadConfiguration).ConfigureAwait(false);
        }

        /// <summary>
        /// Assembles the final ZIP archive by processing all packages from the manifest.
        /// </summary>
        /// <param name="manifest">The parsed manifest containing the list of packages to process.</param>
        /// <param name="downloadConfiguration">Configuration specifying the version and output file parameters.</param>
        /// <returns>A task representing the asynchronous assembly operation.</returns>
        /// <remarks>
        /// Creates a new ZIP archive at the specified output location, adds default settings,
        /// then downloads and processes all packages specified in the manifest.
        /// </remarks>
        private async Task AssemblePackagesAsync(Manifest manifest, DownloadConfiguration downloadConfiguration)
        {
            using (var finalZip = ZipFile.Open(downloadConfiguration.GetOutputFileName(), ZipArchiveMode.Create))
            {
                AddDefaultSettings(finalZip);

                await DownloadAndProcessPackagesAsync(manifest, finalZip, downloadConfiguration).ConfigureAwait(false);
            }

            Log.Information("[*] Archive assembled: {0}", downloadConfiguration.GetOutputFileName());
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

            PackageExtractor.AddTextFile(finalZip, "AppSettings.xml", settings);
        }

        /// <summary>
        /// Initiates the parallel download and processing of all packages from the manifest.
        /// </summary>
        /// <param name="manifest">The parsed manifest containing the list of packages to process.</param>
        /// <param name="finalZip">The ZIP archive where processed packages will be integrated.</param>
        /// <param name="downloadConfiguration">Configuration specifying download parameters.</param>
        /// <returns>A task representing the parallel download and processing operations.</returns>
        /// <remarks>
        /// Uses Task.WhenAll to download and process multiple packages concurrently,
        /// improving overall download performance.
        /// </remarks>
        private async Task DownloadAndProcessPackagesAsync(Manifest manifest, ZipArchive finalZip, DownloadConfiguration downloadConfiguration)
        {
            await Task.WhenAll(manifest.Packages!.Select(package => DownloadAndProcessPackageAsync(package, finalZip, downloadConfiguration)).ToList()).ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads and processes a single package from the manifest.
        /// </summary>
        /// <param name="package">The name of the package to download and process.</param>
        /// <param name="finalZip">The ZIP archive where the processed package will be integrated.</param>
        /// <param name="downloadConfiguration">Configuration specifying download parameters.</param>
        /// <returns>A task representing the asynchronous download and processing operation.</returns>
        /// <remarks>
        /// For each package, this method:
        /// 1. Downloads the package from the CDN
        /// 2. Delegates to PackageExtractor to extract and integrate the package contents
        ///    into the final ZIP archive
        /// </remarks>
        private async Task DownloadAndProcessPackageAsync(string package, ZipArchive finalZip, DownloadConfiguration downloadConfiguration)
        {
            Log.Information("[*] Downloading package {0}...", package);

            var bytes = await HttpHelper.GetBytesAsync(_httpClient, UrlBuilder.GetPackageUrl(downloadConfiguration, package)).ConfigureAwait(false);

            await PackageExtractor.ProcessPackageAsync(bytes, package, finalZip).ConfigureAwait(false);
        }
    }
}