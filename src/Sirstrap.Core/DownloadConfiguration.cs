using Serilog;

namespace Sirstrap.Core
{
    /// <summary>
    /// Encapsulates configuration settings for downloading and processing Roblox application
    /// packages, including version, channel, binary type, and compression parameters.
    /// </summary>
    public class DownloadConfiguration
    {
        /// <summary>
        /// Gets or sets the deployment channel to download from (e.g., "LIVE" or other test channels).
        /// </summary>
        /// <value>
        /// The channel name that determines which Roblox deployment source is used.
        /// </value>
        public string? Channel { get; set; }

        /// <summary>
        /// Gets or sets the type of binary to download (e.g., "WindowsPlayer", "WindowsStudio", "MacPlayer", "MacStudio").
        /// </summary>
        /// <value>
        /// The binary type that determines which Roblox application package to download.
        /// </value>
        public string? BinaryType { get; set; }

        /// <summary>
        /// Gets or sets the version identifier for the Roblox application to download.
        /// </summary>
        /// <value>
        /// The version string, typically in the format "version-X.Y.Z.W".
        /// </value>
        public string? Version { get; set; }

        /// <summary>
        /// Gets or sets the blob directory path within the CDN where the packages are stored.
        /// </summary>
        /// <value>
        /// The relative path to the blob directory, typically ending with a forward slash.
        /// </value>
        public string? BlobDir { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to compress the final ZIP archive.
        /// </summary>
        /// <value>
        /// <c>true</c> if the final ZIP should be compressed; otherwise, <c>false</c>.
        /// </value>
        public bool CompressZip { get; set; }

        /// <summary>
        /// Gets or sets the compression level to use when creating the ZIP archive.
        /// </summary>
        /// <value>
        /// An integer representing the compression level, where higher values indicate
        /// more compression but potentially slower processing.
        /// </value>
        public int CompressionLevel { get; set; }

        /// <summary>
        /// Gets or sets the launch URL or arguments to pass to the Roblox executable.
        /// </summary>
        /// <value>
        /// The URL or arguments string that specifies which Roblox experience to launch.
        /// Typically a protocol URL like "roblox://game/placeid=12345678".
        /// </value>
        public string? LaunchUrl { get; set; }

        /// <summary>
        /// Determines whether the configured binary type is a macOS application.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the binary type is "MacPlayer" or "MacStudio" (case-insensitive);
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool IsMacBinary()
        {
            return BinaryType!.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) || BinaryType.Equals("MacStudio", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Generates the output filename for the downloaded and processed ZIP archive.
        /// </summary>
        /// <returns>
        /// A filename based on the configured version with a ".zip" extension.
        /// </returns>
        public string GetOutputFileName()
        {
            return Path.Combine(GetCacheDirectory(), $"{Version}.zip");
        }

        public static string GetCacheDirectory()
        {
            string cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Cache");

            Directory.CreateDirectory(cacheDir);

            return cacheDir;
        }

        public static void ClearCacheDirectory()
        {
            string cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Cache");

            if (Directory.Exists(cacheDir))
            {
                try
                {
                    Log.Information("[*] Clearing cache directory: {0}", cacheDir);

                    foreach (string file in Directory.GetFiles(cacheDir))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            Log.Warning("[!] Could not delete cache file {0}: {1}", file, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Error clearing cache directory: {0}", ex.Message);
                }
            }

            Directory.CreateDirectory(cacheDir);
        }
    }
}