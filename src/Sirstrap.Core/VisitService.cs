namespace Sirstrap.Core
{
    public class VisitService : IDisposable
    {
        private const string CSRF_TOKEN_URL = "https://friends.roblox.com/v1/users/1/request-friendship";
        private const string AUTH_TICKET_URL = "https://auth.roblox.com/v1/authentication-ticket";
        private const string ROBLOX_GAME_URL = "https://www.roblox.com/games";
        private const string ROBLOX_PLAYER_BETA = "RobloxPlayerBeta";

        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;

        public VisitService(HttpClient? httpClient = null)
        {
            _ownsHttpClient = httpClient is null;
            _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromMinutes(1) };
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

        public async Task<string> GetCsrfTokenAsync(string roblosecurityToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, CSRF_TOKEN_URL);

            request.Headers.Add("Cookie", $".ROBLOSECURITY={roblosecurityToken}");

            var response = await _httpClient.SendAsync(request);

            if (response.Headers.TryGetValues("x-csrf-token", out var csrfValues))
                return csrfValues.First();

            throw new InvalidOperationException("Failed to retrieve CSRF token from response headers.");
        }

        public async Task<string> GetAuthTicketAsync(string roblosecurityToken, string csrfToken, long placeId)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, AUTH_TICKET_URL);

            request.Headers.Add("Cookie", $".ROBLOSECURITY={roblosecurityToken}");
            request.Headers.Add("x-csrf-token", csrfToken);
            request.Headers.Referrer = new Uri($"{ROBLOX_GAME_URL}/{placeId}");

            var response = await _httpClient.SendAsync(request);

            if (response.Headers.TryGetValues("rbx-authentication-ticket", out var ticketValues))
                return ticketValues.First();

            throw new InvalidOperationException("Failed to retrieve authentication ticket from response headers.");
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
