namespace Sirstrap.Core.Tests.Deployment
{
    public class ManifestTests
    {
        [Fact]
        public void Parse_ReturnsInvalidEmptyManifest_ForNullOrEmpty()
        {
            Manifest fromNull = ManifestParser.Parse(null);
            Manifest fromEmpty = ManifestParser.Parse(string.Empty);

            Assert.False(fromNull.IsValid);
            Assert.Empty(fromNull.Packages);
            Assert.False(fromEmpty.IsValid);
        }

        [Fact]
        public void Parse_MarksValid_WhenFirstLineIsV0_AndCollectsZipPackages()
        {
            Manifest manifest = ManifestParser.Parse("v0\r\nRobloxApp.zip\r\nLibraries.zip\r\nignored-line\r\nchecksum");

            Assert.True(manifest.IsValid);
            Assert.Equal(["RobloxApp.zip", "Libraries.zip"], manifest.Packages);
        }

        [Fact]
        public void Parse_MarksInvalid_WhenFirstLineIsNotV0()
        {
            Manifest manifest = ManifestParser.Parse("v1\nRobloxApp.zip");

            Assert.False(manifest.IsValid);
            Assert.Equal(["RobloxApp.zip"], manifest.Packages);
        }

        [Fact]
        public void Manifest_DefaultsToInvalidWithEmptyPackages()
        {
            Manifest manifest = new();

            Assert.False(manifest.IsValid);
            Assert.Empty(manifest.Packages);
        }
    }
}
