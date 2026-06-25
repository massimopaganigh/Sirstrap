namespace Sirstrap.Core.Cdn
{
    public interface ICdnProber
    {
        Task<CdnProbeResult?> ProbeAsync(CdnCandidate candidate, Configuration configuration, CancellationToken cancellationToken);
    }
}
