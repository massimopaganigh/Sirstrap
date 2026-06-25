namespace Sirstrap.Core.Cdn
{
    public interface ICdnCandidateProvider
    {
        IReadOnlyList<CdnCandidate> GetCandidates();
    }
}
