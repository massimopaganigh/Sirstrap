namespace Sirstrap.Core.Settings
{
    public static class IniFormat
    {
        public static bool TryParseSectionHeader(string trimmedRow, out SettingsSection? section)
        {
            section = null;

            if (!trimmedRow.StartsWith('['))
                return false;

            if (trimmedRow.Equals("[SETTINGS]", StringComparison.InvariantCultureIgnoreCase))
                section = SettingsSection.Settings;
            else if (trimmedRow.Equals("[STATE]", StringComparison.InvariantCultureIgnoreCase))
                section = SettingsSection.State;

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
