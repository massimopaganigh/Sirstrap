namespace Sirstrap.Core
{
    /// <summary>
    /// Provides functionality to create, validate, and normalize download configuration settings
    /// for different Roblox binary types and deployment channels.
    /// </summary>
    public static class ConfigurationManager
    {
        /// <summary>
        /// A dictionary mapping binary type identifiers to their associated metadata.
        /// </summary>
        /// <remarks>
        /// Supports case-insensitive lookup for binary types including:
        /// - WindowsPlayer: Standard Windows Roblox Player
        /// - WindowsStudio64: Windows Roblox Studio (64-bit)
        /// - MacPlayer: macOS Roblox Player
        /// - MacStudio: macOS Roblox Studio
        /// </remarks>
        private static readonly Dictionary<string, BinaryTypeInfo> BinaryTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            { "WindowsPlayer", new BinaryTypeInfo { VersionFile = "/version", DefaultBlobDir = "/" } },
            { "WindowsStudio64", new BinaryTypeInfo { VersionFile = "/versionQTStudio", DefaultBlobDir = "/" } },
            { "MacPlayer", new BinaryTypeInfo { VersionFile = "/mac/version", DefaultBlobDir = "/mac/" } },
            { "MacStudio", new BinaryTypeInfo { VersionFile = "/mac/versionStudio", DefaultBlobDir = "/mac/" } }
        };

        /// <summary>
        /// Creates a download configuration based on the provided command-line arguments.
        /// </summary>
        /// <param name="args">A dictionary of argument name-value pairs from the command line.</param>
        /// <returns>
        /// A fully initialized and validated DownloadConfiguration object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when an unsupported binary type is specified.
        /// </exception>
        /// <remarks>
        /// Applies defaults for missing arguments:
        /// - channel: "LIVE"
        /// - binaryType: "WindowsPlayer"
        /// - compressZip: false
        /// - compressionLevel: 6
        /// 
        /// The blob directory is determined based on the binary type if not explicitly provided.
        /// </remarks>
        public static DownloadConfiguration CreateDownloadConfiguration(Dictionary<string, string> args)
        {
            var downloadConfiguration = new DownloadConfiguration
            {
                Channel = args.GetValueOrDefault("channel", "LIVE"),
                BinaryType = args.GetValueOrDefault("binaryType", "WindowsPlayer"),
                Version = args.GetValueOrDefault("version") ?? string.Empty,
                BlobDir = GetBlobDirectory(args),
                CompressZip = args.GetValueOrDefault("compressZip", "false").Equals("true", StringComparison.OrdinalIgnoreCase),
                CompressionLevel = GetCompressionLevel(args)
            };

            ValidateConfiguration(downloadConfiguration);

            return downloadConfiguration;
        }

        /// <summary>
        /// Determines the appropriate blob directory based on arguments or binary type defaults.
        /// </summary>
        /// <param name="arguments">The command-line arguments dictionary.</param>
        /// <returns>
        /// A normalized blob directory path, ensuring it starts and ends with a forward slash.
        /// </returns>
        /// <remarks>
        /// If a blob directory is explicitly provided in the arguments, it is normalized.
        /// Otherwise, the default blob directory for the specified binary type is used.
        /// </remarks>
        private static string GetBlobDirectory(Dictionary<string, string> arguments)
        {
            var blobDir = arguments.GetValueOrDefault("blobDir");

            if (string.IsNullOrEmpty(blobDir))
            {
                return BinaryTypes.TryGetValue(arguments.GetValueOrDefault("binaryType", "WindowsPlayer"), out var bt) ? bt.DefaultBlobDir : string.Empty;
            }

            return NormalizeBlobDirectory(blobDir);
        }

        /// <summary>
        /// Parses and validates the compression level from the command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments dictionary.</param>
        /// <returns>
        /// An integer compression level between 1 and 9, defaulting to 6 if not specified
        /// or if the specified value is invalid.
        /// </returns>
        /// <remarks>
        /// Valid compression levels are integers from 1 (least compression) to 9 (most compression).
        /// </remarks>
        private static int GetCompressionLevel(Dictionary<string, string> args)
        {
            if (args.TryGetValue("compressionLevel", out var value) && int.TryParse(value, out var level) && level is >= 1 and <= 9)
            {
                return level;
            }

            return 6;
        }

        /// <summary>
        /// Validates that the download configuration contains supported settings.
        /// </summary>
        /// <param name="downloadConfiguration">The configuration to validate.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the binary type specified in the configuration is not supported.
        /// </exception>
        private static void ValidateConfiguration(DownloadConfiguration downloadConfiguration)
        {
            if (!BinaryTypes.ContainsKey(downloadConfiguration.BinaryType))
            {
                throw new ArgumentException($"Unsupported binary type: {downloadConfiguration.BinaryType}");
            }
        }

        /// <summary>
        /// Ensures the blob directory path is properly formatted.
        /// </summary>
        /// <param name="blobDir">The blob directory path to normalize.</param>
        /// <returns>
        /// A normalized blob directory path that starts and ends with a forward slash.
        /// </returns>
        private static string NormalizeBlobDirectory(string blobDir)
        {
            if (!blobDir.StartsWith($"/"))
            {
                blobDir = $"/{blobDir}";
            }

            if (!blobDir.EndsWith($"/"))
            {
                blobDir += "/";
            }

            return blobDir;
        }
    }
}