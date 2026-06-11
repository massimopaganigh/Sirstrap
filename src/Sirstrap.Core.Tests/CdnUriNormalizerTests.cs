namespace Sirstrap.Core.Tests
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

        [Theory]
        [InlineData("setup.rbxcdn.com")]
        [InlineData("ftp://setup.rbxcdn.com")]
        [InlineData("file:///x")]
        [InlineData("javascript:void(0)")]
        [InlineData("https://")]
        public void Normalize_ReturnsEmpty_ForInvalidInput(string input)
        {
            Assert.Equal(string.Empty, _normalizer.Normalize(input));
        }

        [Theory]
        [InlineData("https://setup.rbxcdn.com", "https://setup.rbxcdn.com")]
        [InlineData("  https://setup-ak.rbxcdn.com///  ", "https://setup-ak.rbxcdn.com")]
        [InlineData("http://example.com/path/", "http://example.com/path")]
        [InlineData("https://example.com/path?q=1#frag", "https://example.com/path")]
        public void Normalize_ReturnsCanonicalForm_ForValidInput(string input, string expected)
        {
            Assert.Equal(expected, _normalizer.Normalize(input));
        }
    }
}
