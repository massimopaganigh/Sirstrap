namespace Sirstrap.Core.Update
{
    public sealed class GitHubReleaseClient(HttpClient httpClient)
    {
#pragma warning disable S1075 // URIs should not be hardcoded - External API endpoint.
        private const string RELEASES_URI = "https://api.github.com/repos/massimopaganigh/sirstrap/releases";
#pragma warning restore S1075

        public async Task<IReadOnlyList<GitHubRelease>> GetReleasesAsync()
        {
            try
            {
                using var jsonDocument = JsonDocument.Parse(await httpClient.GetStringAsync(RELEASES_URI));

                return [.. jsonDocument.RootElement.EnumerateArray().Select(GitHubRelease.FromJson)];
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to retrieve the Sirstrap releases from GitHub.");

                return [];
            }
        }
    }
}
