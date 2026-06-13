namespace Sirstrap.Core.Tests.Cdn
{
    public class HttpCdnProberTests
    {
        private static Configuration NewConfiguration() => new()
        {
            BinaryType = "WindowsPlayer",
            ChannelName = "LIVE",
            BlobDirectory = "/",
            VersionHash = "v1"
        };

        [Fact]
        public async Task ProbeAsync_ReturnsResult_AndRecordsSuccess_OnHttpSuccess()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, "ok");
            RecordingCdnTelemetry telemetry = new();
            HttpCdnProber prober = new(client, new FakeCdnProbeUriFactory(), telemetry);
            CdnCandidate candidate = new("https://a.example.com", 0);

            var result = await prober.ProbeAsync(candidate, NewConfiguration(), TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(candidate, result.Candidate);
            Assert.Single(telemetry.Probes);
            Assert.True(telemetry.Probes[0].Success);
        }

        [Fact]
        public async Task ProbeAsync_ReturnsNull_AndRecordsFailure_OnHttpError()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.NotFound, "missing");
            RecordingCdnTelemetry telemetry = new();
            HttpCdnProber prober = new(client, new FakeCdnProbeUriFactory(), telemetry);

            var result = await prober.ProbeAsync(new CdnCandidate("https://a.example.com", 0), NewConfiguration(), TestContext.Current.CancellationToken);

            Assert.Null(result);
            Assert.Single(telemetry.Probes);
            Assert.False(telemetry.Probes[0].Success);
        }

        [Fact]
        public async Task ProbeAsync_ReturnsNull_AndRecordsFailure_OnException()
        {
            HttpClient client = StubHttpMessageHandler.Client(_ => throw new HttpRequestException("boom"));
            RecordingCdnTelemetry telemetry = new();
            HttpCdnProber prober = new(client, new FakeCdnProbeUriFactory(), telemetry);

            var result = await prober.ProbeAsync(new CdnCandidate("https://a.example.com", 0), NewConfiguration(), TestContext.Current.CancellationToken);

            Assert.Null(result);
            Assert.Single(telemetry.Probes);
            Assert.False(telemetry.Probes[0].Success);
        }

        [Fact]
        public async Task ProbeAsync_Throws_OnNullArguments()
        {
            HttpCdnProber prober = new(new HttpClient(), new FakeCdnProbeUriFactory(), new RecordingCdnTelemetry());

            await Assert.ThrowsAsync<ArgumentNullException>(() => prober.ProbeAsync(null!, NewConfiguration(), TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>(() => prober.ProbeAsync(new CdnCandidate("https://a", 0), null!, TestContext.Current.CancellationToken));
        }

        [Fact]
        public void Constructor_Throws_OnNullDependencies()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpCdnProber(null!, new FakeCdnProbeUriFactory(), new RecordingCdnTelemetry()));
            Assert.Throws<ArgumentNullException>(() => new HttpCdnProber(new HttpClient(), null!, new RecordingCdnTelemetry()));
            Assert.Throws<ArgumentNullException>(() => new HttpCdnProber(new HttpClient(), new FakeCdnProbeUriFactory(), null!));
        }
    }
}
