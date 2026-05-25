namespace Sirstrap.Core
{
    public interface ICdnProber
    {
        Task<CdnProbeResult?> ProbeAsync(CdnCandidate candidate, Configuration configuration, CancellationToken cancellationToken);
    }
}
