namespace Sirstrap
{
    /// <summary>
    /// Provides functionality to parse command-line arguments in the format "--key=value"
    /// into a dictionary for easier access and configuration.
    /// </summary>
    public static class CommandLineParser
    {
        /// <summary>
        /// Parses an array of command-line arguments into a dictionary of key-value pairs.
        /// </summary>
        /// <param name="args">The command-line arguments to parse.</param>
        /// <returns>
        /// A dictionary mapping argument names to their values, with case-insensitive keys.
        /// Only arguments in the format "--key=value" are included.
        /// </returns>
        /// <remarks>
        /// The parsing process:
        /// 1. Filters for arguments that start with "--"
        /// 2. Removes the leading "--" prefix
        /// 3. Splits each argument at the first "=" character
        /// 4. Discards arguments that don't contain an "=" character
        /// 5. Creates a dictionary with keys being the part before "=" and values being the part after
        /// 
        /// Example: "--channel=LIVE" becomes a dictionary entry with key "channel" and value "LIVE"
        /// </remarks>
        public static Dictionary<string, string> Parse(string[] args)
        {
            return args.Where(arg => arg.StartsWith("--")).Select(arg => arg[2..].Split('=', 2)).Where(parts => parts.Length == 2).ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);
        }
    }
}