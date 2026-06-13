namespace Sirstrap.Core.Tests.Deployment
{
    public class PackageRootCatalogTests
    {
        [Fact]
        public void TryGetRoot_ReturnsPlayerRoot_ForKnownPlayerPackage()
        {
            Assert.True(PackageRootCatalog.TryGetRoot("content-textures2.zip", out var root));
            Assert.Equal("content/textures/", root);
        }

        [Fact]
        public void TryGetRoot_ReturnsEmptyRoot_ForRootLevelPackage()
        {
            Assert.True(PackageRootCatalog.TryGetRoot("RobloxApp.zip", out var root));
            Assert.Equal(string.Empty, root);
        }

        [Fact]
        public void TryGetRoot_UsesStudioCatalog_ForRobloxStudioPackage()
        {
            Assert.True(PackageRootCatalog.TryGetRoot("RobloxStudio.zip", out var root));
            Assert.Equal(string.Empty, root);
        }

        [Fact]
        public void TryGetRoot_DoesNotUseStudioCatalog_ForOtherStudioOnlyPackages()
        {
            Assert.False(PackageRootCatalog.TryGetRoot("BuiltInPlugins.zip", out _));
        }

        [Fact]
        public void TryGetRoot_ReturnsFalse_ForUnknownPackage()
        {
            Assert.False(PackageRootCatalog.TryGetRoot("unknown-package.zip", out var root));
            Assert.Null(root);
        }
    }
}
