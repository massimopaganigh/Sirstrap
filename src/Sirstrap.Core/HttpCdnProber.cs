namespace Sirstrap.Core
{
    public sealed class HttpCdnProber : ICdnProber
    {
        private readonly HttpClient _httpClient;
        private readonly ICdnProbeUriFactory _probeUriFactory;
        private readonly TimeSpan _probeTimeout;

        public HttpCdnProber(HttpClient httpClient, ICdnProbeUriFactory probeUriFactory, TimeSpan probeTimeout)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(probeUriFactory);

            _httpClient = httpClient;
            _probeUriFactory = probeUriFactory;
            _probeTimeout = probeTimeout;
        }

        public async Task<CdnProbeResult?> ProbeAsync(CdnCandidate candidate, Configuration configuration, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            ArgumentNullException.ThrowIfNull(configuration);

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(_probeTimeout);

            string probeUri = _probeUriFactory.Create(configuration, candidate.BaseUri);
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                using HttpRequestMessage request = new(HttpMethod.Get, probeUri);
                using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);

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
    }
}
