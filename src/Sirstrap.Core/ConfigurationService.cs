namespace Sirstrap.Core
{
    public static class ConfigurationService
    {
        private const string OPTION_PREFIX = "--";

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

        private static bool IsOption(string argument) => !string.IsNullOrEmpty(argument)
            && argument.StartsWith(OPTION_PREFIX);

        private static string NormalizeBlobDirectory(string blobDirectory)
        {
            string normalized = blobDirectory;

            if (!normalized.StartsWith('/'))
                normalized = $"/{normalized}";

            if (!normalized.EndsWith('/'))
                normalized += "/";

            return normalized;
        }

        private static void ParseLaunchUrl(string[] arguments, Dictionary<string, string> configuration)
        {
            if (arguments.Length > 0
                && !IsOption(arguments.First()))
                configuration["launch-uri"] = arguments.First();
        }

        private static void ParseOptions(string[] arguments, Dictionary<string, string> configuration)
        {
            for (int i = 0; i < arguments.Length; i++)
            {
                if (!IsOption(arguments[i]))
                    continue;

                string key = RemoveOptionPrefix(arguments[i]);

                if (i + 1 < arguments.Length
                    && !IsOption(arguments[i + 1]))
                {
                    string value = arguments[i + 1];

                    if (!string.IsNullOrEmpty(key)
                        && !string.IsNullOrEmpty(value))
                    {
                        configuration[key] = value;
                        i++;
                    }
                }
            }
        }

        private static string RemoveOptionPrefix(string option) => option[OPTION_PREFIX.Length..];

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

        public static Dictionary<string, string> ParseConfiguration(string[] arguments)
        {
            ArgumentNullException.ThrowIfNull(arguments);

            Dictionary<string, string> configuration = new(StringComparer.OrdinalIgnoreCase);

            ParseOptions(arguments, configuration);
            ParseLaunchUrl(arguments, configuration);

            return configuration;
        }
    }
}
