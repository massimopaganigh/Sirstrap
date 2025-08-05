using Sirstrap.Core.Extensions;
using Sirstrap.Core.Interfaces;
using Sirstrap.Core.Models;

namespace Sirstrap.Core.Services
{
    public class RobloxDownloadService(RobloxDownloadConfiguration robloxDownloadConfiguration) : IRobloxDownloadService
    {
        #region Properties
        private static readonly Dictionary<string, string> _playerRoots = new(StringComparer.OrdinalIgnoreCase) { { "RobloxApp.zip", string.Empty }, { "redist.zip", string.Empty }, { "shaders.zip", "shaders/" }, { "ssl.zip", "ssl/" }, { "WebView2.zip", string.Empty }, { "WebView2RuntimeInstaller.zip", "WebView2RuntimeInstaller/" }, { "content-avatar.zip", "content/avatar/" }, { "content-configs.zip", "content/configs/" }, { "content-fonts.zip", "content/fonts/" }, { "content-sky.zip", "content/sky/" }, { "content-sounds.zip", "content/sounds/" }, { "content-textures2.zip", "content/textures/" }, { "content-models.zip", "content/models/" }, { "content-platform-fonts.zip", "PlatformContent/pc/fonts/" }, { "content-platform-dictionaries.zip", "PlatformContent/pc/shared_compression_dictionaries/" }, { "content-terrain.zip", "PlatformContent/pc/terrain/" }, { "content-textures3.zip", "PlatformContent/pc/textures/" }, { "extracontent-luapackages.zip", "ExtraContent/LuaPackages/" }, { "extracontent-translations.zip", "ExtraContent/translations/" }, { "extracontent-models.zip", "ExtraContent/models/" }, { "extracontent-textures.zip", "ExtraContent/textures/" }, { "extracontent-places.zip", "ExtraContent/places/" } };
        private static readonly Dictionary<string, string> _studioRoots = new(StringComparer.OrdinalIgnoreCase) { { "RobloxStudio.zip", string.Empty }, { "RibbonConfig.zip", "RibbonConfig/" }, { "redist.zip", string.Empty }, { "Libraries.zip", string.Empty }, { "LibrariesQt5.zip", string.Empty }, { "WebView2.zip", string.Empty }, { "WebView2RuntimeInstaller.zip", string.Empty }, { "shaders.zip", "shaders/" }, { "ssl.zip", "ssl/" }, { "Qml.zip", "Qml/" }, { "Plugins.zip", "Plugins/" }, { "StudioFonts.zip", "StudioFonts/" }, { "BuiltInPlugins.zip", "BuiltInPlugins/" }, { "ApplicationConfig.zip", "ApplicationConfig/" }, { "BuiltInStandalonePlugins.zip", "BuiltInStandalonePlugins/" }, { "content-qt_translations.zip", "content/qt_translations/" }, { "content-sky.zip", "content/sky/" }, { "content-fonts.zip", "content/fonts/" }, { "content-avatar.zip", "content/avatar/" }, { "content-models.zip", "content/models/" }, { "content-sounds.zip", "content/sounds/" }, { "content-configs.zip", "content/configs/" }, { "content-api-docs.zip", "content/api_docs/" }, { "content-textures2.zip", "content/textures/" }, { "content-studio_svg_textures.zip", "content/studio_svg_textures/" }, { "content-platform-fonts.zip", "PlatformContent/pc/fonts/" }, { "content-platform-dictionaries.zip", "PlatformContent/pc/shared_compression_dictionaries/" }, { "content-terrain.zip", "PlatformContent/pc/terrain/" }, { "content-textures3.zip", "PlatformContent/pc/textures/" }, { "extracontent-translations.zip", "ExtraContent/translations/" }, { "extracontent-luapackages.zip", "ExtraContent/LuaPackages/" }, { "extracontent-textures.zip", "ExtraContent/textures/" }, { "extracontent-scripts.zip", "ExtraContent/scripts/" }, { "extracontent-models.zip", "ExtraContent/models/" } };
        private readonly HttpClient _httpClient = new();
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        #endregion

        #region Private Methods
        private async Task DownloadPackageByteArrayAsync(string packageName, ZipArchive zipArchive)
        {
            try
            {
                Log.Information("[*] Downloading package: {0}...", packageName);

                await ExtractPackageByteArrayAsync(await HttpClientExtension.GetByteArrayAsync(_httpClient, UriBuilder.GetPackageUri(robloxDownloadConfiguration, packageName)), packageName, zipArchive);

                Log.Information("[*] The package has been downloaded successfully: {0}.", packageName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] An error occurred while downloading the package: {0}.", packageName);

                throw;
            }
        }

        private async Task ExtractPackageByteArrayAsync(byte[]? packageByteArray, string packageName, ZipArchive zipArchive)
        {
            try
            {
                if (packageByteArray == null)
                    return;

                Dictionary<string, string> roots = GetRoots(packageName.AsSpan());

                if (roots.TryGetValue(packageName, out string? value))
                {
                    using MemoryStream memoryStream = new(packageByteArray);
                    using ZipArchive zipArchive2 = new(memoryStream, ZipArchiveMode.Read);

                    IEnumerable<ZipArchiveEntry> zipArchive2EntryIEnumerable = zipArchive2.Entries.Where(x => !string.IsNullOrEmpty(x.FullName));

                    foreach (ZipArchiveEntry zipArchive2Entry in zipArchive2EntryIEnumerable)
                    {
                        await _semaphoreSlim.WaitAsync();

                        try
                        {
                            using Stream stream2 = zipArchive2Entry.Open();
                            using Stream stream = zipArchive.CreateEntry($"{value}{zipArchive2Entry.FullName.Replace('\\', '/')}", CompressionLevel.Fastest).Open();

                            await stream2.CopyToAsync(stream);
                        }
                        finally
                        {
                            _semaphoreSlim.Release();
                        }
                    }
                }
                else
                {
                    await _semaphoreSlim.WaitAsync();

                    try
                    {
                        using Stream stream = zipArchive.CreateEntry(packageName, CompressionLevel.Fastest).Open();

                        await stream.WriteAsync(packageByteArray);
                    }
                    finally
                    {
                        _semaphoreSlim.Release();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static async Task ExtractPackageContentAsync(string packageContent, string packageName, ZipArchive zipArchive)
        {
            using StreamWriter streamWriter = new(zipArchive.CreateEntry(packageName, CompressionLevel.Optimal).Open());

            await streamWriter.WriteAsync(packageContent);
        }

        private static Dictionary<string, string> GetRoots(ReadOnlySpan<char> packageName)
        {
            if (packageName.Equals("RobloxStudio.zip".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return _studioRoots;

            return _playerRoots;
        }
        #endregion

        public async Task DownloadForMacAsync()
        {
            try
            {
                Log.Information("[*] Downloading packages for Mac...");

                await File.WriteAllBytesAsync(robloxDownloadConfiguration.OutputPath, (await HttpClientExtension.GetByteArrayAsync(_httpClient, UriBuilder.GetPackageUri(robloxDownloadConfiguration, robloxDownloadConfiguration.BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) ? "RobloxPlayer.zip" : "RobloxStudioApp.zip")))!);

                Log.Information("[*] All packages have been downloaded successfully for Mac.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] An error occurred while downloading packages for Mac.");

                throw;
            }
        }

        public async Task DownloadForWindowsAsync()
        {
            try
            {
                Log.Information("[*] Downloading packages for Windows...");

                Manifest manifest = ManifestParser.Parse(await HttpClientExtension.GetStringAsync(_httpClient, UriBuilder.GetManifestUri(robloxDownloadConfiguration)));

                if (!manifest.IsValid)
                    return;

                using ZipArchive zipArchive = ZipFile.Open(robloxDownloadConfiguration.OutputPath, ZipArchiveMode.Create);

                await ExtractPackageContentAsync("""<?xml version="1.0" encoding="UTF-8"?><Settings><ContentFolder>content</ContentFolder><BaseUrl>http://www.roblox.com</BaseUrl></Settings>""", "AppSettings.xml", zipArchive);

                SemaphoreSlim semaphoreSlim = new(Environment.ProcessorCount, Environment.ProcessorCount);

                IEnumerable<Task> downloadTasks = manifest.Packages.Select(async packageName =>
                {
                    await semaphoreSlim.WaitAsync();

                    try
                    {
                        await DownloadPackageByteArrayAsync(packageName, zipArchive);
                    }
                    finally
                    {
                        semaphoreSlim.Release();
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