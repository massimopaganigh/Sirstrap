namespace Sirstrap.Core
{
    public sealed class CdnResolver : ICdnResolver
    {
        // Bumped on every resolution so a stale background drain from a previous
        // resolution cannot overwrite the ranking published by the current one.
        private static int _rankingGeneration;

        private readonly ICdnUriNormalizer _normalizer;
        private readonly ICdnCandidateProvider _candidateProvider;
        private readonly ICdnProber _prober;
        private readonly ICdnTelemetry _telemetry;

        public CdnResolver(ICdnUriNormalizer normalizer, ICdnCandidateProvider candidateProvider, ICdnProber prober, ICdnTelemetry telemetry)
        {
            ArgumentNullException.ThrowIfNull(normalizer);
            ArgumentNullException.ThrowIfNull(candidateProvider);
            ArgumentNullException.ThrowIfNull(prober);
            ArgumentNullException.ThrowIfNull(telemetry);

            _normalizer = normalizer;
            _candidateProvider = candidateProvider;
            _prober = prober;
            _telemetry = telemetry;
        }

        public async Task<string> ResolveAsync(Configuration configuration, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            int generation = Interlocked.Increment(ref _rankingGeneration);

            string normalizedOverride = _normalizer.Normalize(SirstrapConfiguration.RobloxCdnUriOverride);

            SirstrapConfiguration.RobloxCdnUriOverride = normalizedOverride;

            if (!string.IsNullOrEmpty(normalizedOverride))
            {
                SirstrapConfiguration.ResolvedRobloxCdnUri = normalizedOverride;
                SirstrapConfiguration.ResolvedRobloxCdnUris = [normalizedOverride];

                Log.Information("[*] Roblox CDN URI override is set, using {0}.", normalizedOverride);

                _telemetry.RecordResolved(normalizedOverride, CdnResolutionSource.Override);

                return normalizedOverride;
            }

            if (string.IsNullOrEmpty(configuration.VersionHash))
            {
                Log.Warning("[*] Roblox CDN probe skipped (no version hash), falling back to {0}.", RobloxCdnService.DefaultBaseUri);

                SirstrapConfiguration.ResolvedRobloxCdnUri = RobloxCdnService.DefaultBaseUri;
                SirstrapConfiguration.ResolvedRobloxCdnUris = [RobloxCdnService.DefaultBaseUri];

                _telemetry.RecordResolved(RobloxCdnService.DefaultBaseUri, CdnResolutionSource.Fallback);

                return RobloxCdnService.DefaultBaseUri;
            }

            (string baseUri, CdnResolutionSource source) = await SelectFastestAsync(configuration, generation, cancellationToken).ConfigureAwait(false);

            SirstrapConfiguration.ResolvedRobloxCdnUri = baseUri;

            _telemetry.RecordResolved(baseUri, source);

            return baseUri;
        }

        private async Task<(string BaseUri, CdnResolutionSource Source)> SelectFastestAsync(Configuration configuration, int generation, CancellationToken cancellationToken)
        {
            Log.Information("[*] Roblox CDN URI override is empty, selecting the fastest Roblox CDN...");

            IReadOnlyList<CdnCandidate> candidates = _candidateProvider.GetCandidates();

            // Probes start simultaneously, so the first successful one is the fastest:
            // returning immediately avoids waiting up to the probe timeout for slow or dead CDNs.
            List<Task<CdnProbeResult?>> pendingProbes = candidates
                .Select(candidate => _prober.ProbeAsync(candidate, configuration, cancellationToken))
                .ToList();

            List<string> ranking = [];

            PublishRanking(generation, candidates, ranking);

            while (pendingProbes.Count > 0)
            {
                Task<CdnProbeResult?> completedProbe = await Task.WhenAny(pendingProbes).ConfigureAwait(false);

                pendingProbes.Remove(completedProbe);

                CdnProbeResult? selected = await completedProbe.ConfigureAwait(false);

                if (selected != null)
                {
                    ranking.Add(selected.Candidate.BaseUri);

                    PublishRanking(generation, candidates, ranking);

                    Log.Information("[*] Selected Roblox CDN: {0} ({1} ms).", selected.Candidate.BaseUri, (int)selected.Elapsed.TotalMilliseconds);

                    // Keep ranking the slower probes in the background so per-file fallback
                    // tries CDNs from fastest to slowest without delaying the download.
                    _ = DrainRemainingProbesAsync(generation, candidates, ranking, pendingProbes);

                    return (selected.Candidate.BaseUri, CdnResolutionSource.Probe);
                }
            }

            Log.Warning("[*] Failed to probe Roblox CDNs, falling back to {0}.", RobloxCdnService.DefaultBaseUri);

            return (RobloxCdnService.DefaultBaseUri, CdnResolutionSource.Fallback);
        }

        private static async Task DrainRemainingProbesAsync(int generation, IReadOnlyList<CdnCandidate> candidates, List<string> ranking, List<Task<CdnProbeResult?>> pendingProbes)
        {
            while (pendingProbes.Count > 0)
            {
                Task<CdnProbeResult?> completedProbe = await Task.WhenAny(pendingProbes).ConfigureAwait(false);

                pendingProbes.Remove(completedProbe);

                CdnProbeResult? result;

                try
                {
                    result = await completedProbe.ConfigureAwait(false);
                }
                catch (Exception)
                {
                    continue;
                }

                if (result != null)
                {
                    ranking.Add(result.Candidate.BaseUri);

                    PublishRanking(generation, candidates, ranking);
                }
            }
        }

        // Probed-and-ranked CDNs come first (fastest to slowest); candidates that failed or have
        // not answered yet are kept at the tail so a missing file is tried everywhere before failing.
        private static void PublishRanking(int generation, IReadOnlyList<CdnCandidate> candidates, List<string> ranking)
        {
            if (Volatile.Read(ref _rankingGeneration) != generation)
                return;

            SirstrapConfiguration.ResolvedRobloxCdnUris =
            [
                .. ranking,
                .. candidates.Select(candidate => candidate.BaseUri).Where(baseUri => !ranking.Contains(baseUri))
            ];
        }
    }
}
