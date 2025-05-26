using Serilog;
using System.IO.Compression;

namespace Sirstrap.Core
{
    /// <summary>
    /// Provides functionality to extract and integrate downloaded Roblox packages 
    /// into a final ZIP archive, handling different extraction paths for Player and Studio components.
    /// </summary>
    public static class PackageExtractor
    {
        private static readonly Dictionary<string, string> ExtractRootsPlayer = new(StringComparer.OrdinalIgnoreCase)
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

        private static readonly Dictionary<string, string> ExtractRootsStudio = new(StringComparer.OrdinalIgnoreCase)
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

        private static readonly Lock _zipLock = new();

        /// <summary>
        /// Creates and adds a text file with the specified content to the final ZIP archive.
        /// </summary>
        /// <param name="finalZip">The target ZIP archive to add the file to.</param>
        /// <param name="entryName">The name and path of the file within the archive.</param>
        /// <param name="settings">The text content to write to the file.</param>
        /// <remarks>
        /// The file is compressed using the optimal compression level.
        /// </remarks>
        public static void AddTextFile(ZipArchive finalZip, string entryName, string settings)
        {
            using var writer = new StreamWriter(finalZip.CreateEntry(entryName, CompressionLevel.Optimal).Open());

            writer.Write(settings);
        }

        /// <summary>
        /// Processes a downloaded package by either extracting and integrating its contents
        /// or adding it as a single file to the final ZIP archive.
        /// </summary>
        /// <param name="bytes">The raw binary content of the package.</param>
        /// <param name="package">The package filename, which determines how it's processed.</param>
        /// <param name="finalZip">The target ZIP archive where content will be added.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// The processing method is determined by the package name:
        /// - If listed in the extract roots dictionaries, the package is extracted and its contents
        ///   are integrated with appropriate directory paths.
        /// - Otherwise, the package is added as a single file.
        /// </remarks>
        public static async Task ProcessPackageAsync(byte[] bytes, string package, ZipArchive finalZip)
        {
            if (bytes == null)
            {
                Log.Error("[!] Failed to download package {0}: Received null data", package);

                return;
            }

            if (GetExtractRoots(package).TryGetValue(package, out string? value))
            {
                await ExtractAndIntegratePackageAsync(bytes, package, finalZip, value).ConfigureAwait(false);
            }
            else
            {
                AddPackageAsFile(bytes, package, finalZip);
            }
        }

        /// <summary>
        /// Determines which extraction root dictionary to use based on the package name.
        /// </summary>
        /// <param name="package">The package filename.</param>
        /// <returns>
        /// The appropriate dictionary mapping package names to extraction root paths.
        /// Returns the Player dictionary by default.
        /// </returns>
        private static Dictionary<string, string> GetExtractRoots(string package)
        {
            if (package.Equals("RobloxApp.zip", StringComparison.OrdinalIgnoreCase))
            {
                return ExtractRootsPlayer;
            }
            else if (package.Equals("RobloxStudio.zip", StringComparison.OrdinalIgnoreCase))
            {
                return ExtractRootsStudio;
            }

            return ExtractRootsPlayer;
        }

        /// <summary>
        /// Extracts the contents of a package and integrates them into the final ZIP archive.
        /// </summary>
        /// <param name="bytes">The raw binary content of the package.</param>
        /// <param name="package">The package filename, used for logging.</param>
        /// <param name="finalZip">The target ZIP archive where content will be added.</param>
        /// <param name="value">The base directory path for entries from this package.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Each entry in the package is extracted and added to the final ZIP with
        /// its path prefixed by the specified value.
        /// </remarks>
        private static async Task ExtractAndIntegratePackageAsync(byte[] bytes, string package, ZipArchive finalZip, string value)
        {
            foreach (var entry in new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read).Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                {
                    continue;
                }

                var targetValue = $"{value}{entry.FullName.Replace('\\', '/')}";

                await IntegrateEntryAsync(entry, targetValue, finalZip).ConfigureAwait(false);
            }

            Log.Information("[*] Package {0} extracted and integrated.", package);
        }

        /// <summary>
        /// Adds a package as a single file to the final ZIP archive.
        /// </summary>
        /// <param name="bytes">The raw binary content of the package.</param>
        /// <param name="package">The package filename, which will be used as the entry name.</param>
        /// <param name="finalZip">The target ZIP archive where the file will be added.</param>
        /// <remarks>
        /// This method is used for packages that are not defined in the extract roots dictionaries.
        /// Access to the ZIP archive is synchronized to prevent concurrent modification issues.
        /// </remarks>
        private static void AddPackageAsFile(byte[] bytes, string package, ZipArchive finalZip)
        {
            lock (_zipLock)
            {
                finalZip.CreateEntry(package, CompressionLevel.Optimal).Open().Write(bytes, 0, bytes.Length);
            }

            Log.Warning("[*] {0} not defined in extract roots: added as single file.", package);
        }

        /// <summary>
        /// Integrates a single entry from a package into the final ZIP archive.
        /// </summary>
        /// <param name="entry">The ZIP archive entry to integrate.</param>
        /// <param name="targetValue">The destination path within the final ZIP archive.</param>
        /// <param name="finalZip">The target ZIP archive where the entry will be added.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Reads the entry content into memory and then writes it to the final ZIP.
        /// Access to the ZIP archive is synchronized to prevent concurrent modification issues.
        /// </remarks>
        private static async Task IntegrateEntryAsync(ZipArchiveEntry entry, string targetValue, ZipArchive finalZip)
        {
            using var msEntry = new MemoryStream();

            await entry.Open().CopyToAsync(msEntry).ConfigureAwait(false);

            var fileData = msEntry.ToArray();

            lock (_zipLock)
            {
                using var entryStream = finalZip.CreateEntry(targetValue, CompressionLevel.Optimal).Open();

                entryStream.Write(fileData, 0, fileData.Length);
            }
        }
    }
}