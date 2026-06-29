using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Monsoon.Services.Roblox
{
    public readonly record struct DownloadProgress(int CompletedPackages, int TotalPackages, string CurrentPackage, long BytesDownloaded, long TotalBytes);

    public sealed class RobloxCdnClient(HttpClient http, bool ownsHttp = false, string? channel = null, ILogger? logger = null) : IDisposable
    {
        private static readonly HashSet<HttpStatusCode> MissCodes =
        [
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound,
        ];
        private readonly string _channel = string.IsNullOrWhiteSpace(channel) ? RobloxDeployment.DefaultChannel : channel;
        private readonly HttpClient _http = http ?? throw new ArgumentNullException(nameof(http));
        private readonly ILogger _log = logger ?? Log.Logger;

        public RobloxCdnClient(string? channel = null, ILogger? logger = null) : this(CreateDefaultHttpClient(), ownsHttp: true, channel, logger)
        {
        }

        private static HttpClient CreateDefaultHttpClient() => new(new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            ConnectTimeout = TimeSpan.FromSeconds(10),
            MaxConnectionsPerServer = 8
        })
        {
            Timeout = TimeSpan.FromMinutes(10)
        };

        private Task DownloadPackageCoreAsync(string version, Package package, string destinationFile, bool verifyHash, Action<long>? onBytes, CancellationToken ct)
        {
            var file = RobloxDeployment.DeploymentFile(version, package.FileName);
            var directory = Path.GetDirectoryName(destinationFile);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            return WithFailoverAsync(file, async (response, c) =>
            {
                await using (var source = await response.Content.ReadAsStreamAsync(c).ConfigureAwait(false))
                await using (var destination = File.Create(destinationFile))
                {
                    var buffer = new byte[81920];
                    long total = 0;
                    int read;

                    while ((read = await source.ReadAsync(buffer, c).ConfigureAwait(false)) > 0)
                    {
                        await destination.WriteAsync(buffer.AsMemory(0, read), c).ConfigureAwait(false);

                        total += read;

                        onBytes?.Invoke(total);
                    }
                }

                if (verifyHash)
                    await VerifyHashAsync(package, destinationFile, c).ConfigureAwait(false);
            }, ct);
        }

        private static void ExtractPackage(string zipPath, string installRoot, string relativeDirectory)
        {
            var destinationBase = Path.GetFullPath(Path.Combine(installRoot, relativeDirectory));

            using var archive = ZipFile.OpenRead(zipPath);

            foreach (var entry in archive.Entries)
            {
                var name = entry.FullName.Replace('\\', '/');

                if (name.EndsWith('/'))
                    continue;

                var destinationPath = Path.GetFullPath(Path.Combine(destinationBase, name));

                if (!IsWithin(destinationBase, destinationPath))
                    throw new RobloxDeploymentException($"Zip entry '{entry.FullName}' escapes the target directory.");

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                entry.ExtractToFile(destinationPath, overwrite: true);
            }
        }

        private static bool IsTransient(Exception ex) => ex is HttpRequestException or IOException or RobloxHashMismatchException or TaskCanceledException;

        private static bool IsWithin(string baseDirectory, string candidate)
        {
            var root = baseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            return candidate.Equals(baseDirectory, StringComparison.OrdinalIgnoreCase) || candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Best-effort cleanup of the scratch directory.
            }
        }

        private static void ValidateVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("Version GUID must be provided.", nameof(version));
        }

        private static async Task VerifyHashAsync(Package package, string file, CancellationToken ct)
        {
            await using var stream = File.OpenRead(file);

            var hash = await MD5.HashDataAsync(stream, ct).ConfigureAwait(false);
            var actual = Convert.ToHexStringLower(hash);

            if (!actual.Equals(package.Signature, StringComparison.OrdinalIgnoreCase))
                throw new RobloxHashMismatchException(package.FileName, package.Signature, actual);
        }

        private async Task<T> WithFailoverAsync<T>(string versionedFile, Func<HttpResponseMessage, CancellationToken, Task<T>> onSuccess, CancellationToken ct)
        {
            Exception? lastError = null;
            HttpStatusCode? lastStatus = null;

            foreach (var url in RobloxDeployment.CandidateUrls(_channel, versionedFile))
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                        return await onSuccess(response, ct).ConfigureAwait(false);

                    lastStatus = response.StatusCode;

                    if (!MissCodes.Contains(response.StatusCode))
                        _log.Warning("Roblox CDN {Url} returned {Status}; trying next candidate.", url, (int)response.StatusCode);
                }
                catch (Exception ex) when (IsTransient(ex) && !ct.IsCancellationRequested)
                {
                    lastError = ex;

                    _log.Warning(ex, "Roblox CDN request to {Url} failed; trying next candidate.", url);
                }
            }

            throw new RobloxDeploymentException($"Failed to fetch '{versionedFile}' from every Roblox mirror.", lastStatus, lastError);
        }

        private async Task WithFailoverAsync(string versionedFile, Func<HttpResponseMessage, CancellationToken, Task> onSuccess, CancellationToken ct) => await WithFailoverAsync<bool>(versionedFile, async (response, c) =>
        {
            await onSuccess(response, c).ConfigureAwait(false);

            return true;
        }, ct).ConfigureAwait(false);

        private static async Task WriteAppSettingsAsync(string installPath, CancellationToken ct)
        {
            const string appSettings = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<Settings>\r\n\t<ContentFolder>content</ContentFolder>\r\n\t<BaseUrl>http://www.roblox.com</BaseUrl>\r\n</Settings>\r\n";

            await File.WriteAllTextAsync(Path.Combine(installPath, "AppSettings.xml"), appSettings, ct).ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (ownsHttp)
                _http.Dispose();
        }

        public async Task DownloadAndExtractAsync(string version, string installPath, IProgress<DownloadProgress>? progress = null, bool verifyHashes = true, bool writeAppSettings = true, CancellationToken ct = default)
        {
            ValidateVersion(version);

            if (string.IsNullOrWhiteSpace(installPath))
                throw new ArgumentException("Install path must be provided.", nameof(installPath));

            var manifest = await GetPackageManifestAsync(version, ct).ConfigureAwait(false);

            Directory.CreateDirectory(installPath);

            var tempDir = Path.Combine(Path.GetTempPath(), "Monsoon-rbx-" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(tempDir);

            try
            {
                var totalBytes = manifest.TotalPackedSize;
                long doneBytes = 0;

                for (var i = 0; i < manifest.Packages.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    var package = manifest.Packages[i];

                    if (!RobloxDeployment.PlayerPackageDirectories.TryGetValue(package.FileName, out var targetDir))
                    {
                        _log.Warning("Unknown Roblox package {Package}; skipping extraction.", package.FileName);

                        doneBytes += package.PackedSize;

                        continue;
                    }

                    var tempFile = Path.Combine(tempDir, package.FileName);
                    var baseBytes = doneBytes;

                    await DownloadPackageCoreAsync(version, package, tempFile, verifyHashes, read => progress?.Report(new DownloadProgress(i, manifest.Packages.Count, package.FileName, baseBytes + read, totalBytes)), ct).ConfigureAwait(false);

                    ExtractPackage(tempFile, installPath, targetDir);

                    File.Delete(tempFile);

                    doneBytes += package.PackedSize;

                    progress?.Report(new DownloadProgress(i + 1, manifest.Packages.Count, package.FileName, doneBytes, totalBytes));
                }

                if (writeAppSettings)
                    await WriteAppSettingsAsync(installPath, ct).ConfigureAwait(false);
            }
            finally
            {
                TryDeleteDirectory(tempDir);
            }
        }

        public Task DownloadPackageAsync(string version, Package package, string destinationFile, IProgress<long>? byteProgress = null, bool verifyHash = true, CancellationToken ct = default)
        {
            ValidateVersion(version);

            ArgumentNullException.ThrowIfNull(package);

            if (string.IsNullOrWhiteSpace(destinationFile))
                throw new ArgumentException("Destination file must be provided.", nameof(destinationFile));

            return DownloadPackageCoreAsync(version, package, destinationFile, verifyHash, byteProgress is null ? null : byteProgress.Report, ct);
        }

        public async Task<PackageManifest> GetPackageManifestAsync(string version, CancellationToken ct = default)
        {
            ValidateVersion(version);

            var file = RobloxDeployment.DeploymentFile(version, RobloxDeployment.PackageManifestFile);
            var content = await WithFailoverAsync(file, static (response, c) => response.Content.ReadAsStringAsync(c), ct).ConfigureAwait(false);

            return PackageManifest.Parse(version, content);
        }
    }
}
