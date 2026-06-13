namespace Sirstrap.Core.Update
{
    public sealed record GitHubReleaseAsset(string Name, string DownloadUri);

    public sealed record GitHubRelease(string TagName, bool IsDraft, string Body, IReadOnlyList<GitHubReleaseAsset> Assets)
    {
        public static GitHubRelease FromJson(JsonElement element)
        {
            var tagName = element.TryGetProperty("tag_name", out JsonElement tagNameElement)
                ? tagNameElement.GetString() ?? string.Empty
                : string.Empty;
            var isDraft = element.TryGetProperty("draft", out JsonElement draftElement) && draftElement.GetBoolean();
            var body = element.TryGetProperty("body", out JsonElement bodyElement)
                ? bodyElement.GetString() ?? string.Empty
                : string.Empty;

            return new GitHubRelease(tagName, isDraft, body, ParseAssets(element));
        }

        public string FindAssetDownloadUri(string assetName)
            => Assets.FirstOrDefault(asset => asset.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))?.DownloadUri ?? string.Empty;

        private static List<GitHubReleaseAsset> ParseAssets(JsonElement element)
        {
            List<GitHubReleaseAsset> assets = [];

            if (!element.TryGetProperty("assets", out JsonElement assetsElement))
                return assets;

            foreach (JsonElement assetElement in assetsElement.EnumerateArray())
            {
                if (!assetElement.TryGetProperty("name", out JsonElement nameElement))
                    continue;

                var name = nameElement.GetString();

                if (string.IsNullOrEmpty(name))
                    continue;

                var downloadUri = assetElement.TryGetProperty("browser_download_url", out JsonElement uriElement)
                    ? uriElement.GetString() ?? string.Empty
                    : string.Empty;

                assets.Add(new GitHubReleaseAsset(name, downloadUri));
            }

            return assets;
        }
    }
}
