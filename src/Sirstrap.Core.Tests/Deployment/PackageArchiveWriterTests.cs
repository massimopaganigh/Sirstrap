namespace Sirstrap.Core.Tests.Deployment
{
    public class PackageArchiveWriterTests
    {
        private static async Task<Dictionary<string, string>> WriteAndRead(string outputPath, Func<PackageArchiveWriter, Task> act)
        {
            await using (var archive = await ZipFile.OpenAsync(outputPath, ZipArchiveMode.Create))
            {
                using PackageArchiveWriter writer = new(archive, System.IO.Compression.CompressionLevel.Fastest);

                await act(writer);
            }

            return ZipTestHelper.ReadZip(outputPath);
        }

        [Fact]
        public async Task AddTextEntryAsync_WritesTextEntry()
        {
            using TempDirectory temp = new();
            string output = temp.Combine("out.zip");

            var entries = await WriteAndRead(output, w => w.AddTextEntryAsync("AppSettings.xml", "<Settings/>"));

            Assert.Equal("<Settings/>", entries["AppSettings.xml"]);
        }

        [Fact]
        public async Task AddPackageAsync_WritesRawEntry_ForUnknownPackage()
        {
            using TempDirectory temp = new();
            string output = temp.Combine("out.zip");

            var entries = await WriteAndRead(output, w => w.AddPackageAsync("CustomExtra.zip", System.Text.Encoding.UTF8.GetBytes("payload")));

            Assert.Equal("payload", entries["CustomExtra.zip"]);
        }

        [Fact]
        public async Task AddPackageAsync_ExplodesPackage_UnderCatalogRoot()
        {
            using TempDirectory temp = new();
            string output = temp.Combine("out.zip");

            byte[] inner = ZipTestHelper.CreateZip(("brick.png", "image-bytes"), ("nested/leaf.txt", "leaf"));

            var entries = await WriteAndRead(output, w => w.AddPackageAsync("content-textures2.zip", inner));

            Assert.Equal("image-bytes", entries["content/textures/brick.png"]);
            Assert.Equal("leaf", entries["content/textures/nested/leaf.txt"]);
        }
    }
}
