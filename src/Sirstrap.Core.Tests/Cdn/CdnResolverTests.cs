namespace Sirstrap.Core.Tests.Cdn
{
    public class CdnResolverTests
    {
        private static Configuration NewConfiguration(string versionHash = "v1") => new()
        {
            ChannelName = "LIVE",
            BlobDirectory = "/",
            VersionHash = versionHash
        };

        private static CdnResolver NewResolver(SirstrapConfiguration config, ICdnCandidateProvider provider, ICdnProber prober, ICdnTelemetry telemetry)
            => new(new CdnUriNormalizer(), provider, prober, telemetry, config);

        [Fact]
        public void Constructor_Throws_OnNullArguments()
        {
            var normalizer = new CdnUriNormalizer();
            var provider = new StaticCandidateProvider([]);
            var prober = new FakeCdnProber([]);
            var telemetry = new RecordingCdnTelemetry();
            var config = new SirstrapConfiguration();

            Assert.Throws<ArgumentNullException>(() => new CdnResolver(null!, provider, prober, telemetry, config));
            Assert.Throws<ArgumentNullException>(() => new CdnResolver(normalizer, null!, prober, telemetry, config));
            Assert.Throws<ArgumentNullException>(() => new CdnResolver(normalizer, provider, null!, telemetry, config));
            Assert.Throws<ArgumentNullException>(() => new CdnResolver(normalizer, provider, prober, null!, config));
            Assert.Throws<ArgumentNullException>(() => new CdnResolver(normalizer, provider, prober, telemetry, null!));
        }

        [Fact]
        public async Task ResolveAsync_Throws_WhenConfigurationNull()
        {
            CdnResolver resolver = NewResolver(new SirstrapConfiguration(), new StaticCandidateProvider([]), new FakeCdnProber([]), new RecordingCdnTelemetry());

            await Assert.ThrowsAsync<ArgumentNullException>(() => resolver.ResolveAsync(null!, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ResolveAsync_UsesOverride_WhenSet()
        {
            SirstrapConfiguration config = new() { RobloxCdnUriOverride = "https://custom.example.com" };
            RecordingCdnTelemetry telemetry = new();
            CdnResolver resolver = NewResolver(config, new StaticCandidateProvider([new CdnCandidate(RobloxCdnService.DefaultBaseUri, 0)]), new FakeCdnProber([]), telemetry);

            string resolved = await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

            Assert.Equal("https://custom.example.com", resolved);
            Assert.Equal("https://custom.example.com", config.ResolvedRobloxCdnUri);
            Assert.Equal(("https://custom.example.com", CdnResolutionSource.Override), Assert.Single(telemetry.Resolved));
        }

        [Fact]
        public async Task ResolveAsync_NormalizesOverride_AndWritesItBack()
        {
            SirstrapConfiguration config = new() { RobloxCdnUriOverride = "  https://custom.example.com///  " };
            CdnResolver resolver = NewResolver(config, new StaticCandidateProvider([]), new FakeCdnProber([]), new RecordingCdnTelemetry());

            string resolved = await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

            Assert.Equal("https://custom.example.com", resolved);
            Assert.Equal("https://custom.example.com", config.RobloxCdnUriOverride);
        }

        [Fact]
        public async Task ResolveAsync_FallsBack_WhenVersionHashEmpty()
        {
            SirstrapConfiguration config = new() { RobloxCdnUriOverride = string.Empty };
            RecordingCdnTelemetry telemetry = new();
            CdnResolver resolver = NewResolver(config, new StaticCandidateProvider([new CdnCandidate(RobloxCdnService.DefaultBaseUri, 0)]), new FakeCdnProber([]), telemetry);

            string resolved = await resolver.ResolveAsync(NewConfiguration(versionHash: string.Empty), TestContext.Current.CancellationToken);

            Assert.Equal(RobloxCdnService.DefaultBaseUri, resolved);
            Assert.Equal((RobloxCdnService.DefaultBaseUri, CdnResolutionSource.Fallback), Assert.Single(telemetry.Resolved));
        }

        [Fact]
        public async Task ResolveAsync_FallsBack_WhenAllProbesFail()
        {
            SirstrapConfiguration config = new() { RobloxCdnUriOverride = string.Empty };
            RecordingCdnTelemetry telemetry = new();
            CdnResolver resolver = NewResolver(
                config,
                new StaticCandidateProvider([new CdnCandidate("https://a.example.com", 0)]),
                new FakeCdnProber(new() { ["https://a.example.com"] = null }),
                telemetry);

            string resolved = await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

            Assert.Equal(RobloxCdnService.DefaultBaseUri, resolved);
            Assert.Equal((RobloxCdnService.DefaultBaseUri, CdnResolutionSource.Fallback), Assert.Single(telemetry.Resolved));
        }

        [Fact]
        public async Task ResolveAsync_ReturnsFastestProbe_WithoutWaitingForSlowOnes()
        {
            SirstrapConfiguration config = new() { RobloxCdnUriOverride = string.Empty };
            RecordingCdnTelemetry telemetry = new();
            CdnResolver resolver = NewResolver(
                config,
                new StaticCandidateProvider(
                [
                    new CdnCandidate("https://hanging.example.com", 0),
                    new CdnCandidate("https://fast.example.com", 2)
                ]),
                new FakeCdnProber(new()
                {
                    ["https://hanging.example.com"] = TimeSpan.FromSeconds(30),
                    ["https://fast.example.com"] = TimeSpan.FromMilliseconds(50)
                }),
                telemetry);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            string resolved = await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);
            stopwatch.Stop();

            Assert.Equal("https://fast.example.com", resolved);
            Assert.Equal(("https://fast.example.com", CdnResolutionSource.Probe), Assert.Single(telemetry.Resolved));
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10));
        }

        [Fact]
        public async Task ResolveAsync_PublishesRankedCdnUris_FastestFirst()
        {
            SirstrapConfiguration config = new() { RobloxCdnUriOverride = string.Empty };
            CdnResolver resolver = NewResolver(
                config,
                new StaticCandidateProvider(
                [
                    new CdnCandidate("https://slow.example.com", 0),
                    new CdnCandidate("https://fast.example.com", 2),
                    new CdnCandidate("https://dead.example.com", 2)
                ]),
                new FakeCdnProber(new()
                {
                    ["https://slow.example.com"] = TimeSpan.FromMilliseconds(300),
                    ["https://fast.example.com"] = TimeSpan.FromMilliseconds(50),
                    ["https://dead.example.com"] = null
                }),
                new RecordingCdnTelemetry());

            string resolved = await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

            Assert.Equal("https://fast.example.com", resolved);

            DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);

            while (config.ResolvedRobloxCdnUris.Count > 1 && config.ResolvedRobloxCdnUris[1] != "https://slow.example.com" && DateTime.UtcNow < deadline)
                await Task.Delay(20, TestContext.Current.CancellationToken);

            Assert.Equal(["https://fast.example.com", "https://slow.example.com", "https://dead.example.com"], config.ResolvedRobloxCdnUris);
        }

        [Fact]
        public async Task ResolveAsync_PublishesOnlyOverride_WhenOverrideSet()
        {
            SirstrapConfiguration config = new() { RobloxCdnUriOverride = "https://custom.example.com" };
            CdnResolver resolver = NewResolver(config, new StaticCandidateProvider([new CdnCandidate(RobloxCdnService.DefaultBaseUri, 0)]), new FakeCdnProber([]), new RecordingCdnTelemetry());

            await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

            Assert.Equal(["https://custom.example.com"], config.ResolvedRobloxCdnUris);
        }

        [Fact]
        public async Task ResolveAsync_PublishesAllCandidates_WhenAllProbesFail()
        {
            SirstrapConfiguration config = new() { RobloxCdnUriOverride = string.Empty };
            CdnResolver resolver = NewResolver(
                config,
                new StaticCandidateProvider(
                [
                    new CdnCandidate("https://a.example.com", 0),
                    new CdnCandidate("https://b.example.com", 2)
                ]),
                new FakeCdnProber(new()
                {
                    ["https://a.example.com"] = null,
                    ["https://b.example.com"] = null
                }),
                new RecordingCdnTelemetry());

            await resolver.ResolveAsync(NewConfiguration(), TestContext.Current.CancellationToken);

            Assert.Equal(["https://a.example.com", "https://b.example.com"], config.ResolvedRobloxCdnUris);
        }
    }
}
