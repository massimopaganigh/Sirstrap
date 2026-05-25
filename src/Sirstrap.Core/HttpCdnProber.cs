namespace Sirstrap.Core
{
    public sealed class HttpCdnProber : ICdnProber
    {
        private readonly HttpClient _httpClient;
        private readonly ICdnProbeUriFactory _probeUriFactory;
        private readonly ICdnTelemetry _telemetry;
        private readonly TimeSpan _probeTimeout;

        public HttpCdnProber(HttpClient httpClient, ICdnProbeUriFactory probeUriFactory, ICdnTelemetry telemetry, TimeSpan probeTimeout)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(probeUriFactory);
            ArgumentNullException.ThrowIfNull(telemetry);

            _httpClient = httpClient;
            _probeUriFactory = probeUriFactory;
            _telemetry = telemetry;
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

                    _telemetry.RecordProbe(candidate.BaseUri, success: false, stopwatch.Elapsed);

                    return null;
                }

                _telemetry.RecordProbe(candidate.BaseUri, success: true, stopwatch.Elapsed);

                return new CdnProbeResult(candidate, stopwatch.Elapsed);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();

                Log.Warning(ex, "[*] Roblox CDN probe timed out for {0}.", candidate.BaseUri);

                _telemetry.RecordProbe(candidate.BaseUri, success: false, stopwatch.Elapsed);

                return null;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                Log.Warning(ex, "[*] Roblox CDN probe failed for {0}: {1}", candidate.BaseUri, ex.Message);

                _telemetry.RecordProbe(candidate.BaseUri, success: false, stopwatch.Elapsed);

                return null;
            }
        }
    }
}
