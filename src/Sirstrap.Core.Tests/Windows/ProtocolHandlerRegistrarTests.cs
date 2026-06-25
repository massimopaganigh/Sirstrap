using Microsoft.Win32;

namespace Sirstrap.Core.Tests.Windows
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416", Justification = "Tests target Windows.")]
    public sealed class ProtocolHandlerRegistrarTests : IDisposable
    {
        private readonly string _protocol = $"sirstraptest-{Guid.NewGuid():N}";

        public void Dispose()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{_protocol}", throwOnMissingSubKey: false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        [Fact]
        public void RegisterProtocolHandler_WritesProtocolKey_AndUnregisterRemovesIt()
        {
            ProtocolHandlerRegistrar registrar = new(new FakeUacService());

            Assert.True(registrar.RegisterProtocolHandler(_protocol, []));
            Assert.True(RegistryOperations.RegistryKeyExists(Registry.CurrentUser, $@"Software\Classes\{_protocol}\shell\open\command"));

            Assert.True(registrar.RegisterProtocolHandler(_protocol, []));

            registrar.UnregisterProtocolHandler(_protocol);
            Assert.False(RegistryOperations.RegistryKeyExists(Registry.CurrentUser, $@"Software\Classes\{_protocol}"));
        }

        [Fact]
        public void UnregisterProtocolHandlers_RemovesEach()
        {
            ProtocolHandlerRegistrar registrar = new(new FakeUacService());
            registrar.RegisterProtocolHandler(_protocol, []);

            registrar.UnregisterProtocolHandlers([_protocol]);

            Assert.False(RegistryOperations.RegistryKeyExists(Registry.CurrentUser, $@"Software\Classes\{_protocol}"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("invalid protocol!")]
        public void RegisterProtocolHandler_Throws_OnInvalidName(string protocol)
        {
            ProtocolHandlerRegistrar registrar = new(new FakeUacService());

            Assert.Throws<ArgumentException>(() => registrar.RegisterProtocolHandler(protocol, []));
        }

        [Fact]
        public void RegisterProtocolHandler_Overwrites_WhenRegisteredWithDifferentHandler()
        {
            using (RegistryKey command = RegistryOperations.CreateRegistrySubKey(Registry.CurrentUser, $@"Software\Classes\{_protocol}\shell\open\command", "seed"))
                RegistryOperations.SetRegistryValue(command, string.Empty, "some-other-handler \"%1\"");

            ProtocolHandlerRegistrar registrar = new(new FakeUacService());

            Assert.True(registrar.RegisterProtocolHandler(_protocol, []));

            using RegistryKey? command2 = RegistryOperations.OpenRegistrySubKey(Registry.CurrentUser, $@"Software\Classes\{_protocol}\shell\open\command");
            Assert.Contains(AppDomain.CurrentDomain.FriendlyName, RegistryOperations.GetRegistryStringValue(command2!, string.Empty));
        }

        [Fact]
        public void UnregisterProtocolHandler_Throws_OnInvalidName()
        {
            ProtocolHandlerRegistrar registrar = new(new FakeUacService());

            Assert.Throws<ArgumentException>(() => registrar.UnregisterProtocolHandler(" "));
        }
    }
}
