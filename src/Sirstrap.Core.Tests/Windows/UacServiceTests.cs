namespace Sirstrap.Core.Tests.Windows
{
    public class UacServiceTests
    {
        [Fact]
        public void IsRunningAsAdministrator_ReturnsWithoutThrowing()
        {
            UacService service = new();

            Assert.Null(Record.Exception(() => service.IsRunningAsAdministrator()));
        }

        [Fact]
        public void EnsureAdministratorPrivileges_ReturnsTrue_WhenOperationSucceeds()
        {
            UacService service = new();

            Assert.True(service.EnsureAdministratorPrivileges(() => true, [], "operation"));
        }

        [Fact]
        public void EnsureAdministratorPrivileges_ReturnsFalse_WhenOperationThrows()
        {
            UacService service = new();

            Assert.False(service.EnsureAdministratorPrivileges(() => throw new InvalidOperationException(), [], "operation"));
        }
    }
}
