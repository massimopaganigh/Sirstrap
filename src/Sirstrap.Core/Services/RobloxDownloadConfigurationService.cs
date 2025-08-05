using Sirstrap.Core.Interfaces;
using Sirstrap.Core.Models;

namespace Sirstrap.Core.Services
{
    public class RobloxDownloadConfigurationService : IRobloxDownloadConfigurationService
    {
        #region Properties
        private const string OPTION_PREFIX = "--";
        private static readonly Dictionary<string, string> _binaryTypes = new(StringComparer.OrdinalIgnoreCase) { { "WindowsPlayer", "/" }, { "WindowsStudio64", "/" }, { "MacPlayer", "/mac/" }, { "MacStudio", "/mac/" } };
        #endregion

        #region Private Methods
        private RobloxDownloadConfiguration CreateConfigurationFromArguments(Dictionary<string, string> arguments)
        {
            string binaryType = arguments.GetValueOrDefault("binary-type", "WindowsPlayer");

            ValidateBinaryType(binaryType);

            RobloxDownloadConfiguration robloxDownloadConfiguration = new()
            {
                BinaryType = binaryType,
                ChannelName = arguments.GetValueOrDefault("channel-name", "LIVE"),
                VersionHash = arguments.GetValueOrDefault("version-hash", string.Empty),
                BlobDirectory = GetBlobDirectory(arguments, binaryType),
                LaunchUri = arguments.GetValueOrDefault("launch-uri", string.Empty)
            };

            return robloxDownloadConfiguration;
        }
        private static string GetBlobDirectory(Dictionary<string, string> arguments, string binaryType)
        {
            string? blobDirectory = arguments.GetValueOrDefault("blob-directory");

            return string.IsNullOrEmpty(blobDirectory) ? _binaryTypes[binaryType] : NormalizeBlobDirectory(blobDirectory);
        }

        private static bool IsOption(string argument) => !string.IsNullOrEmpty(argument) && argument.StartsWith(OPTION_PREFIX);

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
            if (arguments.Length > 0 && !IsOption(arguments.First()))
                configuration["launch-uri"] = arguments.First();
        }

        private static void ParseOptions(string[] arguments, Dictionary<string, string> configuration)
        {
            for (int i = 0; i < arguments.Length; i++)
            {
                if (!IsOption(arguments[i]))
                    continue;

                string key = RemoveOptionPrefix(arguments[i]);

                if (i + 1 < arguments.Length && !IsOption(arguments[i + 1]))
                {
                    string value = arguments[i + 1];

                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
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
        #endregion

        public void ClearCacheDirectory()
        {
            string cacheDirectory = GetCacheDirectory();

            try
            {
                foreach (string file in Directory.GetFiles(cacheDirectory))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception) { /* Sybau 🥀 */ }
                }
            }
            catch (Exception) { /* Sybau 🥀 */ }

            Directory.CreateDirectory(cacheDirectory);
        }

        public static string GetCacheDirectory()
        {
            string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Cache");

            Directory.CreateDirectory(cacheDirectory);

            return cacheDirectory;
        }

        public RobloxDownloadConfiguration ParseConfiguration(string[] arguments)
        {
            RobloxDownloadConfiguration robloxDownloadConfiguration = new();

            try
            {
                Dictionary<string, string> configuration = new(StringComparer.OrdinalIgnoreCase);

                ParseOptions(arguments, configuration);
                ParseLaunchUrl(arguments, configuration);

                return CreateConfigurationFromArguments(configuration);
            }
            catch (Exception)
            {
                return robloxDownloadConfiguration;
            }
        }
    }
}