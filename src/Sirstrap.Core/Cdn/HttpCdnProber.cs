namespace Sirstrap.Core.Cdn
{
    public sealed class HttpCdnProber : ICdnProber
    {
        private static readonly TimeSpan _probeTimeout = TimeSpan.FromSeconds(5);

        private readonly HttpClient _httpClient;
        private readonly ICdnProbeUriFactory _probeUriFactory;
        private readonly ICdnTelemetry _telemetry;

        public HttpCdnProber(HttpClient httpClient, ICdnProbeUriFactory probeUriFactory, ICdnTelemetry telemetry)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(probeUriFactory);
            ArgumentNullException.ThrowIfNull(telemetry);

            _httpClient = httpClient;
            _probeUriFactory = probeUriFactory;
            _telemetry = telemetry;
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
                    Log.Warning("[!] The Roblox CDN probe for {BaseUri} failed with HTTP {StatusCode}.", candidate.BaseUri, (int)response.StatusCode);

                    _telemetry.RecordProbe(candidate.BaseUri, success: false, stopwatch.Elapsed);

                    return null;
                }

                _telemetry.RecordProbe(candidate.BaseUri, success: true, stopwatch.Elapsed);

                return new CdnProbeResult(candidate, stopwatch.Elapsed);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();

                Log.Warning(ex, "[!] The Roblox CDN probe for {BaseUri} timed out.", candidate.BaseUri);

                _telemetry.RecordProbe(candidate.BaseUri, success: false, stopwatch.Elapsed);

                return null;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                Log.Warning(ex, "[!] The Roblox CDN probe for {BaseUri} failed.", candidate.BaseUri);

                _telemetry.RecordProbe(candidate.BaseUri, success: false, stopwatch.Elapsed);

                return null;
            }
        }
    }
}
