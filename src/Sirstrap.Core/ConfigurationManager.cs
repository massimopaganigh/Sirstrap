namespace Sirstrap.Core
{
    public static class ConfigurationManager
    {
        private static readonly Dictionary<string, string> _binaryTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            { "WindowsPlayer", "/" },
            { "WindowsStudio64", "/" },
            { "MacPlayer", "/mac/" },
            { "MacStudio", "/mac/" }
        };

        private static string GetBlobDirectory(Dictionary<string, string> arguments, string binaryType)
        {
            string? blobDirectory = arguments.GetValueOrDefault("blob-directory");

            return string.IsNullOrEmpty(blobDirectory)
                ? _binaryTypes[binaryType]
                : NormalizeBlobDirectory(blobDirectory);
        }

        private static string NormalizeBlobDirectory(string blobDirectory)
        {
            string normalized = blobDirectory;

            if (!normalized.StartsWith('/'))
                normalized = $"/{normalized}";

            if (!normalized.EndsWith('/'))
                normalized += "/";

            return normalized;
        }

        private static void ValidateBinaryType(string binaryType)
        {
            if (!_binaryTypes.ContainsKey(binaryType))
                throw new ArgumentException($"Unsupported binary type: {binaryType}.");
        }

        public static Configuration CreateConfigurationFromArguments(Dictionary<string, string> arguments)
        {
            string binaryType = arguments.GetValueOrDefault("binary-type", "WindowsPlayer");

            ValidateBinaryType(binaryType);

            Configuration configuration = new()
            {
                BinaryType = binaryType,
                ChannelName = arguments.GetValueOrDefault("channel-name", "LIVE"),
                VersionHash = arguments.GetValueOrDefault("version-hash", string.Empty),
                BlobDirectory = GetBlobDirectory(arguments, binaryType),
                LaunchUri = arguments.GetValueOrDefault("launch-uri", string.Empty)
            };

            return configuration;
        }
    }
}