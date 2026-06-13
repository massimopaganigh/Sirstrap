namespace Sirstrap.Core.Deployment
{
    public sealed class PackageManager(HttpClient httpClient, IRobloxUriFactory robloxUriFactory, IPathManager pathManager, IPerformanceTelemetry performanceTelemetry) : IPackageManager
    {
        private const string APP_SETTINGS_XML = """<?xml version="1.0" encoding="UTF-8"?><Settings><ContentFolder>content</ContentFolder><BaseUrl>http://www.roblox.com</BaseUrl></Settings>""";

        public async Task DownloadMacArchiveAsync(Configuration configuration)
        {
            string archiveName = configuration.IsMacPlayer() ? "RobloxPlayer.zip" : "RobloxStudioApp.zip";

            using ITelemetryScope scope = performanceTelemetry.Measure("packages.download.mac", new Dictionary<string, object>
            {
                ["archive"] = archiveName
            });

            try
            {
                Log.Information("[*] Downloading the Mac archive {ArchiveName}...", archiveName);

                byte[]? archiveBytes = await HttpClientExtension.GetByteArrayAsync(httpClient, robloxUriFactory.GetPackageUri(configuration, archiveName))
                    ?? throw new InvalidOperationException($"No bytes were downloaded for the package for Mac: {archiveName}.");

                int byteCount = archiveBytes.Length;

                await File.WriteAllBytesAsync(pathManager.GetOutputPath(configuration), archiveBytes);

                Log.Information("[*] Downloaded the Mac archive {ArchiveName}.", archiveName);

                performanceTelemetry.RecordCounter("packages.download.mac.bytes", new Dictionary<string, object>
                {
                    ["bytes"] = byteCount,
                    ["archive"] = archiveName
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to download the Mac archive {ArchiveName}.", archiveName);

                scope.MarkFailed();

                throw new InvalidOperationException("An error occurred while downloading the package for Mac.", ex);
            }
        }

        public async Task DownloadWindowsArchiveAsync(Configuration configuration)
        {
            using ITelemetryScope scope = performanceTelemetry.Measure("packages.download.windows");

            try
            {
                Log.Information("[*] Downloading the Windows packages...");

                Manifest manifest = ManifestParser.Parse(await HttpClientExtension.GetStringAsync(httpClient, robloxUriFactory.GetManifestUri(configuration)));

                if (!manifest.IsValid)
                {
                    scope.MarkFailed();

                    performanceTelemetry.RecordCounter("packages.download.windows.manifest_invalid");

                    return;
                }

                int packageCount = manifest.Packages.Count;

                scope.SetTag("packageCount", packageCount.ToString());

                performanceTelemetry.RecordCounter("packages.download.windows.manifest", new Dictionary<string, object>
                {
                    ["packageCount"] = packageCount
                });

                string outputPath = pathManager.GetOutputPath(configuration);

                FileSystemOperations.DeleteFile(outputPath);

                using ZipArchive archive = await ZipFile.OpenAsync(outputPath, ZipArchiveMode.Create);
                using PackageArchiveWriter archiveWriter = new(archive, GetEntryCompressionLevel(configuration));

                await archiveWriter.AddTextEntryAsync("AppSettings.xml", APP_SETTINGS_XML);

                long totalBytes = await DownloadPackagesAsync(configuration, manifest.Packages, archiveWriter);

                Log.Information("[*] Downloaded all the Windows packages.");

                performanceTelemetry.RecordCounter("packages.download.windows.bytes", new Dictionary<string, object>
                {
                    ["bytes"] = totalBytes,
                    ["packageCount"] = packageCount
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to download the Windows packages.");

                scope.MarkFailed();

                throw new InvalidOperationException("An error occurred while downloading packages for Windows.", ex);
            }
        }

        private async Task<int> DownloadPackageAsync(Configuration configuration, string package, PackageArchiveWriter archiveWriter)
        {
            try
            {
                Log.Information("[*] Downloading the package {Package}...", package);

                byte[]? packageBytes = await HttpClientExtension.GetByteArrayAsync(httpClient, robloxUriFactory.GetPackageUri(configuration, package))
                    ?? throw new InvalidOperationException($"No bytes were downloaded for the package: {package}.");

                int byteCount = packageBytes.Length;

                await archiveWriter.AddPackageAsync(package, packageBytes);

                Log.Information("[*] Downloaded the package {Package}.", package);

                return byteCount;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to download the package {Package}.", package);

                throw new InvalidOperationException($"An error occurred while downloading the package: {package}.", ex);
            }
        }

        private async Task<long> DownloadPackagesAsync(Configuration configuration, IReadOnlyList<string> packages, PackageArchiveWriter archiveWriter)
        {
            int downloadConcurrency = Math.Max(Environment.ProcessorCount, 8);

            using SemaphoreSlim semaphore = new(downloadConcurrency, downloadConcurrency);
            long totalBytes = 0;

            IEnumerable<Task> downloadTasks = packages.Select(async package =>
            {
                await semaphore.WaitAsync();

                try
                {
                    int bytes = await DownloadPackageAsync(configuration, package, archiveWriter);

                    Interlocked.Add(ref totalBytes, bytes);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(downloadTasks);

            return totalBytes;
        }

        private static CompressionLevel GetEntryCompressionLevel(Configuration configuration)
            => configuration.IsWindowsPlayer()
                ? CompressionLevel.NoCompression
                : CompressionLevel.Fastest;
    }
}
