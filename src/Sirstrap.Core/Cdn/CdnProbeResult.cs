namespace Sirstrap.Core.Cdn
{
    public sealed record CdnProbeResult(CdnCandidate Candidate, TimeSpan Elapsed);
}
