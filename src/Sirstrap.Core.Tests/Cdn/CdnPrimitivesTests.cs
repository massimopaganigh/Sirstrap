namespace Sirstrap.Core.Tests.Cdn
{
    public class CdnPrimitivesTests
    {
        [Fact]
        public void CdnCandidate_StoresBaseUriAndPriority()
        {
            CdnCandidate candidate = new("https://a.example.com", 3);

            Assert.Equal("https://a.example.com", candidate.BaseUri);
            Assert.Equal(3, candidate.FallbackPriority);
        }

        [Fact]
        public void CdnProbeResult_StoresCandidateAndElapsed()
        {
            CdnCandidate candidate = new("https://a.example.com", 0);
            CdnProbeResult result = new(candidate, TimeSpan.FromMilliseconds(42));

            Assert.Same(candidate, result.Candidate);
            Assert.Equal(TimeSpan.FromMilliseconds(42), result.Elapsed);
        }

        [Theory]
        [InlineData(CdnResolutionSource.Override)]
        [InlineData(CdnResolutionSource.Probe)]
        [InlineData(CdnResolutionSource.Fallback)]
        public void CdnResolutionSource_DefinesExpectedValues(CdnResolutionSource source)
        {
            Assert.Contains(source, Enum.GetValues<CdnResolutionSource>());
        }

        [Fact]
        public void DefaultCdnCandidateProvider_IncludesDefaultBaseUri_AndIsStable()
        {
            DefaultCdnCandidateProvider provider = new();

            var candidates = provider.GetCandidates();

            Assert.NotEmpty(candidates);
            Assert.Contains(candidates, c => c.BaseUri == RobloxCdnService.DefaultBaseUri);
            Assert.Same(candidates, provider.GetCandidates());
        }

        [Fact]
        public void RobloxCdnService_NormalizeCdnUriOverride_DelegatesToNormalizer()
        {
            Assert.Equal("https://setup-ak.rbxcdn.com", RobloxCdnService.NormalizeCdnUriOverride("  https://setup-ak.rbxcdn.com/  "));
            Assert.Equal(string.Empty, RobloxCdnService.NormalizeCdnUriOverride(null));
            Assert.Equal(RobloxCdnService.DefaultBaseUri, RobloxCdnService.NormalizeCdnUriOverride(RobloxCdnService.DefaultBaseUri));
        }
    }
}
