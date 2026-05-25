namespace Sirstrap.Core
{
    public sealed class CdnResolver : ICdnResolver
    {
        private readonly ICdnUriNormalizer _normalizer;
        private readonly ICdnCandidateProvider _candidateProvider;
        private readonly ICdnProber _prober;

        public CdnResolver(ICdnUriNormalizer normalizer, ICdnCandidateProvider candidateProvider, ICdnProber prober)
        {
            ArgumentNullException.ThrowIfNull(normalizer);
            ArgumentNullException.ThrowIfNull(candidateProvider);
            ArgumentNullException.ThrowIfNull(prober);

            _normalizer = normalizer;
            _candidateProvider = candidateProvider;
            _prober = prober;
        }

        public async Task<string> ResolveAsync(Configuration configuration, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            string normalizedOverride = _normalizer.Normalize(SirstrapConfiguration.RobloxCdnUriOverride);

            SirstrapConfiguration.RobloxCdnUriOverride = normalizedOverride;

            if (!string.IsNullOrEmpty(normalizedOverride))
            {
                SirstrapConfiguration.ResolvedRobloxCdnUri = normalizedOverride;

                Log.Information("[*] Roblox CDN URI override is set, using {0}.", normalizedOverride);

                return normalizedOverride;
            }

            if (string.IsNullOrEmpty(configuration.VersionHash))
            {
                Log.Warning("[*] Roblox CDN probe skipped (no version hash), falling back to {0}.", RobloxCdnService.DefaultBaseUri);

                SirstrapConfiguration.ResolvedRobloxCdnUri = RobloxCdnService.DefaultBaseUri;

                return RobloxCdnService.DefaultBaseUri;
            }

            string fastestBaseUri = await SelectFastestAsync(configuration, cancellationToken).ConfigureAwait(false);

            SirstrapConfiguration.ResolvedRobloxCdnUri = fastestBaseUri;

            return fastestBaseUri;
        }

        private async Task<string> SelectFastestAsync(Configuration configuration, CancellationToken cancellationToken)
        {
            Log.Information("[*] Roblox CDN URI override is empty, selecting the fastest Roblox CDN...");

            IReadOnlyList<CdnCandidate> candidates = _candidateProvider.GetCandidates();

            Task<CdnProbeResult?>[] probeTasks = candidates
                .Select(candidate => _prober.ProbeAsync(candidate, configuration, cancellationToken))
                .ToArray();

            CdnProbeResult? selected = (await Task.WhenAll(probeTasks).ConfigureAwait(false))
                .OfType<CdnProbeResult>()
                .OrderBy(result => result.Elapsed)
                .ThenBy(result => result.Candidate.FallbackPriority)
                .FirstOrDefault();

            if (selected == null)
            {
                Log.Warning("[*] Failed to probe Roblox CDNs, falling back to {0}.", RobloxCdnService.DefaultBaseUri);

                return RobloxCdnService.DefaultBaseUri;
            }

            Log.Information("[*] Selected Roblox CDN: {0} ({1} ms).", selected.Candidate.BaseUri, (int)selected.Elapsed.TotalMilliseconds);

            return selected.Candidate.BaseUri;
        }
    }
}
