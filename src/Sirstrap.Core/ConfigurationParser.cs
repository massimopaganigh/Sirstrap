namespace Sirstrap.Core
{
    public class ConfigurationParser
    {
        private const int MAX_SPLIT_PARTS = 2;
        private const string OPTION_PREFIX = "--";

        private bool IsOption(string argument) => !string.IsNullOrEmpty(argument) && argument.StartsWith(OPTION_PREFIX);

        private bool IsValidKeyValuePair((string key, string value) pair) => !string.IsNullOrEmpty(pair.key) && !string.IsNullOrEmpty(pair.value);

        private void ParseLaunchUrl(string[] arguments, Dictionary<string, string> configuration)
        {
            if (arguments.Length > 0
                && !IsOption(arguments.First()))
                configuration["launchUrl"] = arguments.First();
        }

        private void ParseOptions(string[] arguments, Dictionary<string, string> configuration)
        {
            var options = arguments.Where(IsOption).Select(RemoveOptionPrefix).Select(SplitOption).Where(IsValidKeyValuePair);

            foreach (var (key, value) in options)
                configuration[key] = value;
        }

        private string RemoveOptionPrefix(string option) => option[OPTION_PREFIX.Length..];

        private (string key, string value) SplitOption(string option)
        {
            string[] parts = option.Split('=', MAX_SPLIT_PARTS);

            return parts.Length == MAX_SPLIT_PARTS
                ? (parts[0], parts[1])
                : (string.Empty, string.Empty);
        }

        public Dictionary<string, string> ParseConfiguration(string[] arguments)
        {
            ArgumentNullException.ThrowIfNull(arguments);

            Dictionary<string, string> configuration = new(StringComparer.OrdinalIgnoreCase);

            ParseOptions(arguments, configuration);
            ParseLaunchUrl(arguments, configuration);

            return configuration;
        }
    }
}