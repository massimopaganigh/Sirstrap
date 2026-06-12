namespace Sirstrap.Core.Tests
{
    public class CdnResolverTests
    {
        private sealed class RecordingTelemetry : ICdnTelemetry
        {
            public List<(string BaseUri, CdnResolutionSource Source)> Records { get; } = [];

            public List<(string BaseUri, bool Success, TimeSpan Elapsed)> Probes { get; } = [];

            public void RecordResolved(string baseUri, CdnResolutionSource source) => Records.Add((baseUri, source));

            public void RecordProbe(string baseUri, bool success, TimeSpan elapsed) => Probes.Add((baseUri, success, elapsed));
        }

        private sealed class FakeProber : ICdnProber
        {
            private readonly Dictionary<string, TimeSpan?> _results;

            public FakeProber(Dictionary<string, TimeSpan?> results) => _results = results;

            public async Task<CdnProbeResult?> ProbeAsync(CdnCandidate candidate, Configuration configuration, CancellationToken cancellationToken)
            {
                if (_results.TryGetValue(candidate.BaseUri, out var elapsed)
                    && elapsed.HasValue)
                {
                    await Task.Delay(elapsed.Value, cancellationToken);

                    return new CdnProbeResult(candidate, elapsed.Value);
                }

                return null;
            }
        }

        private sealed class StaticCandidateProvider(IReadOnlyList<CdnCandidate> candidates) : ICdnCandidateProvider
        {
            public IReadOnlyList<CdnCandidate> GetCandidates() => candidates;
        }

        private static Configuration NewConfiguration(string versionHash = "v1") => new()
        {
            ChannelName = "LIVE",
            BlobDirectory = "/",
            VersionHash = versionHash
        };

        [Fact]
        public async Task ResolveAsync_RecordsOverrideTelemetry_WhenOverrideSet()
        {
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                SirstrapConfiguration.RobloxCdnUriOverride = "https://custom.example.com";

                RecordingTelemetry telemetry = new();
                CdnResolver resolver = new(
                    new CdnUriNormalizer(),
                    new StaticCandidateProvider([new CdnCandidate(RobloxCdnService.DefaultBaseUri, 0)]),
                    new FakeProber([]),
                    telemetry);

                string resolved = await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

                Assert.Equal("https://custom.example.com", resolved);
                Assert.Single(telemetry.Records);
                Assert.Equal(("https://custom.example.com", CdnResolutionSource.Override), telemetry.Records[0]);
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;
            }
        }

        [Fact]
        public async Task ResolveAsync_RecordsFallbackTelemetry_WhenVersionHashEmpty()
        {
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                SirstrapConfiguration.RobloxCdnUriOverride = string.Empty;

                RecordingTelemetry telemetry = new();
                CdnResolver resolver = new(
                    new CdnUriNormalizer(),
                    new StaticCandidateProvider([new CdnCandidate(RobloxCdnService.DefaultBaseUri, 0)]),
                    new FakeProber([]),
                    telemetry);

                string resolved = await resolver.ResolveAsync(NewConfiguration(versionHash: string.Empty), TestContext.Current.CancellationToken);

                Assert.Equal(RobloxCdnService.DefaultBaseUri, resolved);
                Assert.Single(telemetry.Records);
                Assert.Equal((RobloxCdnService.DefaultBaseUri, CdnResolutionSource.Fallback), telemetry.Records[0]);
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;
            }
        }

        [Fact]
        public async Task ResolveAsync_RecordsFallbackTelemetry_WhenAllProbesFail()
        {
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                SirstrapConfiguration.RobloxCdnUriOverride = string.Empty;

                IReadOnlyList<CdnCandidate> candidates = [new CdnCandidate("https://a.example.com", 0)];
                Dictionary<string, TimeSpan?> probeResults = new()
                {
                    ["https://a.example.com"] = null
                };

                RecordingTelemetry telemetry = new();
                CdnResolver resolver = new(
                    new CdnUriNormalizer(),
                    new StaticCandidateProvider(candidates),
                    new FakeProber(probeResults),
                    telemetry);

                string resolved = await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

                Assert.Equal(RobloxCdnService.DefaultBaseUri, resolved);
                Assert.Single(telemetry.Records);
                Assert.Equal((RobloxCdnService.DefaultBaseUri, CdnResolutionSource.Fallback), telemetry.Records[0]);
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;
            }
        }

        [Fact]
        public async Task ResolveAsync_ReturnsFirstSuccessfulProbe_WithoutWaitingForSlowProbes()
        {
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                SirstrapConfiguration.RobloxCdnUriOverride = string.Empty;

                IReadOnlyList<CdnCandidate> candidates =
                [
                    new CdnCandidate("https://hanging.example.com", 0),
                    new CdnCandidate("https://fast.example.com", 2)
                ];
                Dictionary<string, TimeSpan?> probeResults = new()
                {
                    ["https://hanging.example.com"] = TimeSpan.FromSeconds(30),
                    ["https://fast.example.com"] = TimeSpan.FromMilliseconds(50)
                };

                CdnResolver resolver = new(
                    new CdnUriNormalizer(),
                    new StaticCandidateProvider(candidates),
                    new FakeProber(probeResults),
                    new RecordingTelemetry());

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                string resolved = await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

                stopwatch.Stop();

                Assert.Equal("https://fast.example.com", resolved);
                Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10), $"Resolution took {stopwatch.Elapsed} instead of returning at the first successful probe.");
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;
            }
        }

        [Fact]
        public async Task ResolveAsync_PublishesRankedCdnUris_FastestFirst()
        {
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                SirstrapConfiguration.RobloxCdnUriOverride = string.Empty;

                IReadOnlyList<CdnCandidate> candidates =
                [
                    new CdnCandidate("https://slow.example.com", 0),
                    new CdnCandidate("https://fast.example.com", 2),
                    new CdnCandidate("https://dead.example.com", 2)
                ];
                Dictionary<string, TimeSpan?> probeResults = new()
                {
                    ["https://slow.example.com"] = TimeSpan.FromMilliseconds(300),
                    ["https://fast.example.com"] = TimeSpan.FromMilliseconds(50),
                    ["https://dead.example.com"] = null
                };

                CdnResolver resolver = new(
                    new CdnUriNormalizer(),
                    new StaticCandidateProvider(candidates),
                    new FakeProber(probeResults),
                    new RecordingTelemetry());

                string resolved = await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

                Assert.Equal("https://fast.example.com", resolved);

                // The slower probes are ranked in the background, so wait for the full ranking.
                var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);

                while (SirstrapConfiguration.ResolvedRobloxCdnUris[1] != "https://slow.example.com" && DateTime.UtcNow < deadline)
                    await Task.Delay(20, TestContext.Current.CancellationToken);

                Assert.Equal(["https://fast.example.com", "https://slow.example.com", "https://dead.example.com"], SirstrapConfiguration.ResolvedRobloxCdnUris);
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;
            }
        }

        [Fact]
        public async Task ResolveAsync_PublishesOnlyOverride_WhenOverrideSet()
        {
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                SirstrapConfiguration.RobloxCdnUriOverride = "https://custom.example.com";

                CdnResolver resolver = new(
                    new CdnUriNormalizer(),
                    new StaticCandidateProvider([new CdnCandidate(RobloxCdnService.DefaultBaseUri, 0)]),
                    new FakeProber([]),
                    new RecordingTelemetry());

                await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

                Assert.Equal(["https://custom.example.com"], SirstrapConfiguration.ResolvedRobloxCdnUris);
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;
            }
        }

        [Fact]
        public async Task ResolveAsync_PublishesAllCandidates_WhenAllProbesFail()
        {
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                SirstrapConfiguration.RobloxCdnUriOverride = string.Empty;

                IReadOnlyList<CdnCandidate> candidates =
                [
                    new CdnCandidate("https://a.example.com", 0),
                    new CdnCandidate("https://b.example.com", 2)
                ];
                Dictionary<string, TimeSpan?> probeResults = new()
                {
                    ["https://a.example.com"] = null,
                    ["https://b.example.com"] = null
                };

                CdnResolver resolver = new(
                    new CdnUriNormalizer(),
                    new StaticCandidateProvider(candidates),
                    new FakeProber(probeResults),
                    new RecordingTelemetry());

                await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

                Assert.Equal(["https://a.example.com", "https://b.example.com"], SirstrapConfiguration.ResolvedRobloxCdnUris);
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;
            }
        }

        [Fact]
        public async Task ResolveAsync_RecordsProbeTelemetry_WithFastestCandidate()
        {
            string originalOverride = SirstrapConfiguration.RobloxCdnUriOverride;

            try
            {
                SirstrapConfiguration.RobloxCdnUriOverride = string.Empty;

                IReadOnlyList<CdnCandidate> candidates =
                [
                    new CdnCandidate("https://slow.example.com", 0),
                    new CdnCandidate("https://fast.example.com", 2)
                ];
                Dictionary<string, TimeSpan?> probeResults = new()
                {
                    ["https://slow.example.com"] = TimeSpan.FromMilliseconds(300),
                    ["https://fast.example.com"] = TimeSpan.FromMilliseconds(50)
                };

                RecordingTelemetry telemetry = new();
                CdnResolver resolver = new(
                    new CdnUriNormalizer(),
                    new StaticCandidateProvider(candidates),
                    new FakeProber(probeResults),
                    telemetry);

                string resolved = await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

                Assert.Equal("https://fast.example.com", resolved);
                Assert.Single(telemetry.Records);
                Assert.Equal(("https://fast.example.com", CdnResolutionSource.Probe), telemetry.Records[0]);
            }
            finally
            {
                SirstrapConfiguration.RobloxCdnUriOverride = originalOverride;
            }
        }
    }
}
