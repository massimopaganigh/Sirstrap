namespace Sirstrap.Core.Cdn
{
    public sealed class CdnUriNormalizer : ICdnUriNormalizer
    {
        public string Normalize(string? cdnUriOverride)
        {
            if (string.IsNullOrWhiteSpace(cdnUriOverride))
                return string.Empty;

            string trimmed = cdnUriOverride.Trim().TrimEnd('/');

            if (string.IsNullOrEmpty(trimmed))
                return string.Empty;

            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || string.IsNullOrWhiteSpace(uri.Host))
            {
                Log.Warning("[!] Ignoring the invalid Roblox CDN URI override {CdnUriOverride}.", cdnUriOverride);

                return string.Empty;
            }

            return uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
        }
    }
}
