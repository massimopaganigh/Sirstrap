namespace Sirstrap.Core.Tests.Deployment
{
    public class EnumTests
    {
        [Fact]
        public void VersionResolutionSource_HasExpectedMembers()
        {
            Assert.Equal(
                ["Override", "RobloxApi", "SirHurt", "SirHurtFallback", "Failed"],
                Enum.GetNames<VersionResolutionSource>());
        }
    }
}
