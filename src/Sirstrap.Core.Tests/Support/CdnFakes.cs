namespace Sirstrap.Core.Tests.Support
{
    public sealed class RecordingCdnTelemetry : ICdnTelemetry
    {
        public List<(string BaseUri, CdnResolutionSource Source)> Resolved { get; } = [];

        public List<(string BaseUri, bool Success, TimeSpan Elapsed)> Probes { get; } = [];

        public void RecordResolved(string baseUri, CdnResolutionSource source) => Resolved.Add((baseUri, source));

        public void RecordProbe(string baseUri, bool success, TimeSpan elapsed) => Probes.Add((baseUri, success, elapsed));
    }

    public sealed class FakeCdnProber(Dictionary<string, TimeSpan?> results) : ICdnProber
    {
        public async Task<CdnProbeResult?> ProbeAsync(CdnCandidate candidate, Configuration configuration, CancellationToken cancellationToken)
        {
            if (results.TryGetValue(candidate.BaseUri, out var elapsed) && elapsed.HasValue)
            {
                await Task.Delay(elapsed.Value, cancellationToken);

                return new CdnProbeResult(candidate, elapsed.Value);
            }

            return null;
        }
    }

    public sealed class StaticCandidateProvider(IReadOnlyList<CdnCandidate> candidates) : ICdnCandidateProvider
    {
        public IReadOnlyList<CdnCandidate> GetCandidates() => candidates;
    }

    public sealed class FakeCdnProbeUriFactory(string uri = "https://probe.example.com/manifest") : ICdnProbeUriFactory
    {
        public string Create(Configuration configuration, string baseUri) => uri;
    }

    public sealed class FakeRobloxUriFactory : IRobloxUriFactory
    {
        public List<string> Calls { get; } = [];

        public string GetManifestUri(Configuration configuration) => Record($"manifest:{configuration.VersionHash}");

        public string GetManifestUri(Configuration configuration, string robloxCdnUri) => Record($"manifest:{robloxCdnUri}:{configuration.VersionHash}");

        public string GetPackageUri(Configuration configuration, string package) => Record($"package:{package}");

        public string GetPackageUri(Configuration configuration, string package, string robloxCdnUri) => Record($"package:{robloxCdnUri}:{package}");

        private string Record(string value)
        {
            Calls.Add(value);

            return value;
        }
    }
}
