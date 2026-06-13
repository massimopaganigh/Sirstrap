namespace Sirstrap.Core.Settings
{
    public static class IniFormat
    {
        public static HashSet<string> ExtractSectionKeys(IEnumerable<string> rows, string sectionHeader)
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var inSection = false;

            foreach (var row in rows)
            {
                var trimmedRow = row.Trim();

                if (IsSectionHeader(trimmedRow, sectionHeader, out var isMatchingSection))
                {
                    inSection = isMatchingSection;

                    continue;
                }

                if (!inSection)
                    continue;

                if (TryParseRow(trimmedRow, out var key, out _))
                    keys.Add(key);
            }

            return keys;
        }

        public static bool IsSectionHeader(string trimmedRow, string sectionHeader, out bool isMatchingSection)
        {
            isMatchingSection = false;

            if (!trimmedRow.StartsWith('['))
                return false;

            isMatchingSection = trimmedRow.Equals(sectionHeader, StringComparison.InvariantCultureIgnoreCase);

            return true;
        }

        public static bool TryParseRow(string trimmedRow, out string key, out string value)
        {
            key = string.Empty;
            value = string.Empty;

            if (!trimmedRow.Contains('='))
                return false;

            var parts = trimmedRow.Split('=', 2);

            if (parts.Length != 2)
                return false;

            key = parts[0].Trim();
            value = parts[1].Trim();

            return !string.IsNullOrEmpty(key);
        }
    }
}
