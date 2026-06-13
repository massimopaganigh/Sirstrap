namespace Sirstrap.Core.Cdn
{
    public sealed class DefaultCdnCandidateProvider : ICdnCandidateProvider
    {
#pragma warning disable S1075 // URIs should not be hardcoded - These are official Roblox deployment CDNs.
        private static readonly IReadOnlyList<CdnCandidate> _candidates =
        [
            new(RobloxCdnService.DefaultBaseUri, 0),
            new("https://setup-aws.rbxcdn.com", 2),
            new("https://setup-ak.rbxcdn.com", 2),
            new("https://roblox-setup.cachefly.net", 2),
            new("https://s3.amazonaws.com/setup.roblox.com", 4)
        ];
#pragma warning restore S1075

        public IReadOnlyList<CdnCandidate> GetCandidates() => _candidates;
    }
}
