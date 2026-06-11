namespace Sirstrap.Core
{
    public sealed record CdnProbeResult(CdnCandidate Candidate, TimeSpan Elapsed);
}
