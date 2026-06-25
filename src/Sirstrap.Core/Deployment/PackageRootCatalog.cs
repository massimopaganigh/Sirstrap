namespace Sirstrap.Core.Deployment
{
    public static class PackageRootCatalog
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

        public static bool TryGetRoot(string package, out string root)
        {
            Dictionary<string, string> roots = package.Equals("RobloxStudio.zip", StringComparison.OrdinalIgnoreCase)
                ? _studioRoots
                : _playerRoots;

            return roots.TryGetValue(package, out root!);
        }
    }
}
