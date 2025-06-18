namespace Sirstrap.Core
{
    public static class ConfigurationParser
    {
        private const string OPTION_PREFIX = "--";

        private static bool IsOption(string argument) => !string.IsNullOrEmpty(argument)
            && argument.StartsWith(OPTION_PREFIX);

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

                if (i + 1 < arguments.Length && !IsOption(arguments[i + 1]))
                {
                    string value = arguments[i + 1];

                    if (!string.IsNullOrEmpty(key)
                        && !string.IsNullOrEmpty(value))
                    {
                        configuration[key] = value;
                        i++;
                    }
                }
                else if (!string.IsNullOrEmpty(key))
                {
                    configuration[key] = "true";
                }
            }
        }

        private static string RemoveOptionPrefix(string option) => option[OPTION_PREFIX.Length..];

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