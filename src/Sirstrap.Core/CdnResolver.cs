namespace Sirstrap.Core
{
    public sealed class CdnResolver : ICdnResolver
    {
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

            string normalizedOverride = _normalizer.Normalize(SirstrapConfiguration.RobloxCdnUriOverride);

            SirstrapConfiguration.RobloxCdnUriOverride = normalizedOverride;

            if (!string.IsNullOrEmpty(normalizedOverride))
            {
                SirstrapConfiguration.ResolvedRobloxCdnUri = normalizedOverride;

                Log.Information("[*] Roblox CDN URI override is set, using {0}.", normalizedOverride);

                _telemetry.RecordResolved(normalizedOverride, CdnResolutionSource.Override);

                return normalizedOverride;
            }

            if (string.IsNullOrEmpty(configuration.VersionHash))
            {
                Log.Warning("[*] Roblox CDN probe skipped (no version hash), falling back to {0}.", RobloxCdnService.DefaultBaseUri);

                SirstrapConfiguration.ResolvedRobloxCdnUri = RobloxCdnService.DefaultBaseUri;

                _telemetry.RecordResolved(RobloxCdnService.DefaultBaseUri, CdnResolutionSource.Fallback);

                return RobloxCdnService.DefaultBaseUri;
            }

            (string baseUri, CdnResolutionSource source) = await SelectFastestAsync(configuration, cancellationToken).ConfigureAwait(false);

            SirstrapConfiguration.ResolvedRobloxCdnUri = baseUri;

            _telemetry.RecordResolved(baseUri, source);

            return baseUri;
        }

        private async Task<(string BaseUri, CdnResolutionSource Source)> SelectFastestAsync(Configuration configuration, CancellationToken cancellationToken)
        {
            Log.Information("[*] Roblox CDN URI override is empty, selecting the fastest Roblox CDN...");

            IReadOnlyList<CdnCandidate> candidates = _candidateProvider.GetCandidates();

            // Probes start simultaneously, so the first successful one is the fastest:
            // returning immediately avoids waiting up to the probe timeout for slow or dead CDNs.
            List<Task<CdnProbeResult?>> pendingProbes = candidates
                .Select(candidate => _prober.ProbeAsync(candidate, configuration, cancellationToken))
                .ToList();

            while (pendingProbes.Count > 0)
            {
                Task<CdnProbeResult?> completedProbe = await Task.WhenAny(pendingProbes).ConfigureAwait(false);

                pendingProbes.Remove(completedProbe);

                CdnProbeResult? selected = await completedProbe.ConfigureAwait(false);

                if (selected != null)
                {
                    Log.Information("[*] Selected Roblox CDN: {0} ({1} ms).", selected.Candidate.BaseUri, (int)selected.Elapsed.TotalMilliseconds);

                    return (selected.Candidate.BaseUri, CdnResolutionSource.Probe);
                }
            }

            Log.Warning("[*] Failed to probe Roblox CDNs, falling back to {0}.", RobloxCdnService.DefaultBaseUri);

            return (RobloxCdnService.DefaultBaseUri, CdnResolutionSource.Fallback);
        }
    }
}
