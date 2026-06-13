namespace Sirstrap.Core.Deployment
{
    public sealed class PackageArchiveWriter(ZipArchive archive, CompressionLevel compressionLevel) : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public void Dispose() => _semaphore.Dispose();

        public async Task AddPackageAsync(string package, byte[] packageBytes)
        {
            if (PackageRootCatalog.TryGetRoot(package, out string root))
                await AddExplodedPackageAsync(root, packageBytes);
            else
                await WriteEntryAsync(package, packageBytes);
        }

        public async Task AddTextEntryAsync(string entryPath, string content)
        {
            await _semaphore.WaitAsync();

            try
            {
                ZipArchiveEntry entry = archive.CreateEntry(entryPath, CompressionLevel.Optimal);

                await using StreamWriter writer = new(await entry.OpenAsync());

                await writer.WriteAsync(content);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task AddExplodedPackageAsync(string root, byte[] packageBytes)
        {
            using MemoryStream packageStream = new(packageBytes);
            using ZipArchive packageArchive = new(packageStream, ZipArchiveMode.Read);

            IEnumerable<ZipArchiveEntry> entries = packageArchive.Entries.Where(x => !string.IsNullOrEmpty(x.FullName));

            foreach (ZipArchiveEntry entry in entries)
            {
                string entryPath = $"{root}{entry.FullName.Replace('\\', '/')}";
                byte[] entryBytes = await ReadEntryBytesAsync(entry);

                await WriteEntryAsync(entryPath, entryBytes);
            }
        }

        private static async Task<byte[]> ReadEntryBytesAsync(ZipArchiveEntry entry)
        {
            byte[] entryBytes = new byte[checked((int)entry.Length)];

            await using Stream sourceStream = await entry.OpenAsync();

            await sourceStream.ReadExactlyAsync(entryBytes);

            return entryBytes;
        }

        private async Task WriteEntryAsync(string entryPath, byte[] entryBytes)
        {
            await _semaphore.WaitAsync();

            try
            {
                ZipArchiveEntry entry = archive.CreateEntry(entryPath, compressionLevel);

                await using Stream entryStream = await entry.OpenAsync();

                await entryStream.WriteAsync(entryBytes);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
