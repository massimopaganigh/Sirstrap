using System.Net;
using System.Net.Http.Headers;

namespace Sirstrap.Core
{
    public class RobloxAuthClient : IRobloxAuthClient
    {
#pragma warning disable S1075 // URIs should not be hardcoded - These are external API endpoints
        private const string CSRF_TOKEN_URL = "https://friends.roblox.com/v1/users/1/request-friendship";
        private const string AUTH_TICKET_URL = "https://auth.roblox.com/v1/authentication-ticket";
        private const string ROBLOX_GAME_URL = "https://www.roblox.com/games";
#pragma warning restore S1075
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";
        private const int MAX_CSRF_RETRIES = 3;

        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;

        public RobloxAuthClient(HttpClient? httpClient = null)
        {
            if (httpClient is not null)
            {
                _ownsHttpClient = false;
                _httpClient = httpClient;
            }
            else
            {
                _ownsHttpClient = true;
                var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    UseCookies = false
                };
                _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(1) };
            }
        }

        public void Dispose()
        {
            if (_ownsHttpClient)
                _httpClient.Dispose();
        }

        private HttpRequestMessage CreatePostRequest(string url, string roblosecurityToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(string.Empty)
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content.Headers.ContentLength = 0;
            request.Headers.Add("Cookie", $".ROBLOSECURITY={roblosecurityToken}");
            request.Headers.Add("User-Agent", USER_AGENT);

            return request;
        }

        private static async Task LogResponseDetailsAsync(HttpResponseMessage response, string context)
        {
            string body = string.Empty;

            try
            {
                body = await response.Content.ReadAsStringAsync();
            }
            catch { }

            Log.Warning("[AUTH] {0} - Status: {1} ({2})", context, (int)response.StatusCode, response.StatusCode);
            Log.Warning("[AUTH] {0} - Response headers: {1}", context, string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));

            if (response.Content.Headers.Any())
                Log.Warning("[AUTH] {0} - Content headers: {1}", context, string.Join(", ", response.Content.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));

            if (!string.IsNullOrEmpty(body))
                Log.Warning("[AUTH] {0} - Response body: {1}", context, body);
        }

        public async Task<string> GetCsrfTokenAsync(string roblosecurityToken)
        {
            Log.Information("[AUTH] GetCsrfToken: POST {0}", CSRF_TOKEN_URL);
            Log.Information("[AUTH] GetCsrfToken: Cookie length: {0}, User-Agent: {1}", roblosecurityToken.Length, USER_AGENT);

            var stopwatch = Stopwatch.StartNew();

            using var request = CreatePostRequest(CSRF_TOKEN_URL, roblosecurityToken);

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AUTH] GetCsrfToken: HTTP request FAILED after {0}ms. Network error: {1}", stopwatch.ElapsedMilliseconds, ex.Message);
                Log.Error("[AUTH] GetCsrfToken: Verify network connectivity to friends.roblox.com. Check DNS resolution and firewall rules.");

                throw;
            }

            Log.Information("[AUTH] GetCsrfToken: Response received in {0}ms, status: {1} ({2})", stopwatch.ElapsedMilliseconds, (int)response.StatusCode, response.StatusCode);

            if (response.Headers.TryGetValues("x-csrf-token", out var csrfValues))
            {
                string token = csrfValues.First();

                Log.Information("[AUTH] GetCsrfToken: SUCCESS - CSRF token retrieved (token length: {0}).", token.Length);

                return token;
            }

            await LogResponseDetailsAsync(response, "GetCsrfToken");

            Log.Error("[AUTH] GetCsrfToken: FAILED - No x-csrf-token header in response.");
            Log.Error("[AUTH] GetCsrfToken API requirements:");
            Log.Error("[AUTH]   Endpoint: POST {0}", CSRF_TOKEN_URL);
            Log.Error("[AUTH]   Required headers: Cookie (.ROBLOSECURITY), Content-Type (application/json), User-Agent");
            Log.Error("[AUTH]   Expected response: x-csrf-token header (returned even on 403 Forbidden)");
            Log.Error("[AUTH]   Common failure reasons:");
            Log.Error("[AUTH]     - 401 Unauthorized: .ROBLOSECURITY cookie is invalid or expired");
            Log.Error("[AUTH]     - 429 Too Many Requests: rate limited, wait before retrying");
            Log.Error("[AUTH]     - No x-csrf-token header: endpoint may have changed or cookie is completely invalid");

            throw new InvalidOperationException("Failed to retrieve CSRF token from response headers.");
        }

        public async Task<string> GetAuthTicketAsync(string roblosecurityToken, string csrfToken, long placeId)
        {
            string currentCsrfToken = csrfToken;

            Log.Information("[AUTH] GetAuthTicket: POST {0} for place {1}", AUTH_TICKET_URL, placeId);
            Log.Information("[AUTH] GetAuthTicket: CSRF token length: {0}, Cookie length: {1}", csrfToken.Length, roblosecurityToken.Length);

            for (int attempt = 1; attempt <= MAX_CSRF_RETRIES; attempt++)
            {
                Log.Information("[AUTH] GetAuthTicket: Attempt {0}/{1}...", attempt, MAX_CSRF_RETRIES);

                var stopwatch = Stopwatch.StartNew();

                using var request = CreatePostRequest(AUTH_TICKET_URL, roblosecurityToken);

                request.Headers.Add("x-csrf-token", currentCsrfToken);
                request.Headers.Referrer = new Uri($"{ROBLOX_GAME_URL}/{placeId}");

                Log.Information("[AUTH] GetAuthTicket: Referer: {0}/{1}", ROBLOX_GAME_URL, placeId);

                HttpResponseMessage response;

                try
                {
                    response = await _httpClient.SendAsync(request);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[AUTH] GetAuthTicket: HTTP request FAILED on attempt {0} after {1}ms: {2}", attempt, stopwatch.ElapsedMilliseconds, ex.Message);

                    if (attempt < MAX_CSRF_RETRIES)
                    {
                        int delaySeconds = (int)Math.Pow(2, attempt);

                        Log.Warning("[AUTH] GetAuthTicket: Retrying in {0}s...", delaySeconds);

                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

                        continue;
                    }

                    throw;
                }

                Log.Information("[AUTH] GetAuthTicket: Response received in {0}ms, status: {1} ({2})", stopwatch.ElapsedMilliseconds, (int)response.StatusCode, response.StatusCode);

                if (response.Headers.TryGetValues("rbx-authentication-ticket", out var ticketValues))
                {
                    string ticket = ticketValues.First();

                    Log.Information("[AUTH] GetAuthTicket: SUCCESS - Auth ticket retrieved on attempt {0} (ticket length: {1}).", attempt, ticket.Length);

                    return ticket;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden
                    && response.Headers.TryGetValues("x-csrf-token", out var freshCsrfValues))
                {
                    currentCsrfToken = freshCsrfValues.First();

                    Log.Warning("[AUTH] GetAuthTicket: 403 with fresh CSRF token (length: {0}), retrying (attempt {1}/{2})...", currentCsrfToken.Length, attempt, MAX_CSRF_RETRIES);

                    continue;
                }

                await LogResponseDetailsAsync(response, $"GetAuthTicket (attempt {attempt}/{MAX_CSRF_RETRIES})");

                if (attempt < MAX_CSRF_RETRIES)
                {
                    int delaySeconds = (int)Math.Pow(2, attempt);

                    Log.Warning("[AUTH] GetAuthTicket: Retrying in {0}s (attempt {1}/{2})...", delaySeconds, attempt, MAX_CSRF_RETRIES);

                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            Log.Error("[AUTH] GetAuthTicket: FAILED after {0} attempts.", MAX_CSRF_RETRIES);
            Log.Error("[AUTH] GetAuthTicket API requirements:");
            Log.Error("[AUTH]   Endpoint: POST {0}", AUTH_TICKET_URL);
            Log.Error("[AUTH]   Required headers:");
            Log.Error("[AUTH]     - Cookie: .ROBLOSECURITY=<token>");
            Log.Error("[AUTH]     - x-csrf-token: <csrf_token from GetCsrfToken>");
            Log.Error("[AUTH]     - Referer: {0}/<placeId>", ROBLOX_GAME_URL);
            Log.Error("[AUTH]     - Content-Type: application/json");
            Log.Error("[AUTH]     - User-Agent: Chrome-like UA string");
            Log.Error("[AUTH]   Expected response: rbx-authentication-ticket header");
            Log.Error("[AUTH]   Common failure reasons:");
            Log.Error("[AUTH]     - 403 Forbidden: CSRF token expired/invalid (auto-retried with fresh token)");
            Log.Error("[AUTH]     - 401 Unauthorized: .ROBLOSECURITY cookie is invalid or expired");
            Log.Error("[AUTH]     - 403 without new CSRF: account may be banned or restricted");
            Log.Error("[AUTH]     - 429 Too Many Requests: rate limited");
            Log.Error("[AUTH]     - Missing Referer header: some endpoints require it for CORS validation");

            throw new InvalidOperationException("Failed to retrieve authentication ticket from response headers after all retries.");
        }
    }
}
