namespace Sirstrap.Core
{
    public static class StringExtension
    {
        public static string? WithQuotes(this string? input)
        {
            string? output = input;

            if (input != null
                && !(input.StartsWith('\"')
                && input.EndsWith('\"')))
                output = string.Format("\"{0}\"", input);

            return output;
        }
    }
}