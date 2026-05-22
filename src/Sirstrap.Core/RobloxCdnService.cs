namespace Sirstrap.Core
{
    public static class RobloxCdnService
    {
#pragma warning disable S1075 // URIs should not be hardcoded - These are official Roblox deployment CDNs.
        public const string DefaultBaseUri = "https://setup.rbxcdn.com";

        private static readonly CdnCandidate[] _candidates =
        [
            new(DefaultBaseUri, 0),
            new("https://setup-aws.rbxcdn.com", 2),
            new("https://setup-ak.rbxcdn.com", 2),
            new("https://roblox-setup.cachefly.net", 2),
            new("https://s3.amazonaws.com/setup.roblox.com", 4)
        ];
#pragma warning restore S1075

        private static string GetProbeUri(Configuration configuration, string baseUri)
        {
            if (configuration.IsMacBinary())
            {
                string package = configuration.BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase)
                    ? "RobloxPlayer.zip"
                    : "RobloxStudioApp.zip";

                return UriBuilder.GetPackageUri(configuration, package, baseUri);
            }

            return UriBuilder.GetManifestUri(configuration, baseUri);
        }

        private static async Task<string> GetFastestBaseUriAsync(HttpClient httpClient, Configuration configuration, CancellationToken cancellationToken)
        {
            Log.Information("[*] Roblox CDN URI override is empty, selecting the fastest Roblox CDN...");

            Task<CdnProbeResult?>[] probeTasks = _candidates
                .Select(candidate => ProbeCandidateAsync(httpClient, configuration, candidate, cancellationToken))
                .ToArray();

            CdnProbeResult? selected = (await Task.WhenAll(probeTasks).ConfigureAwait(false))
                .OfType<CdnProbeResult>()
                .OrderBy(result => result.Elapsed)
                .ThenBy(result => result.Candidate.FallbackPriority)
                .FirstOrDefault();

            if (selected == null)
            {
                Log.Warning("[*] Failed to probe Roblox CDNs, falling back to {0}.", DefaultBaseUri);

                return DefaultBaseUri;
            }

            Log.Information("[*] Selected Roblox CDN: {0} ({1} ms).", selected.Candidate.BaseUri, (int)selected.Elapsed.TotalMilliseconds);

            return selected.Candidate.BaseUri;
        }

        private static async Task<CdnProbeResult?> ProbeCandidateAsync(HttpClient httpClient, Configuration configuration, CdnCandidate candidate, CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));

            string probeUri = GetProbeUri(configuration, candidate.BaseUri);
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                using HttpRequestMessage request = new(HttpMethod.Get, probeUri);
                using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);

                stopwatch.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("[*] Roblox CDN probe failed for {0}: HTTP {1}.", candidate.BaseUri, (int)response.StatusCode);

                    return null;
                }

                return new CdnProbeResult(candidate, stopwatch.Elapsed);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                Log.Warning(ex, "[*] Roblox CDN probe timed out for {0}.", candidate.BaseUri);

                return null;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[*] Roblox CDN probe failed for {0}: {1}", candidate.BaseUri, ex.Message);

                return null;
            }
        }

        public static string NormalizeCdnUriOverride(string? cdnUriOverride)
        {
            if (string.IsNullOrWhiteSpace(cdnUriOverride))
                return string.Empty;

            string normalized = cdnUriOverride.Trim().TrimEnd('/');

            if (string.IsNullOrEmpty(normalized))
                return string.Empty;

            if (!Uri.TryCreate(normalized, UriKind.Absolute, out Uri? uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || string.IsNullOrWhiteSpace(uri.Host))
            {
                Log.Warning("[*] Ignoring invalid Roblox CDN URI override: {0}.", cdnUriOverride);

                return string.Empty;
            }

            return uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
        }

        public static async Task<string> ResolveAsync(HttpClient httpClient, Configuration configuration, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(configuration);

            string normalizedOverride = NormalizeCdnUriOverride(SirstrapConfiguration.RobloxCdnUriOverride);

            SirstrapConfiguration.RobloxCdnUriOverride = normalizedOverride;

            if (!string.IsNullOrEmpty(normalizedOverride))
            {
                SirstrapConfiguration.ResolvedRobloxCdnUri = normalizedOverride;

                Log.Information("[*] Roblox CDN URI override is set, using {0}.", normalizedOverride);

                return normalizedOverride;
            }

            string fastestBaseUri = await GetFastestBaseUriAsync(httpClient, configuration, cancellationToken).ConfigureAwait(false);

            SirstrapConfiguration.ResolvedRobloxCdnUri = fastestBaseUri;

            return fastestBaseUri;
        }

        private sealed record CdnCandidate(string BaseUri, int FallbackPriority);

        private sealed record CdnProbeResult(CdnCandidate Candidate, TimeSpan Elapsed);
    }
}
