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

        private async Task DownloadPackageAsync(Configuration configuration, string package, ZipArchive archive)
        {
            try
            {
                Log.Information("[*] Downloading package: {0}...", package);

                byte[]? packageBytes = await HttpClientExtension.GetByteArrayAsync(_httpClient, UriBuilder.GetPackageUri(configuration, package));

                await ExtractPackageBytesAsync(packageBytes, package, archive);

                Log.Information("[*] The package has been downloaded successfully: {0}.", package);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] An error occurred while downloading the package: {0}.", package);

                throw;
            }
        }

        private async Task ExtractPackageBytesAsync(byte[]? packageBytes, string package, ZipArchive archive)
        {
            try
            {
                if (packageBytes == null)
                    return;

                Dictionary<string, string> roots = GetRoots(package.AsSpan());

                if (roots.TryGetValue(package, out string? value))
                {
                    using MemoryStream packageStream = new(packageBytes);
                    using ZipArchive packageArchive = new(packageStream, ZipArchiveMode.Read);

                    IEnumerable<ZipArchiveEntry> entries = packageArchive.Entries.Where(x => !string.IsNullOrEmpty(x.FullName));

                    foreach (ZipArchiveEntry entry in entries)
                    {
                        await _semaphore.WaitAsync();

                        try
                        {
                            string entryPath = $"{value}{entry.FullName.Replace('\\', '/')}";

                            using Stream sourceStream = entry.Open();
                            using Stream targetStream = archive.CreateEntry(entryPath, CompressionLevel.Fastest).Open();

                            await sourceStream.CopyToAsync(targetStream);
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
                        using Stream entryStream = archive.CreateEntry(package, CompressionLevel.Fastest).Open();

                        await entryStream.WriteAsync(packageBytes);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static async Task ExtractPackageContentAsync(string content, string package, ZipArchive archive)
        {
            using StreamWriter writer = new(archive.CreateEntry(package, CompressionLevel.Optimal).Open());

            await writer.WriteAsync(content);
        }

        private static Dictionary<string, string> GetRoots(ReadOnlySpan<char> package)
        {
            if (package.Equals("RobloxStudio.zip".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return _studioRoots;

            return _playerRoots;
        }

        public void Dispose() => _semaphore?.Dispose();

        public async Task Download4MacAsync(Configuration configuration)
        {
            try
            {
                string archiveName = configuration.BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) ? "RobloxPlayer.zip" : "RobloxStudioApp.zip";

                Log.Information("[*] Downloading package for Mac: {0}...", archiveName);

                byte[]? archiveBytes = await HttpClientExtension.GetByteArrayAsync(_httpClient, UriBuilder.GetPackageUri(configuration, archiveName));

                if (archiveBytes != null)
                    await File.WriteAllBytesAsync(configuration.GetOutputPath(), archiveBytes);

                Log.Information("[*] The package has been downloaded successfully for Mac: {0}.", archiveName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] An error occurred while downloading the package for Mac.");

                throw;
            }
        }

        public async Task Download4WindowsAsync(Configuration configuration)
        {
            try
            {
                Log.Information("[*] Downloading packages for Windows...");

                Manifest manifest = ManifestParser.Parse(await HttpClientExtension.GetStringAsync(_httpClient, UriBuilder.GetManifestUri(configuration)));

                if (!manifest.IsValid)
                    return;

                using ZipArchive archive = ZipFile.Open(configuration.GetOutputPath(), ZipArchiveMode.Create);

                await ExtractPackageContentAsync(APP_SETTINGS_XML, "AppSettings.xml", archive);

                SemaphoreSlim semaphore = new(Environment.ProcessorCount, Environment.ProcessorCount);

                IEnumerable<Task> downloadTasks = manifest.Packages.Select(async package =>
                {
                    await semaphore.WaitAsync();

                    try
                    {
                        await DownloadPackageAsync(configuration, package, archive);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(downloadTasks);

                Log.Information("[*] All packages have been downloaded successfully for Windows.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] An error occurred while downloading packages for Windows.");

                throw;
            }
        }
    }
}