namespace Sirstrap.Core.Cdn
{
    public static class RobloxCdnService
    {
#pragma warning disable S1075 // URIs should not be hardcoded - Official Roblox deployment CDN.
        public const string DefaultBaseUri = "https://setup.rbxcdn.com";
#pragma warning restore S1075

        private static readonly CdnUriNormalizer _normalizer = new();

        public static string NormalizeCdnUriOverride(string? cdnUriOverride) => _normalizer.Normalize(cdnUriOverride);
    }
}
