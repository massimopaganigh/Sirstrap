using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace Monsoon.Services.Weao
{
    public sealed class WeaoClient : IDisposable
    {
        public const string UserAgent = "WEAO-3PService";

        private static readonly string[] DefaultDomains =
        [
            "https://weao.xyz",
            "https://whatexpsare.online",
            "https://whatexploitsaretra.sh",
            "https://weao.gg",
        ];
        private readonly IReadOnlyList<string> _domains;
        private readonly HttpClient _http;
        private readonly ILogger _log;
        private readonly bool _ownsHttp;

        public WeaoClient(IReadOnlyList<string>? domains = null, ILogger? logger = null) : this(CreateDefaultHttpClient(), ownsHttp: true, domains, logger)
        {
        }

        public WeaoClient(HttpClient http, bool ownsHttp = false, IReadOnlyList<string>? domains = null, ILogger? logger = null)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _ownsHttp = ownsHttp;
            _domains = domains is { Count: > 0 } ? domains : DefaultDomains;
            _log = logger ?? Log.Logger;

            if (!_http.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent))
                _http.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        private static HttpClient CreateDefaultHttpClient() => new(new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            ConnectTimeout = TimeSpan.FromSeconds(10)
        })
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        private async Task<T> GetAsync<T>(string path, JsonTypeInfo<T> typeInfo, CancellationToken ct)
        {
            Exception? lastError = null;

            for (var i = 0; i < _domains.Count; i++)
            {
                var url = _domains[i] + path;

                try
                {
                    return await SendAsync(url, typeInfo, ct).ConfigureAwait(false);
                }
                catch (WeaoRateLimitException)
                {
                    throw;
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
                {
                    lastError = ex;

                    _log.Warning(ex, "WEAO request to {Url} failed; trying next mirror ({Index}/{Count})", url, i + 1, _domains.Count);
                }
            }

            throw new WeaoRequestException("All WEAO mirrors failed to respond.", statusCode: null, innerException: lastError);
        }

        private async Task<T> SendAsync<T>(string url, JsonTypeInfo<T> typeInfo, CancellationToken ct)
        {
            using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                throw new WeaoRateLimitException(await TryReadRateLimitAsync(response, ct).ConfigureAwait(false));

            if (!response.IsSuccessStatusCode)
                throw new WeaoRequestException($"WEAO request to {url} returned {(int)response.StatusCode} {response.ReasonPhrase}.", response.StatusCode);

            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);

            try
            {
                var result = await JsonSerializer.DeserializeAsync(stream, typeInfo, ct).ConfigureAwait(false);

                return result ?? throw new WeaoRequestException($"WEAO request to {url} returned an empty or null body.", response.StatusCode);
            }
            catch (JsonException ex)
            {
                throw new WeaoRequestException($"Failed to parse WEAO response from {url}.", response.StatusCode, ex);
            }
        }

        private static async Task<RateLimitInfo?> TryReadRateLimitAsync(HttpResponseMessage response, CancellationToken ct)
        {
            try
            {
                await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);

                var error = await JsonSerializer.DeserializeAsync(stream, WeaoJsonContext.Default.WeaoErrorResponse, ct).ConfigureAwait(false);

                return error?.RateLimitInfo;
            }
            catch (Exception ex) when (ex is JsonException or HttpRequestException or IOException)
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (_ownsHttp)
                _http.Dispose();
        }

        public Task<RobloxVersions> GetCurrentVersionsAsync(CancellationToken ct = default) => GetAsync("/api/versions/current", WeaoJsonContext.Default.RobloxVersions, ct);

        public Task<ExploitStatus> GetExploitAsync(string exploit, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(exploit))
                throw new ArgumentException("Exploit name must be provided.", nameof(exploit));

            return GetAsync("/api/status/exploits/" + Uri.EscapeDataString(exploit), WeaoJsonContext.Default.ExploitStatus, ct);
        }

        public Task<List<ExploitStatus>> GetExploitsAsync(CancellationToken ct = default) => GetAsync("/api/status/exploits", WeaoJsonContext.Default.ListExploitStatus, ct);

        public Task<RobloxVersions> GetFutureVersionsAsync(CancellationToken ct = default) => GetAsync("/api/versions/future", WeaoJsonContext.Default.RobloxVersions, ct);

        public Task<RobloxVersions> GetPastVersionsAsync(CancellationToken ct = default) => GetAsync("/api/versions/past", WeaoJsonContext.Default.RobloxVersions, ct);

        public Task<SuncData> GetSuncAsync(ExploitStatus exploit, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(exploit);

            if (exploit.Sunc?.Scrap is not { } scrap
                || exploit.Sunc?.Key is not { } key)
                throw new WeaoException($"Exploit '{exploit.Title}' does not expose sUNC credentials.");

            return GetSuncAsync(scrap, key, ct);
        }

        public Task<SuncData> GetSuncAsync(string scrap, string key, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(scrap))
                throw new ArgumentException("sUNC scrap must be provided.", nameof(scrap));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("sUNC key must be provided.", nameof(key));

            var path = $"/api/sunc?scrap={Uri.EscapeDataString(scrap)}&key={Uri.EscapeDataString(key)}";

            return GetAsync(path, WeaoJsonContext.Default.SuncData, ct);
        }
    }
}
