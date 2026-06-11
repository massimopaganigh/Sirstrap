namespace Sirstrap.Core
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
                Log.Warning("[*] Ignoring invalid Roblox CDN URI override: {0}.", cdnUriOverride);

                return string.Empty;
            }

            return uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
        }
    }
}
