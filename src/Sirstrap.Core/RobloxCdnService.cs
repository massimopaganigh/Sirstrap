namespace Sirstrap.Core
{
    public static class RobloxCdnService
    {
#pragma warning disable S1075 // URIs should not be hardcoded - Official Roblox deployment CDN.
        public const string DefaultBaseUri = "https://setup.rbxcdn.com";
#pragma warning restore S1075

        private static readonly TimeSpan _probeTimeout = TimeSpan.FromSeconds(5);
        private static readonly ICdnUriNormalizer _normalizer = new CdnUriNormalizer();
        private static readonly ICdnCandidateProvider _candidateProvider = new DefaultCdnCandidateProvider();
        private static readonly ICdnProbeUriFactory _probeUriFactory = new CdnProbeUriFactory();

        public static string NormalizeCdnUriOverride(string? cdnUriOverride) => _normalizer.Normalize(cdnUriOverride);

        public static Task<string> ResolveAsync(HttpClient httpClient, Configuration configuration, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(httpClient);

            ICdnResolver resolver = new CdnResolver(
                _normalizer,
                _candidateProvider,
                new HttpCdnProber(httpClient, _probeUriFactory, _probeTimeout));

            return resolver.ResolveAsync(configuration, cancellationToken);
        }
    }
}
