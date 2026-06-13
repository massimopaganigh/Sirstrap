using Microsoft.Win32;

namespace Sirstrap.Core.Tests.Windows
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416", Justification = "Tests target Windows.")]
    public sealed class RegistryGapTests : IDisposable
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
        public void CleanRegistryKeys_DeletesEachProvidedKey()
        {
            string keyPath = $@"{_basePath}\bulk";
            using (RegistryKey key = RegistryOperations.CreateRegistrySubKey(Registry.CurrentUser, keyPath, "k"))
                RegistryOperations.SetRegistryValue(key, null, "default");

            new RegistryManager().CleanRegistryKeys(Registry.CurrentUser, [keyPath]);

            Assert.False(RegistryOperations.RegistryKeyExists(Registry.CurrentUser, keyPath));
        }

        [Fact]
        public void CleanLocalMachineRegistry_IsSafe_ForNonExistentKey()
        {
            Assert.Null(Record.Exception(() => new RegistryManager().CleanLocalMachineRegistry([$@"{_basePath}\nope"])));
        }

        [Fact]
        public void GetRegistryStringValue_ReadsDefaultValue()
        {
            string keyPath = $@"{_basePath}\defaultval";
            using RegistryKey key = RegistryOperations.CreateRegistrySubKey(Registry.CurrentUser, keyPath, "k");
            RegistryOperations.SetRegistryValue(key, null, "the-default");

            Assert.Equal("the-default", RegistryOperations.GetRegistryStringValue(key, null));
        }

        [Fact]
        public void SetRegistryStringValueIfDifferent_WritesWhenCurrentMissing()
        {
            string keyPath = $@"{_basePath}\ifdiff";
            using RegistryKey key = RegistryOperations.CreateRegistrySubKey(Registry.CurrentUser, keyPath, "k");

            Assert.True(RegistryOperations.SetRegistryStringValueIfDifferent(key, "Name", "first"));
            Assert.False(RegistryOperations.SetRegistryStringValueIfDifferent(key, "Name", "first"));
        }

        [Fact]
        public void OpenRegistrySubKey_ReturnsNull_ForMissingKey()
        {
            Assert.Null(RegistryOperations.OpenRegistrySubKey(Registry.CurrentUser, $@"{_basePath}\absent"));
        }

        [Fact]
        public void RegistryOperations_Throws_OnNullArgumentsForReads()
        {
            Assert.Throws<ArgumentNullException>(() => RegistryOperations.GetSubKeyNames(null!));
            Assert.Throws<ArgumentNullException>(() => RegistryOperations.GetRegistryStringValue(null!, "n"));
            Assert.Throws<ArgumentNullException>(() => RegistryOperations.RegistryKeyExists(null!, "p"));
            Assert.Throws<ArgumentNullException>(() => RegistryOperations.DeleteRegistrySubKeyTree(null!, "p"));
            Assert.Throws<ArgumentNullException>(() => RegistryOperations.GetHiveName(null!));
        }
    }
}
