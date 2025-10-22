namespace Sirstrap.Core.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private const string ARGUMENT_PREFIX = "--";

        private static readonly Dictionary<string, string> _binaryTypeBlobDirectory = new(StringComparer.OrdinalIgnoreCase)
        {
            { "WindowsPlayer", "/" },
            { "WindowsStudio64", "/" },
            { "MacPlayer", "/mac/" },
            { "MacStudio", "/mac/" }
        };

        public Configuration GetConfiguration(string[] rawArguments)
        {
            var arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            ParseConfiguration(rawArguments, arguments);

            var binaryType = arguments.GetValueOrDefault("binary-type", "WindowsPlayer");

            if (!_binaryTypeBlobDirectory.ContainsKey(binaryType))
                throw new ArgumentException($"Unsupported binary type: {binaryType}");

            return new Configuration
            {
                BinaryType = binaryType,
                BlobDirectory = GetBlobDirectory(arguments, binaryType),
                ChannelName = arguments.GetValueOrDefault("channel-name", "LIVE"),
                LaunchUri = arguments.GetValueOrDefault("launch-uri", string.Empty),
                VersionHash = arguments.GetValueOrDefault("version-hash", string.Empty)
            };
        }

        #region PRIVATE METHODS
        private static string GetBlobDirectory(Dictionary<string, string> arguments, string binaryType)
        {
            var blobDirectory = arguments.GetValueOrDefault("blob-directory");

            return string.IsNullOrEmpty(blobDirectory) ? _binaryTypeBlobDirectory[binaryType] : NormalizeBlobDirectory(blobDirectory);
        }

        private static bool IsArgument(string argument) => !string.IsNullOrEmpty(argument) && argument.StartsWith(ARGUMENT_PREFIX);

        private static string NormalizeBlobDirectory(string blobDirectory)
        {
            var normalizedBlobDirectory = blobDirectory;

            if (!normalizedBlobDirectory.StartsWith('/'))
                normalizedBlobDirectory = $"/{normalizedBlobDirectory}";

            if (!normalizedBlobDirectory.EndsWith('/'))
                normalizedBlobDirectory += "/";

            return normalizedBlobDirectory;
        }

        private static void ParseConfiguration(string[] arguments, Dictionary<string, string> configuration)
        {
            if (arguments.Length > 0
                && !IsArgument(arguments.First()))
                configuration["launch-uri"] = arguments.First();

            for (int i = 0; i < arguments.Length; i++)
            {
                if (!IsArgument(arguments[i]))
                    continue;

                var key = arguments[i][ARGUMENT_PREFIX.Length..];

                if (i + 1 < arguments.Length
                    && !IsArgument(arguments[i + 1]))
                {
                    var value = arguments[i + 1];

                    if (!string.IsNullOrEmpty(key)
                        && !string.IsNullOrEmpty(value))
                    {
                        configuration[key] = value;

                        i++;
                    }
                }
            }
        }
        #endregion
    }
}
