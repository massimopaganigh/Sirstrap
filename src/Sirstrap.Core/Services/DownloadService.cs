namespace Sirstrap.Core.Services
{
    public class DownloadService(Configuration configuration, HttpClient httpClient, IManifestService manifestService, IUriService uriService) : IDownloadService
    {
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
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        [Obsolete]
        public async Task MacDownloadAsync() // TODO: remove Mac support
        {
            try
            {
                var archiveBytes = await httpClient.BetterGetByteArrayAsync(uriService.GetPackageUri(configuration.BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) ? "RobloxPlayer.zip" : "RobloxStudioApp.zip"));

                await File.WriteAllBytesAsync(configuration.DownloadOutput, archiveBytes!);

                Install();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error downloading packages: {0}.", ex.Message);

                throw;
            }
        }

        public async Task WindowsDownloadAsync()
        {
            try
            {
                var manifest = manifestService.GetManifest(await httpClient.BetterGetStringAsync(uriService.GetManifestUri()));

                if (!manifest.IsValid)
                    return;

                using var archive = ZipFile.Open(configuration.DownloadOutput, ZipArchiveMode.Create);

                await GetAppSettingsXml("""<?xml version="1.0" encoding="UTF-8"?><Settings><ContentFolder>content</ContentFolder><BaseUrl>http://www.roblox.com</BaseUrl></Settings>""", "AppSettings.xml", archive);

                var semaphoreSlim = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
                var downloadTasks = manifest.Packages.Select(async package =>
                {
                    await semaphoreSlim.WaitAsync();

                    try
                    {
                        await DownloadAsync(package, archive);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[!] Error downloading {0}: {1}.", package, ex.Message);

                        throw;
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                });

                await Task.WhenAll(downloadTasks);

                Install();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error downloading packages: {0}.", ex.Message);

                throw;
            }
        }

        #region METODI PRIVATI
        private async Task DownloadAsync(string package, ZipArchive archive)
        {
            var packageBytes = await httpClient.BetterGetByteArrayAsync(uriService.GetPackageUri(package));

            await ExtractPackageBytesAsync(packageBytes!, package, archive);
        }

        private async Task ExtractPackageBytesAsync(byte[] packageBytes, string package, ZipArchive archive)
        {
            Dictionary<string, string> roots = GetRoots(package.AsSpan());

            if (roots.TryGetValue(package, out var value))
            {
                using var packageStream = new MemoryStream(packageBytes);
                using var packageArchive = new ZipArchive(packageStream, ZipArchiveMode.Read);

                var archiveEntries = packageArchive.Entries.Where(x => !string.IsNullOrEmpty(x.FullName));

                foreach (var archiveEntry in archiveEntries)
                {
                    await _semaphore.WaitAsync();

                    try
                    {
                        var archiveEntryPath = $"{value}{archiveEntry.FullName.Replace('\\', '/')}";

                        using var sourceStream = archiveEntry.Open();
                        using var targetStream = archive.CreateEntry(archiveEntryPath, CompressionLevel.Fastest).Open();

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
                    using var archiveEntry = archive.CreateEntry(package, CompressionLevel.Fastest).Open();

                    await archiveEntry.WriteAsync(packageBytes);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        private static async Task GetAppSettingsXml(string content, string package, ZipArchive archive)
        {
            using var streamWriter = new StreamWriter(archive.CreateEntry(package, CompressionLevel.Fastest).Open());

            await streamWriter.WriteAsync(content);
        }

        private static Dictionary<string, string> GetRoots(ReadOnlySpan<char> package)
        {
            if (package.Equals("RobloxStudio.zip".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return _studioRoots;

            return _playerRoots;
        }

        private void Install()
        {
            var target = Path.Combine(Directories.VersionsDirectory, configuration.VersionHash);

            target.BetterDirectoryDelete();

            using var archive = ZipFile.OpenRead(configuration.DownloadOutput);

            foreach (var archiveEntry in archive.Entries)
            {
                var archiveEntryPath = Path.GetFullPath(Path.Combine(target, archiveEntry.FullName));

                Path.GetDirectoryName(archiveEntryPath)!.BetterDirectoryCreate();

                if (!string.IsNullOrEmpty(archiveEntry.Name))
                    archiveEntry.ExtractToFile(archiveEntryPath, true);
            }
        }
        #endregion
    }
}
