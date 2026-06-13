namespace Sirstrap.Core.Tests.Cdn
{
    public class CdnUriNormalizerTests
    {
        private readonly CdnUriNormalizer _normalizer = new();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Normalize_ReturnsEmpty_ForBlankInput(string? input)
        {
            Assert.Equal(string.Empty, _normalizer.Normalize(input));
        }

        [Fact]
        public void Normalize_ReturnsEmpty_WhenOnlyTrailingSlashes()
        {
            Assert.Equal(string.Empty, _normalizer.Normalize("///"));
        }

        [Theory]
        [InlineData("setup.rbxcdn.com")]
        [InlineData("ftp://setup.rbxcdn.com")]
        [InlineData("not a uri")]
        public void Normalize_ReturnsEmpty_ForInvalidOrNonHttpUris(string input)
        {
            Assert.Equal(string.Empty, _normalizer.Normalize(input));
        }

        [Fact]
        public void Normalize_TrimsWhitespaceAndTrailingSlashes()
        {
            Assert.Equal("https://setup-ak.rbxcdn.com", _normalizer.Normalize("  https://setup-ak.rbxcdn.com///  "));
        }

        [Fact]
        public void Normalize_StripsQueryAndFragment()
        {
            Assert.Equal("https://setup-aws.rbxcdn.com/path", _normalizer.Normalize("https://setup-aws.rbxcdn.com/path?a=1#frag"));
        }

        [Fact]
        public void Normalize_AcceptsHttpScheme()
        {
            Assert.Equal("http://example.com", _normalizer.Normalize("http://example.com"));
        }
    }
}
