namespace Sirstrap.Core.Tests.Update
{
    public class ReleaseTagTests
    {
        [Fact]
        public void TryParse_ParsesVersionAndChannel()
        {
            Assert.True(ReleaseTag.TryParse("v1.2.3.4-beta", out var tag));
            Assert.Equal(new Version("1.2.3.4"), tag.Version);
            Assert.Equal("-beta", tag.Channel);
        }

        [Fact]
        public void TryParse_ParsesVersionWithoutChannel()
        {
            Assert.True(ReleaseTag.TryParse("v2.0.0.0", out var tag));
            Assert.Equal(new Version("2.0.0.0"), tag.Version);
            Assert.Equal(string.Empty, tag.Channel);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-a-version-beta")]
        public void TryParse_ReturnsFalse_ForInvalidTags(string? tag)
        {
            Assert.False(ReleaseTag.TryParse(tag, out _));
        }
    }
}
