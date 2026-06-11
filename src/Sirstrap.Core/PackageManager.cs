namespace Sirstrap.Core
{
    public class PackageManager(HttpClient httpClient)
    {
        private const string APP_SETTINGS_XML = """<?xml version="1.0" encoding="UTF-8"?><Settings><ContentFolder>content</ContentFolder><BaseUrl>http://www.roblox.com</BaseUrl></Settings>""";

        private static readonly Dictionary<string, string> _playerRoots = new(StringComparer.OrdinalIgnoreCase)
        {
            { "RobloxApp.zip", string.Empty },
            { "Libraries.zip", string.Empty },
            { "redist.zip", string.Empty },
            { "shaders.zip", "shaders/" },
            { "ssl.zip", "ssl/" },
            { "WebView2.zip", string.Empty },
            { "WebView2RuntimeInstaller.zip", "WebView2RuntimeInstaller/" },
            { "content-avatar.zip", "content/avatar/" },
            { "content-configs.zip", "content/configs/" },
            { "content-fonts.zip", "content/fonts/" },
            { "content-sky.zip", "content/sky/" },
            { "content-sounds.zip", "content/sounds/" },
            { "content-textures2.zip", "content/textures/" },
            { "content-models.zip", "content/models/" },
            { "content-textures3.zip", "PlatformContent/pc/textures/" },
            { "content-terrain.zip", "PlatformContent/pc/terrain/" },
            { "content-platform-fonts.zip", "PlatformContent/pc/fonts/" },
            { "content-platform-dictionaries.zip", "PlatformContent/pc/shared_compression_dictionaries/" },
            { "extracontent-luapackages.zip", "ExtraContent/LuaPackages/" },
            { "extracontent-translations.zip", "ExtraContent/translations/" },
            { "extracontent-models.zip", "ExtraContent/models/" },
            { "extracontent-textures.zip", "ExtraContent/textures/" },
            { "extracontent-places.zip", "ExtraContent/places/" }
        };
        private static readonly Dictionary<string, string> _studioRoots = new(StringComparer.OrdinalIgnoreCase)
        {
            { "RobloxStudio.zip", string.Empty },
            { "LibrariesQt5.zip", string.Empty },
            { "Libraries.zip", string.Empty },
            { "content-studio_svg_textures.zip", "content/studio_svg_textures/"},
            { "content-qt_translations.zip", "content/qt_translations/" },
            { "content-api-docs.zip", "content/api_docs/" },
            { "extracontent-scripts.zip", "ExtraContent/scripts/" },
            { "studiocontent-models.zip", "StudioContent/models/" },
            { "studiocontent-textures.zip", "StudioContent/textures/" },
            { "BuiltInPlugins.zip", "BuiltInPlugins/" },
            { "BuiltInStandalonePlugins.zip", "BuiltInStandalonePlugins/" },
            { "ApplicationConfig.zip", "ApplicationConfig/" },
            { "Plugins.zip", "Plugins/" },
            { "Qml.zip", "Qml/" },
            { "StudioFonts.zip", "StudioFonts/" },
            { "RibbonConfig.zip", "RibbonConfig/" },
            { "redist.zip", string.Empty },
            { "shaders.zip", "shaders/" },
            { "ssl.zip", "ssl/" },
            { "WebView2.zip", string.Empty },
            { "WebView2RuntimeInstaller.zip", "WebView2RuntimeInstaller/" },
            { "content-avatar.zip", "content/avatar/" },
            { "content-configs.zip", "content/configs/" },
            { "content-fonts.zip", "content/fonts/" },
            { "content-sky.zip", "content/sky/" },
            { "content-sounds.zip", "content/sounds/" },
            { "content-textures2.zip", "content/textures/" },
            { "content-models.zip", "content/models/" },
            { "content-textures3.zip", "PlatformContent/pc/textures/" },
            { "content-terrain.zip", "PlatformContent/pc/terrain/" },
            { "content-platform-fonts.zip", "PlatformContent/pc/fonts/" },
            { "content-platform-dictionaries.zip", "PlatformContent/pc/shared_compression_dictionaries/" },
            { "extracontent-luapackages.zip", "ExtraContent/LuaPackages/" },
            { "extracontent-translations.zip", "ExtraContent/translations/" },
            { "extracontent-models.zip", "ExtraContent/models/" },
            { "extracontent-textures.zip", "ExtraContent/textures/" },
            { "extracontent-places.zip", "ExtraContent/places/" }
        };

        private readonly HttpClient _httpClient = httpClient;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private async Task<int> DownloadPackageAsync(Configuration configuration, string package, ZipArchive archive)
        {
            try
            {
                Log.Information("[*] Downloading package: {0}...", package);

                byte[]? packageBytes = await HttpClientExtension.GetByteArrayAsync(_httpClient, UriBuilder.GetPackageUri(configuration, package))
                    ?? throw new InvalidOperationException($"No bytes were downloaded for the package: {package}.");

                int byteCount = packageBytes.Length;

                await ExtractPackageBytesAsync(packageBytes, package, archive, GetEntryCompressionLevel(configuration));

                Log.Information("[*] The package has been downloaded successfully: {0}.", package);

                return byteCount;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] An error occurred while downloading the package: {0}.", package);

                throw new InvalidOperationException($"An error occurred while downloading the package: {package}.", ex);
            }
        }

        private async Task ExtractPackageBytesAsync(byte[] packageBytes, string package, ZipArchive archive, CompressionLevel compressionLevel)
        {
            Dictionary<string, string> roots = GetRoots(package.AsSpan());

            if (roots.TryGetValue(package, out string? value))
            {
                using MemoryStream packageStream = new(packageBytes);
                using ZipArchive packageArchive = new(packageStream, ZipArchiveMode.Read);

                IEnumerable<ZipArchiveEntry> entries = packageArchive.Entries.Where(x => !string.IsNullOrEmpty(x.FullName));

                foreach (ZipArchiveEntry entry in entries)
                {
                    string entryPath = $"{value}{entry.FullName.Replace('\\', '/')}";

                    // Inflate outside the semaphore so packages decompress in parallel;
                    // the lock only guards writes to the shared output archive.
                    byte[] entryBytes = await ReadEntryBytesAsync(entry);

                    await _semaphore.WaitAsync();

                    try
                    {
                        ZipArchiveEntry targetEntry = archive.CreateEntry(entryPath, compressionLevel);

                        await using Stream targetStream = await targetEntry.OpenAsync();

                        await targetStream.WriteAsync(entryBytes);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            }
            else
            {
                await _semaphore.WaitAsync();

                try
                {
                    ZipArchiveEntry entry = archive.CreateEntry(package, compressionLevel);

                    await using Stream entryStream = await entry.OpenAsync();

                    await entryStream.WriteAsync(packageBytes);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        private static async Task<byte[]> ReadEntryBytesAsync(ZipArchiveEntry entry)
        {
            byte[] entryBytes = new byte[checked((int)entry.Length)];

            await using Stream sourceStream = await entry.OpenAsync();

            await sourceStream.ReadExactlyAsync(entryBytes);

            return entryBytes;
        }

        // The Windows Player archive is extracted and deleted right away by Installer.Install,
        // so compressing it only burns CPU under the archive lock; other binaries keep the
        // archive as the final artifact and stay compressed.
        private static CompressionLevel GetEntryCompressionLevel(Configuration configuration)
            => configuration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase)
                ? CompressionLevel.NoCompression
                : CompressionLevel.Fastest;

        private static async Task ExtractPackageContentAsync(string content, string package, ZipArchive archive)
        {
            ZipArchiveEntry entry = archive.CreateEntry(package, CompressionLevel.Optimal);

            await using StreamWriter writer = new(await entry.OpenAsync());

            await writer.WriteAsync(content);
        }

        private static Dictionary<string, string> GetRoots(ReadOnlySpan<char> package)
        {
            if (package.Equals("RobloxStudio.zip".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return _studioRoots;

            return _playerRoots;
        }

        public void DisposeSemaphore() => _semaphore?.Dispose();

        public async Task Download4MacAsync(Configuration configuration)
        {
            string archiveName = configuration.BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) ? "RobloxPlayer.zip" : "RobloxStudioApp.zip";

            using ITelemetryScope scope = Telemetry.Performance.Measure("packages.download.mac", new Dictionary<string, object>
            {
                ["archive"] = archiveName
            });

            try
            {
                Log.Information("[*] Downloading package for Mac: {0}...", archiveName);

                byte[]? archiveBytes = await HttpClientExtension.GetByteArrayAsync(_httpClient, UriBuilder.GetPackageUri(configuration, archiveName))
                    ?? throw new InvalidOperationException($"No bytes were downloaded for the package for Mac: {archiveName}.");

                int byteCount = archiveBytes.Length;

                await File.WriteAllBytesAsync(configuration.GetOutputPath(), archiveBytes);

                Log.Information("[*] The package has been downloaded successfully for Mac: {0}.", archiveName);

                Telemetry.Performance.RecordCounter("packages.download.mac.bytes", new Dictionary<string, object>
                {
                    ["bytes"] = byteCount,
                    ["archive"] = archiveName
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] An error occurred while downloading the package for Mac.");

                scope.MarkFailed();

                throw new InvalidOperationException("An error occurred while downloading the package for Mac.", ex);
            }
        }

        public async Task Download4WindowsAsync(Configuration configuration)
        {
            using ITelemetryScope scope = Telemetry.Performance.Measure("packages.download.windows");

            try
            {
                Log.Information("[*] Downloading packages for Windows...");

                Manifest manifest = ManifestParser.Parse(await HttpClientExtension.GetStringAsync(_httpClient, UriBuilder.GetManifestUri(configuration)));

                if (!manifest.IsValid)
                {
                    scope.MarkFailed();

                    Telemetry.Performance.RecordCounter("packages.download.windows.manifest_invalid");

                    return;
                }

                int packageCount = manifest.Packages.Count;

                scope.SetTag("packageCount", packageCount.ToString());

                Telemetry.Performance.RecordCounter("packages.download.windows.manifest", new Dictionary<string, object>
                {
                    ["packageCount"] = packageCount
                });

                string outputPath = configuration.GetOutputPath();

                outputPath.BetterFileDelete();

                using ZipArchive archive = await ZipFile.OpenAsync(outputPath, ZipArchiveMode.Create);

                await ExtractPackageContentAsync(APP_SETTINGS_XML, "AppSettings.xml", archive);

                // Downloads are network-bound, so don't let low core counts cap the parallelism.
                int downloadConcurrency = Math.Max(Environment.ProcessorCount, 8);

                using SemaphoreSlim semaphore = new(downloadConcurrency, downloadConcurrency);
                long totalBytes = 0;

                IEnumerable<Task> downloadTasks = manifest.Packages.Select(async package =>
                {
                    await semaphore.WaitAsync();

                    try
                    {
                        int bytes = await DownloadPackageAsync(configuration, package, archive);

                        Interlocked.Add(ref totalBytes, bytes);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(downloadTasks);

                Log.Information("[*] All packages have been downloaded successfully for Windows.");

                Telemetry.Performance.RecordCounter("packages.download.windows.bytes", new Dictionary<string, object>
                {
                    ["bytes"] = totalBytes,
                    ["packageCount"] = packageCount
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] An error occurred while downloading packages for Windows.");

                scope.MarkFailed();

                throw new InvalidOperationException("An error occurred while downloading packages for Windows.", ex);
            }
        }
    }
}
