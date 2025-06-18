namespace Sirstrap.Core
{
    public class PackageExtractor
    {
        private readonly Lock _lock = new();
        private readonly Dictionary<string, string> _playerExtractionRoots = new(StringComparer.OrdinalIgnoreCase)
        {
            { "RobloxApp.zip", string.Empty },
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
            { "content-platform-fonts.zip", "PlatformContent/pc/fonts/" },
            { "content-platform-dictionaries.zip", "PlatformContent/pc/shared_compression_dictionaries/" },
            { "content-terrain.zip", "PlatformContent/pc/terrain/" },
            { "content-textures3.zip", "PlatformContent/pc/textures/" },
            { "extracontent-luapackages.zip", "ExtraContent/LuaPackages/" },
            { "extracontent-translations.zip", "ExtraContent/translations/" },
            { "extracontent-models.zip", "ExtraContent/models/" },
            { "extracontent-textures.zip", "ExtraContent/textures/" },
            { "extracontent-places.zip", "ExtraContent/places/" }
        };
        private readonly Dictionary<string, string> _studioExtractionRoots = new(StringComparer.OrdinalIgnoreCase)
        {
            { "RobloxStudio.zip", string.Empty },
            { "RibbonConfig.zip", "RibbonConfig/" },
            { "redist.zip", string.Empty },
            { "Libraries.zip", string.Empty },
            { "LibrariesQt5.zip", string.Empty },
            { "WebView2.zip", string.Empty },
            { "WebView2RuntimeInstaller.zip", string.Empty },
            { "shaders.zip", "shaders/" },
            { "ssl.zip", "ssl/" },
            { "Qml.zip", "Qml/" },
            { "Plugins.zip", "Plugins/" },
            { "StudioFonts.zip", "StudioFonts/" },
            { "BuiltInPlugins.zip", "BuiltInPlugins/" },
            { "ApplicationConfig.zip", "ApplicationConfig/" },
            { "BuiltInStandalonePlugins.zip", "BuiltInStandalonePlugins/" },
            { "content-qt_translations.zip", "content/qt_translations/" },
            { "content-sky.zip", "content/sky/" },
            { "content-fonts.zip", "content/fonts/" },
            { "content-avatar.zip", "content/avatar/" },
            { "content-models.zip", "content/models/" },
            { "content-sounds.zip", "content/sounds/" },
            { "content-configs.zip", "content/configs/" },
            { "content-api-docs.zip", "content/api_docs/" },
            { "content-textures2.zip", "content/textures/" },
            { "content-studio_svg_textures.zip", "content/studio_svg_textures/" },
            { "content-platform-fonts.zip", "PlatformContent/pc/fonts/" },
            { "content-platform-dictionaries.zip", "PlatformContent/pc/shared_compression_dictionaries/" },
            { "content-terrain.zip", "PlatformContent/pc/terrain/" },
            { "content-textures3.zip", "PlatformContent/pc/textures/" },
            { "extracontent-translations.zip", "ExtraContent/translations/" },
            { "extracontent-luapackages.zip", "ExtraContent/LuaPackages/" },
            { "extracontent-textures.zip", "ExtraContent/textures/" },
            { "extracontent-scripts.zip", "ExtraContent/scripts/" },
            { "extracontent-models.zip", "ExtraContent/models/" }
        };

        private Dictionary<string, string> GetExtractionRoots(string package)
        {
            if (package.Equals("RobloxApp.zip", StringComparison.OrdinalIgnoreCase))
                return _playerExtractionRoots;
            else if (package.Equals("RobloxStudio.zip", StringComparison.OrdinalIgnoreCase))
                return _studioExtractionRoots;

            return _playerExtractionRoots;
        }

        public async Task ExtractPackageBytesAsync(byte[]? bytes, string package, ZipArchive archive)
        {
            Log.Information("[*] Starting package {0} extraction...", package);

            try
            {
                if (bytes == null)
                {
                    Log.Error("[!] Package {0} extraction failed: bytes are null.", package);

                    return;
                }

                if (GetExtractionRoots(package).TryGetValue(package, out string? value))
                    foreach (ZipArchiveEntry entry in new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read).Entries.Where(x => !string.IsNullOrEmpty(x.FullName)))
                    {
                        using MemoryStream stream = new();

                        await entry.Open().CopyToAsync(stream);

                        byte[] entryBytes = stream.ToArray();

                        lock (_lock)
                        {
                            using Stream entryStream = archive.CreateEntry($"{value}{entry.FullName.Replace('\\', '/')}", CompressionLevel.Fastest).Open();

                            entryStream.Write(entryBytes, 0, entryBytes.Length);
                        }
                    }
                else
                    lock (_lock)
                    {
                        using Stream entryStream = archive.CreateEntry(package, CompressionLevel.Fastest).Open();

                        entryStream.Write(bytes, 0, bytes.Length);
                    }

                Log.Information("[*] Package {0} extraction ended.", package);
            }
            catch (Exception ex)
            {
                Log.Error("[!] Package {0} extraction failed: {1}", package, ex.Message);

                throw;
            }
        }

        public static void ExtractPackageContent(string content, string package, ZipArchive archive)
        {
            using StreamWriter writer = new(archive.CreateEntry(package, CompressionLevel.Optimal).Open());

            writer.Write(content);
        }
    }
}