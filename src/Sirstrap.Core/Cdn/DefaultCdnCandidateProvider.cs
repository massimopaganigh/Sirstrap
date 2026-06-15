namespace Sirstrap.Core.Cdn
{
    public sealed class DefaultCdnCandidateProvider : ICdnCandidateProvider
    {
        private static readonly IReadOnlyList<CdnCandidate> _candidates =
        [
            new(RobloxCdnService.DefaultBaseUri, 0),
            new("https://setup-aws.rbxcdn.com", 2),
            new("https://setup-ak.rbxcdn.com", 2),
            new("https://roblox-setup.cachefly.net", 2),
            new("https://s3.amazonaws.com/setup.roblox.com", 4)
        ];

        public IReadOnlyList<CdnCandidate> GetCandidates() => _candidates;
    }
}
