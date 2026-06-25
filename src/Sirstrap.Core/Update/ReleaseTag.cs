namespace Sirstrap.Core.Update
{
    public readonly record struct ReleaseTag(Version Version, string Channel)
    {
        public static bool TryParse(string? tagName, out ReleaseTag releaseTag)
        {
            releaseTag = default;

            if (string.IsNullOrWhiteSpace(tagName))
                return false;

            var tagParts = tagName.Split('-');

            if (!Version.TryParse(tagParts[0].TrimStart('v'), out Version? version))
                return false;

            releaseTag = new ReleaseTag(version, tagParts.Length > 1 ? $"-{tagParts[1]}" : string.Empty);

            return true;
        }
    }
}
