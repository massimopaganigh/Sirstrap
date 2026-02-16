using System.Net;
using System.Net.Http.Headers;

namespace Sirstrap.Core
{
    public class VisitService : IDisposable
    {
        private const string CSRF_TOKEN_URL = "https://friends.roblox.com/v1/users/1/request-friendship";
        private const string AUTH_TICKET_URL = "https://auth.roblox.com/v1/authentication-ticket";
        private const string ROBLOX_GAME_URL = "https://www.roblox.com/games";
        private const string ROBLOX_PLAYER_BETA = "RobloxPlayerBeta";
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";
        private const int MAX_CSRF_RETRIES = 3;

        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;

        public VisitService(HttpClient? httpClient = null)
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

        public static string[] ReadCookies(string cookiesFilePath)
        {
            if (!File.Exists(cookiesFilePath))
                throw new FileNotFoundException($"Cookies file not found: {cookiesFilePath}");

            return File.ReadAllLines(cookiesFilePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
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
            catch { /* Sybau 🥀 */ }

            Log.Warning("[*] {0} - Status: {1} ({2})", context, (int)response.StatusCode, response.StatusCode);
            Log.Warning("[*] {0} - Response headers: {1}", context, string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));

            if (!string.IsNullOrEmpty(body))
                Log.Warning("[*] {0} - Response body: {1}", context, body);
        }

        public async Task<string> GetCsrfTokenAsync(string roblosecurityToken)
        {
            using var request = CreatePostRequest(CSRF_TOKEN_URL, roblosecurityToken);

            var response = await _httpClient.SendAsync(request);

            if (response.Headers.TryGetValues("x-csrf-token", out var csrfValues))
            {
                string token = csrfValues.First();

                Log.Information("[*] CSRF token retrieved (status {0}).", (int)response.StatusCode);

                return token;
            }

            await LogResponseDetailsAsync(response, "GetCsrfToken");

            throw new InvalidOperationException("Failed to retrieve CSRF token from response headers.");
        }

        public async Task<string> GetAuthTicketAsync(string roblosecurityToken, string csrfToken, long placeId)
        {
            string currentCsrfToken = csrfToken;

            for (int attempt = 1; attempt <= MAX_CSRF_RETRIES; attempt++)
            {
                using var request = CreatePostRequest(AUTH_TICKET_URL, roblosecurityToken);

                request.Headers.Add("x-csrf-token", currentCsrfToken);
                request.Headers.Referrer = new Uri($"{ROBLOX_GAME_URL}/{placeId}");

                var response = await _httpClient.SendAsync(request);

                if (response.Headers.TryGetValues("rbx-authentication-ticket", out var ticketValues))
                {
                    string ticket = ticketValues.First();

                    Log.Information("[*] Authentication ticket retrieved (status {0}).", (int)response.StatusCode);

                    return ticket;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden
                    && response.Headers.TryGetValues("x-csrf-token", out var freshCsrfValues))
                {
                    currentCsrfToken = freshCsrfValues.First();

                    Log.Warning("[*] Auth ticket request returned 403 with fresh CSRF token, retrying (attempt {0}/{1})...", attempt, MAX_CSRF_RETRIES);

                    continue;
                }

                await LogResponseDetailsAsync(response, $"GetAuthTicket (attempt {attempt}/{MAX_CSRF_RETRIES})");

                if (attempt < MAX_CSRF_RETRIES)
                {
                    Log.Warning("[*] Retrying auth ticket request (attempt {0}/{1})...", attempt, MAX_CSRF_RETRIES);

                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                }
            }

            throw new InvalidOperationException("Failed to retrieve authentication ticket from response headers after all retries.");
        }

        public static string BuildLaunchUrl(string authTicket, long browserId, long placeId)
        {
            return $"roblox-player:1+launchmode:play+gameinfo:{authTicket}+launchtime:{browserId}+placelauncherurl:https%3A%2F%2Fassetgame.roblox.com%2Fgame%2FPlaceLauncher.ashx%3Frequest%3DRequestGame%26browserTrackerId%3D{browserId}%26placeId%3D{placeId}%26isPlayTogetherGame%3Dfalse+browsertrackerid:{browserId}+robloxLocale:en_us+gameLocale:en_us+channel:";
        }

        public async Task GenerateVisitAsync(Configuration configuration, string roblosecurityToken, long placeId, int timeoutSeconds)
        {
            Log.Information("[*] Getting CSRF token...");

            string csrfToken = await GetCsrfTokenAsync(roblosecurityToken);

            Log.Information("[*] Getting authentication ticket...");

            string authTicket = await GetAuthTicketAsync(roblosecurityToken, csrfToken, placeId);

            long browserId = Random.Shared.NextInt64(100000, 1000000);
            string launchUrl = BuildLaunchUrl(authTicket, browserId, placeId);

            configuration.LaunchUri = launchUrl;

            Log.Information("[*] Launching Roblox with visit URL for place {0}...", placeId);

            RobloxLauncher.Launch(configuration);

            Log.Information("[*] Waiting {0} seconds...", timeoutSeconds);

            await Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));

            CleanRobloxProcesses();
        }

        public async Task RunVisitLoopAsync(Configuration configuration, string cookiesFilePath, long placeId, int timeoutSeconds, CancellationToken cancellationToken = default)
        {
            string[] cookies = ReadCookies(cookiesFilePath);

            if (cookies.Length == 0)
                throw new InvalidOperationException("No cookies found in the cookies file.");

            Log.Information("[*] Visit bot started with {0} cookie(s) for place {1}.", cookies.Length, placeId);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    string token = cookies[Random.Shared.Next(cookies.Length)];

                    await GenerateVisitAsync(configuration, token, placeId, timeoutSeconds);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Visit generation failed: {0}", ex.Message);
                }
            }

            Log.Information("[*] Visit bot stopped.");
        }

        private static void CleanRobloxProcesses()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName(ROBLOX_PLAYER_BETA))
                {
                    try
                    {
                        process.Kill(true);
                        process.WaitForExit(5000);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[!] Failed to kill Roblox process: {0}", ex.Message);
                    }
                }

                Log.Information("[*] Roblox processes cleaned up.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to clean Roblox processes: {0}", ex.Message);
            }
        }
    }
}
