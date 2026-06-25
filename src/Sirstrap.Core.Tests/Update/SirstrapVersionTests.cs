namespace Sirstrap.Core.Tests.Update
{
    public class SirstrapVersionTests
    {
        [Fact]
        public void Channel_ReflectsConfiguration()
        {
            SirstrapConfiguration config = new() { ChannelName = "-beta" };
            SirstrapVersion version = new(config);

            Assert.Equal("-beta", version.Channel);
        }

        [Fact]
        public void Current_ReturnsParsedAssemblyVersion()
        {
            SirstrapVersion version = new(new SirstrapConfiguration());

            Assert.NotNull(version.Current);
            Assert.True(version.Current.Major >= 0);
        }

        [Fact]
        public void GetFullVersion_StartsWithV()
        {
            SirstrapVersion version = new(new SirstrapConfiguration());

            Assert.StartsWith("v", version.GetFullVersion());
        }
    }
}
