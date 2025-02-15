using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using Serilog;

namespace Sirstrap
{
    /// <summary>
    /// The entry point of the application.
    /// </summary>
    public static class Program
    {
        private static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("SirstrapLog.txt").CreateLogger();

            await new RobloxDownloader().ExecuteAsync(args);
            await Log.CloseAndFlushAsync();
        }
    }

    /// <summary>
    /// Manages the downloading, processing, installation, and launching of the application.
    /// </summary>
    public class RobloxDownloader
    {
        private readonly VersionManager _versionManager;
        private readonly PackageManager _packageManager;

        public RobloxDownloader()
        {
            var httpClient = new HttpClient();

            _versionManager = new VersionManager(httpClient);
            _packageManager = new PackageManager(httpClient);
        }

        public async Task ExecuteAsync(string[] args)
        {
            try
            {
                var downloadConfiguration = ConfigurationManager.CreateDownloadConfiguration(CommandLineParser.Parse(args));

                if (!await InitializeDownloadAsync(downloadConfiguration))
                {
                    return;
                }
                if (IsAlreadyInstalled(downloadConfiguration))
                {
                    Log.Information("[*] Version {0} is already installed.", downloadConfiguration.Version);

                    if (LaunchApplication(downloadConfiguration))
                    {
                        return;
                    }
                }

                await DownloadAndProcessFilesAsync(downloadConfiguration);

                InstallAndLaunchApplication(downloadConfiguration);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error: {0}", ex.Message);
            }
        }

        private async Task<bool> InitializeDownloadAsync(DownloadConfiguration downloadConfiguration)
        {
            if (string.IsNullOrEmpty(downloadConfiguration.Version))
            {
                downloadConfiguration.Version = await _versionManager.GetLatestVersionAsync(downloadConfiguration.BinaryType);

                if (string.IsNullOrEmpty(downloadConfiguration.Version))
                {
                    return false;
                }
            }

            downloadConfiguration.Version = VersionManager.NormalizeVersion(downloadConfiguration.Version);

            return true;
        }

        private static bool IsAlreadyInstalled(DownloadConfiguration downloadConfiguration)
        {
            return downloadConfiguration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase) && Directory.Exists(PathManager.GetVersionInstallPath(downloadConfiguration.Version));
        }

        private static bool LaunchApplication(DownloadConfiguration downloadConfiguration)
        {
            return downloadConfiguration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase) && ApplicationLauncher.Launch(downloadConfiguration);
        }

        private async Task DownloadAndProcessFilesAsync(DownloadConfiguration downloadConfiguration)
        {
            if (downloadConfiguration.IsMacBinary())
            {
                await _packageManager.DownloadMacBinaryAsync(downloadConfiguration);
            }
            else
            {
                var manifest = await _packageManager.DownloadManifestAsync(downloadConfiguration);

                if (!string.IsNullOrEmpty(manifest))
                {
                    await _packageManager.ProcessManifestAsync(manifest, downloadConfiguration);
                }
            }
        }

        private static void InstallAndLaunchApplication(DownloadConfiguration downloadConfiguration)
        {
            if (!downloadConfiguration.BinaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            
            ApplicationInstaller.Install(downloadConfiguration);

            LaunchApplication(downloadConfiguration);
        }
    }

    /// <summary>
    /// Parses command line arguments into a dictionary.
    /// </summary>
    public static class CommandLineParser
    {
        public static Dictionary<string, string> Parse(string[] args)
        {
            return args.Where(arg => arg.StartsWith("--")).Select(arg => arg[2..].Split('=', 2)).Where(parts => parts.Length == 2).ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Creates and validates download configuration settings.
    /// </summary>
    public static class ConfigurationManager
    {
        private static readonly Dictionary<string, BinaryTypeInfo> BinaryTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            { "WindowsPlayer", new BinaryTypeInfo { VersionFile = "/version", DefaultBlobDir = "/" } },
            { "WindowsStudio64", new BinaryTypeInfo { VersionFile = "/versionQTStudio", DefaultBlobDir = "/" } },
            { "MacPlayer", new BinaryTypeInfo { VersionFile = "/mac/version", DefaultBlobDir = "/mac/" } },
            { "MacStudio", new BinaryTypeInfo { VersionFile = "/mac/versionStudio", DefaultBlobDir = "/mac/" } }
        };

        public static DownloadConfiguration CreateDownloadConfiguration(Dictionary<string, string> args)
        {
            var downloadConfiguration = new DownloadConfiguration
            {
                Channel = args.GetValueOrDefault("channel", "LIVE"),
                BinaryType = args.GetValueOrDefault("binaryType", "WindowsPlayer"),
                Version = args.GetValueOrDefault("version") ?? string.Empty,
                BlobDir = GetBlobDirectory(args),
                CompressZip = args.GetValueOrDefault("compressZip", "false").Equals("true", StringComparison.OrdinalIgnoreCase),
                CompressionLevel = GetCompressionLevel(args)
            };

            ValidateConfiguration(downloadConfiguration);

            return downloadConfiguration;
        }

        private static string GetBlobDirectory(Dictionary<string, string> arguments)
        {
            var blobDir = arguments.GetValueOrDefault("blobDir");

            if (string.IsNullOrEmpty(blobDir))
            {
                return BinaryTypes.TryGetValue(arguments.GetValueOrDefault("binaryType", "WindowsPlayer"), out var bt) ? bt.DefaultBlobDir : string.Empty;
            }

            return NormalizeBlobDirectory(blobDir);
        }

        private static int GetCompressionLevel(Dictionary<string, string> args)
        {
            if (args.TryGetValue("compressionLevel", out var value) && int.TryParse(value, out var level) && level is >= 1 and <= 9)
            {
                return level;
            }

            return 6;
        }

        private static void ValidateConfiguration(DownloadConfiguration downloadConfiguration)
        {
            if (!BinaryTypes.ContainsKey(downloadConfiguration.BinaryType))
            {
                throw new ArgumentException($"Unsupported binary type: {downloadConfiguration.BinaryType}");
            }
        }

        private static string NormalizeBlobDirectory(string blobDir)
        {
            if (!blobDir.StartsWith($"/"))
            {
                blobDir = "/" + blobDir;
            }

            if (!blobDir.EndsWith($"/"))
            {
                blobDir += "/";
            }

            return blobDir;
        }
    }

    /// <summary>
    /// Contains binary type information such as version file and default blob directory.
    /// </summary>
    public class BinaryTypeInfo
    {
        public string VersionFile { get; set; }

        public string DefaultBlobDir { get; set; }
    }

    /// <summary>
    /// Holds configuration settings for the download operation.
    /// </summary>
    public class DownloadConfiguration
    {
        public string Channel { get; set; }

        public string BinaryType { get; set; }

        public string Version { get; set; }

        public string BlobDir { get; set; }

        public bool CompressZip { get; set; }

        public int CompressionLevel { get; set; }

        public bool IsMacBinary()
        {
            return BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) || BinaryType.Equals("MacStudio", StringComparison.OrdinalIgnoreCase);
        }

        public string GetOutputFileName()
        {
            return $"{Version}.zip";
        }
    }

    /// <summary>
    /// Handles version retrieval and normalization from the API.
    /// </summary>
    public class VersionManager
    {
        private readonly HttpClient _httpClient;

        public VersionManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetLatestVersionAsync(string binaryType)
        {
            Log.Information("[*] No version specified, getting versions from APIs...");

            var sirhurtVersion = await GetSirhurtVersionAsync(binaryType);
            var robloxVersion = await GetRobloxVersionAsync();

            if (string.IsNullOrEmpty(sirhurtVersion) || string.IsNullOrEmpty(robloxVersion))
            {
                Log.Error("[!] Failed to retrieve one or both versions.");

                return string.Empty;
            }

            if (sirhurtVersion.Equals(robloxVersion, StringComparison.OrdinalIgnoreCase))
            {
                Log.Information("[*] Using version: {0}", sirhurtVersion);

                return sirhurtVersion;
            }

            Log.Information("[*] Version mismatch detected:");
            Log.Information("[*] Please choose which version to use:");
            Log.Information("    1. SirHurt version: {0}", sirhurtVersion);
            Log.Information("    2. Roblox version: {0}", robloxVersion);

            var choice = Console.ReadLine();

            return choice == "1" ? sirhurtVersion : robloxVersion;
        }

        private async Task<string> GetSirhurtVersionAsync(string binaryType)
        {
            var versionApiUrl = GetVersionApiUrl(binaryType);

            if (string.IsNullOrEmpty(versionApiUrl))
            {
                Log.Error("[!] Cannot get version for binary type '{0}'.", binaryType);

                return string.Empty;
            }

            try
            {
                var response = await _httpClient.GetStringAsync(versionApiUrl);

                using var jsonDocument = JsonDocument.Parse(response);

                if (jsonDocument.RootElement.EnumerateArray().FirstOrDefault().TryGetProperty("SirHurt V5", out var sirhurt))
                {
                    if (sirhurt.TryGetProperty("roblox_version", out var version))
                    {
                        return version.GetString() ?? string.Empty;
                    }

                    Log.Error("[!] roblox_version field not found in JSON response.");

                    return string.Empty;
                }

                Log.Error("[!] SirHurt V5 field not found in JSON response.");

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting SirHurt version from API: {0}", ex.Message);

                return string.Empty;
            }
        }

        private async Task<string> GetRobloxVersionAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("https://clientsettingscdn.roblox.com/v1/client-version/WindowsPlayer");

                using var jsonDocument = JsonDocument.Parse(response);

                if (jsonDocument.RootElement.TryGetProperty("clientVersionUpload", out var version))
                {
                    return version.GetString() ?? string.Empty;
                }

                Log.Error("[!] clientVersionUpload field not found in JSON response.");

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting Roblox version from API: {0}", ex.Message);

                return string.Empty;
            }
        }

        public static string NormalizeVersion(string version)
        {
            return version.StartsWith("version-", StringComparison.CurrentCultureIgnoreCase) ? version : "version-" + version;
        }

        private static string GetVersionApiUrl(string binaryType)
        {
            return binaryType.Equals("WindowsPlayer", StringComparison.OrdinalIgnoreCase) ? "https://sirhurt.net/status/fetch.php?exploit=SirHurt%20V5" : string.Empty;
        }
    }

    /// <summary>
    /// Provides paths for version installation.
    /// </summary>
    public static class PathManager
    {
        public static string GetVersionInstallPath(string version)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Versions", version);
        }
    }

    /// <summary>
    /// Manages package downloads, manifest processing, and ZIP archive assembly.
    /// </summary>
    public class PackageManager
    {
        private readonly HttpClient _httpClient;

        public PackageManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task DownloadMacBinaryAsync(DownloadConfiguration downloadConfiguration)
        {
            var zipFileName = downloadConfiguration.BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) ? "RobloxPlayer.zip" : "RobloxStudioApp.zip";

            Log.Information("[*] Downloading ZIP archive for {0} ({1})...", downloadConfiguration.BinaryType, zipFileName);

            var bytes = await HttpHelper.GetBytesAsync(_httpClient, UrlBuilder.GetBinaryUrl(downloadConfiguration, zipFileName));

            await File.WriteAllBytesAsync(downloadConfiguration.GetOutputFileName(), bytes);

            Log.Information("[*] File downloaded: {0}", downloadConfiguration.GetOutputFileName());
        }

        public async Task<string> DownloadManifestAsync(DownloadConfiguration downloadConfiguration)
        {
            return await HttpHelper.GetStringAsync(_httpClient, UrlBuilder.GetManifestUrl(downloadConfiguration));
        }

        public async Task ProcessManifestAsync(string manifestContent, DownloadConfiguration downloadConfiguration)
        {
            var manifest = ManifestParser.Parse(manifestContent);

            if (!manifest.IsValid)
            {
                Log.Error("[!] Error: Invalid manifest version or format.");

                return;
            }

            await AssemblePackagesAsync(manifest, downloadConfiguration);
        }

        private async Task AssemblePackagesAsync(Manifest manifest, DownloadConfiguration downloadConfiguration)
        {
            using (var finalZip = ZipFile.Open(downloadConfiguration.GetOutputFileName(), ZipArchiveMode.Create))
            {
                AddDefaultSettings(finalZip);

                await DownloadAndProcessPackagesAsync(manifest, finalZip, downloadConfiguration);
            }

            Log.Information("[*] Archive assembled: {0}", downloadConfiguration.GetOutputFileName());
        }

        private static void AddDefaultSettings(ZipArchive finalZip)
        {
            const string settings = """<?xml version="1.0" encoding="UTF-8"?><Settings><ContentFolder>content</ContentFolder><BaseUrl>http://www.roblox.com</BaseUrl></Settings>""";

            PackageExtractor.AddTextFile(finalZip, "AppSettings.xml", settings);
        }

        private async Task DownloadAndProcessPackagesAsync(Manifest manifest, ZipArchive finalZip, DownloadConfiguration downloadConfiguration)
        {
            await Task.WhenAll(manifest.Packages.Select(package => DownloadAndProcessPackageAsync(package, finalZip, downloadConfiguration)).ToList());
        }

        private async Task DownloadAndProcessPackageAsync(string package, ZipArchive finalZip, DownloadConfiguration downloadConfiguration)
        {
            Log.Information("[*] Downloading package {0}...", package);

            var bytes = await HttpHelper.GetBytesAsync(_httpClient, UrlBuilder.GetPackageUrl(downloadConfiguration, package));

            await PackageExtractor.ProcessPackageAsync(bytes, package, finalZip);
        }
    }

    /// <summary>
    /// Constructs URLs for accessing the manifest, binary, and package files.
    /// </summary>
    public static class UrlBuilder
    {
        private const string HostPath = "https://setup-cfly.rbxcdn.com";

        public static string GetBinaryUrl(DownloadConfiguration downloadConfiguration, string fileName)
        {
            return $"{GetBasePath(downloadConfiguration)}{downloadConfiguration.BlobDir}{downloadConfiguration.Version}-{fileName}";
        }

        public static string GetManifestUrl(DownloadConfiguration downloadConfiguration)
        {
            return $"{GetBasePath(downloadConfiguration)}{downloadConfiguration.BlobDir}{downloadConfiguration.Version}-rbxPkgManifest.txt";
        }

        public static string GetPackageUrl(DownloadConfiguration downloadConfiguration, string packageName)
        {
            return $"{GetBasePath(downloadConfiguration)}{downloadConfiguration.BlobDir}{downloadConfiguration.Version}-{packageName}";
        }

        private static string GetBasePath(DownloadConfiguration downloadConfiguration)
        {
            return downloadConfiguration.Channel.Equals("LIVE", StringComparison.OrdinalIgnoreCase) ? HostPath : $"{HostPath}/channel/{downloadConfiguration.Channel}";
        }
    }

    /// <summary>
    /// Provides HTTP helper methods for downloading strings and byte arrays.
    /// </summary>
    public static class HttpHelper
    {
        public static async Task<byte[]> GetBytesAsync(HttpClient httpClient, string url)
        {
            try
            {
                return await httpClient.GetByteArrayAsync(url);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error requesting binary from {Url}: {ErrorMessage}", url, ex.Message);

                return null!;
            }
        }

        public static async Task<string> GetStringAsync(HttpClient httpClient, string url)
        {
            try
            {
                return await httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error requesting {Url}: {ErrorMessage}", url, ex.Message);

                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Parses the manifest content to extract package information.
    /// </summary>
    public static class ManifestParser
    {
        public static Manifest Parse(string manifestContext)
        {
            var lines = manifestContext.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

            return new Manifest
            {
                IsValid = IsValidManifest(lines),
                Packages = GetPackages(lines)
            };
        }

        private static bool IsValidManifest(string[] lines)
        {
            return lines.Length > 0 && lines[0].Trim().Equals("v0", StringComparison.OrdinalIgnoreCase);
        }

        private static List<string> GetPackages(string[] lines)
        {
            return lines.Where(line => line.Contains('.') && line.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).Select(line => line.Trim()).ToList();
        }
    }

    /// <summary>
    /// Represents the manifest containing package information.
    /// </summary>
    public class Manifest
    {
        public bool IsValid { get; set; }

        public List<string> Packages { get; set; }
    }

    /// <summary>
    /// Extracts and integrates downloaded packages into a final ZIP archive.
    /// </summary>
    public static class PackageExtractor
    {
        private static readonly Dictionary<string, string> ExtractRootsPlayer = new(StringComparer.OrdinalIgnoreCase)
        {
            { "RobloxApp.zip", "" },
            { "redist.zip", "" },
            { "shaders.zip", "shaders/" },
            { "ssl.zip", "ssl/" },
            { "WebView2.zip", "" },
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
            { "RobloxStudio.zip", "" },
            { "RibbonConfig.zip", "RibbonConfig/" },
            { "redist.zip", "" },
            { "Libraries.zip", "" },
            { "LibrariesQt5.zip", "" },
            { "WebView2.zip", "" },
            { "WebView2RuntimeInstaller.zip", "" },
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

        private static readonly object _zipLock = new();

        public static void AddTextFile(ZipArchive finalZip, string entryName, string settings)
        {
            using var writer = new StreamWriter(finalZip.CreateEntry(entryName, CompressionLevel.Optimal).Open());

            writer.Write(settings);
        }

        public static async Task ProcessPackageAsync(byte[] bytes, string package, ZipArchive finalZip)
        {
            if (GetExtractRoots(package).TryGetValue(package, out string? value))
            {
                await ExtractAndIntegratePackageAsync(bytes, package, finalZip, value);
            }
            else
            {
                AddPackageAsFile(bytes, package, finalZip);
            }
        }

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

        private static async Task ExtractAndIntegratePackageAsync(byte[] bytes, string package, ZipArchive finalZip, string value)
        {
            foreach (var entry in new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read).Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                {
                    continue;
                }

                var targetValue = value + entry.FullName.Replace('\\', '/');

                await IntegrateEntryAsync(entry, targetValue, finalZip);
            }

            Log.Information("[*] Package {0} extracted and integrated.", package);
        }

        private static void AddPackageAsFile(byte[] bytes, string package, ZipArchive finalZip)
        {
            lock (_zipLock)
            {
                finalZip.CreateEntry(package, CompressionLevel.Optimal).Open().Write(bytes, 0, bytes.Length);
            }

            Log.Warning("[*] {0} not defined in extract roots: added as single file.", package);
        }

        private static async Task IntegrateEntryAsync(ZipArchiveEntry entry, string targetValue, ZipArchive finalZip)
        {
            using var msEntry = new MemoryStream();

            await entry.Open().CopyToAsync(msEntry);

            var fileData = msEntry.ToArray();

            lock (_zipLock)
            {
                using var entryStream = finalZip.CreateEntry(targetValue, CompressionLevel.Optimal).Open();
                
                entryStream.Write(fileData, 0, fileData.Length);
            }
        }
    }

    /// <summary>
    /// Installs the application by extracting the final ZIP archive to the target directory.
    /// </summary>
    public static class ApplicationInstaller
    {
        public static void Install(DownloadConfiguration downloadConfiguration)
        {
            var targetPath = PathManager.GetVersionInstallPath(downloadConfiguration.Version);
            var zipPath = downloadConfiguration.GetOutputFileName();

            try
            {
                PrepareInstallDirectory(targetPath);

                try
                {
                    using (var archive = ZipFile.OpenRead(zipPath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            var destinationPath = Path.GetFullPath(Path.Combine(targetPath, entry.FullName));

                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? string.Empty);

                            if (!string.IsNullOrEmpty(entry.Name))
                            {
                                entry.ExtractToFile(destinationPath, true);
                            }
                        }
                    }
                }
                finally
                {
                    DeleteFileWithRetry(zipPath);
                }

                Log.Information("[*] Archive successfully extracted to: {0}", targetPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Installation error: {0}", ex.Message);

                throw;
            }
        }

        private static void PrepareInstallDirectory(string targetPath)
        {
            var parentDir = Directory.GetParent(targetPath)?.FullName;

            if (Directory.Exists(parentDir))
            {
                try
                {
                    Directory.Delete(parentDir, recursive: true);
                }
                catch (UnauthorizedAccessException)
                {
                    foreach (var file in Directory.GetFiles(parentDir, "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (IOException) { /* Continue with other files */ }
                    }
                }
            }

            Directory.CreateDirectory(targetPath);
        }

        private static void DeleteFileWithRetry(string filePath, int maxAttempts = 3)
        {
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    return;
                }
                catch (IOException) when (attempt < maxAttempts)
                {
                    Thread.Sleep(100 * attempt);
                }
            }
        }
    }

    /// <summary>
    /// Launches the application executable.
    /// </summary>
    public static class ApplicationLauncher
    {
        public static bool Launch(DownloadConfiguration downloadConfiguration)
        {
            var executablePath = Path.Combine(PathManager.GetVersionInstallPath(downloadConfiguration.Version), "RobloxPlayerBeta.exe");

            if (File.Exists(executablePath))
            {
                Log.Information("[*] Launching {0}...", executablePath);
                Process.Start(new ProcessStartInfo { FileName = executablePath, WorkingDirectory = Path.GetDirectoryName(executablePath), UseShellExecute = true });

                return true;
            }
            else
            {
                Log.Error("[!] Could not find {0}.", executablePath);

                return false;
            }
        }
    }
}