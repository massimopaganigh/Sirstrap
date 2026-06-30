namespace Sirstrap.Core.Tests.Deployment
{
    public class EnumTests
    {
        [Fact]
        public void VersionResolutionSource_HasExpectedMembers()
        {
            Assert.Equal(
                ["Override", "RobloxApi", "SirHurt", "SirHurtFallback", "Weao", "Executor", "Failed"],
                Enum.GetNames<VersionResolutionSource>());
        }
    }
}
