namespace Sirstrap.Core.Tests.Windows
{
    public class UninstallServiceTests
    {
        [Fact]
        public void UnregisterProtocols_DelegatesToRegistrar_ForRobloxPlayer()
        {
            FakeProtocolHandlerRegistrar registrar = new();
            UninstallService service = new(registrar);

            service.UnregisterProtocols();

            Assert.Contains("roblox-player", registrar.Unregistered);
        }
    }
}
