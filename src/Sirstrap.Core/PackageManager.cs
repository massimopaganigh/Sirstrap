namespace Sirstrap.Core
{
    public class PackageManager(HttpClient httpClient)
    {
        private const string APP_SETTINGS_XML = """<?xml version="1.0" encoding="UTF-8"?><Settings><ContentFolder>content</ContentFolder><BaseUrl>http://www.roblox.com</BaseUrl></Settings>""";

        private readonly HttpClient _httpClient = httpClient;
        private readonly PackageExtractor _packageExtractor = new();

        private async Task DownloadPackageAsync(Configuration configuration, string package, ZipArchive archive)
        {
            Log.Information("[*] Starting package {0} download...", package);

            try
            {
                await _packageExtractor.ExtractPackageBytesAsync(await HttpClientExtension.GetByteArrayAsync(_httpClient, UriBuilder.GetPackageUri(configuration, package)), package, archive);

                Log.Information("[*] Package {0} download ended.", package);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Package {0} download failed: {1}", package, ex.Message);

                throw;
            }
        }

        public async Task Download4MacAsync(Configuration configuration)
        {
            string archiveName = configuration.BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase)
                ? "RobloxPlayer.zip"
                : "RobloxStudioApp.zip";

            Log.Information("[*] Starting archive {0} download...", archiveName);

            try
            {
                await File.WriteAllBytesAsync(configuration.GetOutputPath(), (await HttpClientExtension.GetByteArrayAsync(_httpClient, UriBuilder.GetPackageUri(configuration, archiveName)))!);

                Log.Information("[*] Archive {0} download ended.", archiveName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Archive {0} download failed: {1}", archiveName, ex.Message);

                throw;
            }
        }

        public async Task Download4WindowsAsync(Configuration configuration)
        {
            Log.Information("[*] Starting packages download...", configuration.BinaryType);

            try
            {
                Manifest manifest = ManifestParser.Parse(await HttpClientExtension.GetStringAsync(_httpClient, UriBuilder.GetManifestUri(configuration)));

                if (!manifest.IsValid)
                {
                    Log.Error("[!] Packages download failed: invalid manifest.");

                    return;
                }

                using ZipArchive archive = ZipFile.Open(configuration.GetOutputPath(), ZipArchiveMode.Create);

                PackageExtractor.ExtractPackageContent(APP_SETTINGS_XML, "AppSettings.xml", archive);

                await Task.WhenAll(manifest.Packages.Select(package => DownloadPackageAsync(configuration, package, archive)));

                Log.Information("[*] Packages download ended.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Packages download failed: {0}", ex.Message);

                throw;
            }
        }
    }
}