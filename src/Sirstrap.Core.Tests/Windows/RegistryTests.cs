using Microsoft.Win32;

namespace Sirstrap.Core.Tests.Windows
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416", Justification = "Tests target Windows.")]
    public sealed class RegistryTests : IDisposable
    {
        private readonly string _basePath = $@"Software\SirstrapTests\{Guid.NewGuid():N}";

        public void Dispose()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(_basePath, throwOnMissingSubKey: false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        [Fact]
        public void RegistryOperations_CreateOpenWriteReadDelete_RoundTrips()
        {
            string keyPath = $@"{_basePath}\child";

            using (RegistryKey created = RegistryOperations.CreateRegistrySubKey(Registry.CurrentUser, keyPath, "test key"))
            {
                RegistryOperations.SetRegistryValue(created, "Name", "value");
            }

            Assert.True(RegistryOperations.RegistryKeyExists(Registry.CurrentUser, keyPath));

            using (RegistryKey? opened = RegistryOperations.OpenRegistrySubKey(Registry.CurrentUser, keyPath))
            {
                Assert.NotNull(opened);
                Assert.Equal("value", RegistryOperations.GetRegistryStringValue(opened, "Name"));
            }

            using (RegistryKey writable = RegistryOperations.CreateRegistrySubKey(Registry.CurrentUser, keyPath, "test key"))
            {
                Assert.False(RegistryOperations.SetRegistryStringValueIfDifferent(writable, "Name", "value"));
                Assert.True(RegistryOperations.SetRegistryStringValueIfDifferent(writable, "Name", "other"));
            }

            Assert.Contains("child", RegistryOperations.GetSubKeyNames(RegistryOperations.OpenRegistrySubKey(Registry.CurrentUser, _basePath)!));

            RegistryOperations.DeleteRegistrySubKeyTree(Registry.CurrentUser, keyPath);
            Assert.False(RegistryOperations.RegistryKeyExists(Registry.CurrentUser, keyPath));
        }

        [Fact]
        public void RegistryOperations_GetHiveName_ReturnsKnownNames()
        {
            Assert.Equal("HKEY_CURRENT_USER", RegistryOperations.GetHiveName(Registry.CurrentUser));
            Assert.Equal("HKEY_LOCAL_MACHINE", RegistryOperations.GetHiveName(Registry.LocalMachine));
            Assert.Equal("HKEY_USERS", RegistryOperations.GetHiveName(Registry.Users));
            Assert.Equal("HKEY_CLASSES_ROOT", RegistryOperations.GetHiveName(Registry.ClassesRoot));
            Assert.Equal("HKEY_CURRENT_CONFIG", RegistryOperations.GetHiveName(Registry.CurrentConfig));
        }

        [Fact]
        public void RegistryOperations_Throws_OnInvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(() => RegistryOperations.CreateRegistrySubKey(null!, "x", "d"));
            Assert.Throws<ArgumentException>(() => RegistryOperations.CreateRegistrySubKey(Registry.CurrentUser, " ", "d"));
            Assert.Throws<ArgumentNullException>(() => RegistryOperations.SetRegistryValue(null!, "n", "v"));
            Assert.Throws<ArgumentNullException>(() => RegistryOperations.SetRegistryValue(Registry.CurrentUser, "n", null!));
        }

        [Fact]
        public void RegistryManager_CleanCurrentUserRegistry_DeletesExistingKey()
        {
            string keyPath = $@"{_basePath}\to-clean";
            using (RegistryKey key = RegistryOperations.CreateRegistrySubKey(Registry.CurrentUser, keyPath, "k"))
                RegistryOperations.SetRegistryValue(key, "Marker", "1");

            new RegistryManager().CleanCurrentUserRegistry([keyPath]);

            Assert.False(RegistryOperations.RegistryKeyExists(Registry.CurrentUser, keyPath));
        }

        [Fact]
        public void RegistryManager_DeleteRegistryKey_IsSafe_WhenMissing()
        {
            Assert.Null(Record.Exception(() => new RegistryManager().DeleteRegistryKey(Registry.CurrentUser, $@"{_basePath}\missing")));
        }

        [Fact]
        public void RegistryManager_CleanAllUsersRegistry_DoesNotThrow_ForUnknownKey()
        {
            Assert.Null(Record.Exception(() => new RegistryManager().CleanAllUsersRegistry([$@"{_basePath}\nope"])));
        }
    }
}
