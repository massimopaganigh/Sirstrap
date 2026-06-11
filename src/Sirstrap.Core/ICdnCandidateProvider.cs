namespace Sirstrap.Core
{
    public interface ICdnCandidateProvider
    {
        IReadOnlyList<CdnCandidate> GetCandidates();
    }
}
