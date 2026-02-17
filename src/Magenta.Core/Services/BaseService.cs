namespace Magenta.Core.Services
{
    public class BaseService
    {
        public readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromMinutes(1)
        };

        public virtual HttpRequestMessage GetRequest(string requestUri, string roblosecurityCookie)
        {
            HttpRequestMessage request = new(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(string.Empty)
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content.Headers.ContentLength = 0;
            request.Headers.Add("Cookie", $".ROBLOSECURITY={roblosecurityCookie}");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            return request;
        }
    }
}
