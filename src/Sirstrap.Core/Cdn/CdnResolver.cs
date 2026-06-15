namespace Sirstrap.Core.Cdn
{
    public sealed class CdnResolver : ICdnResolver
    {
        private readonly ICdnCandidateProvider _candidateProvider;
        private readonly ICdnUriNormalizer _normalizer;
        private readonly ICdnProber _prober;
        private readonly SirstrapConfiguration _sirstrapConfiguration;
        private readonly ICdnTelemetry _telemetry;

        public CdnResolver(ICdnUriNormalizer normalizer, ICdnCandidateProvider candidateProvider, ICdnProber prober, ICdnTelemetry telemetry, SirstrapConfiguration sirstrapConfiguration)
        {
            ArgumentNullException.ThrowIfNull(normalizer);
            ArgumentNullException.ThrowIfNull(candidateProvider);
            ArgumentNullException.ThrowIfNull(prober);
            ArgumentNullException.ThrowIfNull(telemetry);
            ArgumentNullException.ThrowIfNull(sirstrapConfiguration);

            _normalizer = normalizer;
            _candidateProvider = candidateProvider;
            _prober = prober;
            _telemetry = telemetry;
            _sirstrapConfiguration = sirstrapConfiguration;
        }

        #region PRIVATE METHODS
        private async Task<(string BaseUri, CdnResolutionSource Source)> SelectFastestAsync(Configuration configuration, CancellationToken cancellationToken)
        {
            Log.Information("[*] Selecting the fastest Roblox CDN...");

            var candidates = _candidateProvider.GetCandidates();
            List<Task<CdnProbeResult?>> pendingProbes = [.. candidates.Select(candidate => _prober.ProbeAsync(candidate, configuration, cancellationToken))];

            while (pendingProbes.Count > 0)
            {
                var completedProbe = await Task.WhenAny(pendingProbes).ConfigureAwait(false);

                pendingProbes.Remove(completedProbe);

                var selected = await completedProbe.ConfigureAwait(false);

                if (selected != null)
                {
                    Log.Information("[*] Selected the Roblox CDN {BaseUri} in {ElapsedMs} ms.", selected.Candidate.BaseUri, (int)selected.Elapsed.TotalMilliseconds);

                    return (selected.Candidate.BaseUri, CdnResolutionSource.Probe);
                }
            }

            Log.Warning("[!] Failed to probe the Roblox CDNs, falling back to {BaseUri}.", RobloxCdnService.DefaultBaseUri);

            return (RobloxCdnService.DefaultBaseUri, CdnResolutionSource.Fallback);
        }
        #endregion

        public async Task<string> ResolveAsync(Configuration configuration, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var normalizedOverride = _normalizer.Normalize(_sirstrapConfiguration.RobloxCdnUriOverride);
            _sirstrapConfiguration.RobloxCdnUriOverride = normalizedOverride;

            if (!string.IsNullOrEmpty(normalizedOverride))
            {
                _sirstrapConfiguration.ResolvedRobloxCdnUri = normalizedOverride;

                Log.Information("[*] Using the Roblox CDN URI override {BaseUri}.", normalizedOverride);
                _telemetry.RecordResolved(normalizedOverride, CdnResolutionSource.Override);

                return normalizedOverride;
            }

            if (string.IsNullOrEmpty(configuration.VersionHash))
            {
                Log.Warning("[!] Skipped the Roblox CDN probe (no version hash), falling back to {BaseUri}.", RobloxCdnService.DefaultBaseUri);

                _sirstrapConfiguration.ResolvedRobloxCdnUri = RobloxCdnService.DefaultBaseUri;

                _telemetry.RecordResolved(RobloxCdnService.DefaultBaseUri, CdnResolutionSource.Fallback);

                return RobloxCdnService.DefaultBaseUri;
            }

            (var baseUri, var source) = await SelectFastestAsync(configuration, cancellationToken).ConfigureAwait(false);
            _sirstrapConfiguration.ResolvedRobloxCdnUri = baseUri;

            _telemetry.RecordResolved(baseUri, source);

            return baseUri;
        }
    }
}
