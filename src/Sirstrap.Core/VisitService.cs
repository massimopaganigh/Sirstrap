namespace Sirstrap.Core
{
    public class VisitService(IRobloxAuthClient authClient) : IVisitService
    {
        private const string ROBLOX_PLAYER_BETA = "RobloxPlayerBeta";

        private readonly IRobloxAuthClient _authClient = authClient;

        public static string[] ReadCookies(string cookiesFilePath)
        {
            if (!File.Exists(cookiesFilePath))
                throw new FileNotFoundException($"Cookies file not found: {cookiesFilePath}");

            return File.ReadAllLines(cookiesFilePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
        }

        public static string BuildLaunchUrl(string authTicket, long browserId, long placeId)
        {
            return $"roblox-player:1+launchmode:play+gameinfo:{authTicket}+launchtime:{browserId}+placelauncherurl:https%3A%2F%2Fassetgame.roblox.com%2Fgame%2FPlaceLauncher.ashx%3Frequest%3DRequestGame%26browserTrackerId%3D{browserId}%26placeId%3D{placeId}%26isPlayTogetherGame%3Dfalse+browsertrackerid:{browserId}+robloxLocale:en_us+gameLocale:en_us+channel:";
        }

        public async Task GenerateVisitAsync(Configuration configuration, string roblosecurityToken, long placeId, int timeoutSeconds)
        {
            string tokenPreview = roblosecurityToken.Length > 12
                ? $"{roblosecurityToken[..6]}...{roblosecurityToken[^6..]}"
                : "***";

            Log.Information("[VISIT] --- Generate Visit Start ---");
            Log.Information("[VISIT] Place ID: {0}, Timeout: {1}s, Cookie: {2}", placeId, timeoutSeconds, tokenPreview);

            var stopwatch = Stopwatch.StartNew();

            Log.Information("[VISIT] Step 1/5: Getting CSRF token...");

            string csrfToken;

            try
            {
                csrfToken = await _authClient.GetCsrfTokenAsync(roblosecurityToken);

                Log.Information("[VISIT] CSRF token acquired: {0} ({1}ms)", csrfToken[..Math.Min(8, csrfToken.Length)] + "...", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[VISIT] CSRF token request FAILED after {0}ms.", stopwatch.ElapsedMilliseconds);
                Log.Error("[VISIT] Requirements: POST to friends.roblox.com/v1/users/1/request-friendship with .ROBLOSECURITY cookie. Expected: x-csrf-token header in response (even on 403).");
                Log.Error("[VISIT] Possible causes: invalid/expired cookie, IP ban, rate limiting, network issue.");

                throw;
            }

            Log.Information("[VISIT] Step 2/5: Getting authentication ticket...");

            long ticketStart = stopwatch.ElapsedMilliseconds;

            string authTicket;

            try
            {
                authTicket = await _authClient.GetAuthTicketAsync(roblosecurityToken, csrfToken, placeId);

                Log.Information("[VISIT] Auth ticket acquired ({0}ms). Ticket length: {1}", stopwatch.ElapsedMilliseconds - ticketStart, authTicket.Length);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[VISIT] Auth ticket request FAILED after {0}ms.", stopwatch.ElapsedMilliseconds - ticketStart);
                Log.Error("[VISIT] Requirements: POST to auth.roblox.com/v1/authentication-ticket with .ROBLOSECURITY cookie, x-csrf-token header, Referer: https://www.roblox.com/games/{0}. Expected: rbx-authentication-ticket header in response.", placeId);
                Log.Error("[VISIT] Possible causes: invalid CSRF token (stale), invalid/expired cookie, place ID does not exist, account banned, rate limiting.");

                throw;
            }

            Log.Information("[VISIT] Step 3/5: Building launch URL...");

            long browserId = Random.Shared.NextInt64(100000, 1000000);

            Log.Information("[VISIT] Generated browser ID: {0}", browserId);

            string launchUrl = BuildLaunchUrl(authTicket, browserId, placeId);

            Log.Information("[VISIT] Launch URL built (length: {0}). Protocol: roblox-player, Place: {1}, BrowserTracker: {2}", launchUrl.Length, placeId, browserId);

            configuration.LaunchUri = launchUrl;

            Log.Information("[VISIT] Step 4/5: Launching Roblox player...");

            long launchStart = stopwatch.ElapsedMilliseconds;

            RobloxLauncher.Launch(configuration);

            Log.Information("[VISIT] Roblox player launched ({0}ms).", stopwatch.ElapsedMilliseconds - launchStart);
            Log.Information("[VISIT] Step 5/5: Waiting {0} seconds for visit to register...", timeoutSeconds);

            await Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));

            Log.Information("[VISIT] Timeout elapsed, cleaning up Roblox processes...");

            CleanRobloxProcesses();

            stopwatch.Stop();

            Log.Information("[VISIT] --- Generate Visit Complete (total: {0}ms) ---", stopwatch.ElapsedMilliseconds);
        }

        public async Task RunVisitLoopAsync(Configuration configuration, string cookiesFilePath, long placeId, int timeoutSeconds, CancellationToken cancellationToken = default)
        {
            Log.Information("[VISIT] Loading cookies from: {0}", cookiesFilePath);

            string[] cookies = ReadCookies(cookiesFilePath);

            if (cookies.Length == 0)
                throw new InvalidOperationException("No cookies found in the cookies file.");

            Log.Information("[VISIT] Loaded {0} cookie(s).", cookies.Length);

            for (int i = 0; i < cookies.Length; i++)
            {
                string preview = cookies[i].Length > 12
                    ? $"{cookies[i][..6]}...{cookies[i][^6..]}"
                    : "***";

                Log.Information("[VISIT] Cookie [{0}]: {1} (length: {2})", i, preview, cookies[i].Length);
            }

            Log.Information("[VISIT] Visit bot started. Place: {0}, Timeout: {1}s, Cookies: {2}", placeId, timeoutSeconds, cookies.Length);

            int iteration = 0;
            int successCount = 0;
            int failCount = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                iteration++;

                int cookieIndex = Random.Shared.Next(cookies.Length);
                string token = cookies[cookieIndex];
                string tokenPreview = token.Length > 12
                    ? $"{token[..6]}...{token[^6..]}"
                    : "***";

                Log.Information("[VISIT] ===== Iteration #{0} (success: {1}, fail: {2}) =====", iteration, successCount, failCount);
                Log.Information("[VISIT] Selected cookie index: {0}/{1} ({2})", cookieIndex, cookies.Length - 1, tokenPreview);

                try
                {
                    await GenerateVisitAsync(configuration, token, placeId, timeoutSeconds);

                    successCount++;

                    Log.Information("[VISIT] Iteration #{0} completed successfully. Running total: {1} success, {2} fail.", iteration, successCount, failCount);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    Log.Information("[VISIT] Cancellation requested during iteration #{0}.", iteration);

                    break;
                }
                catch (Exception ex)
                {
                    failCount++;

                    Log.Error(ex, "[VISIT] Iteration #{0} FAILED: {1}. Running total: {2} success, {3} fail.", iteration, ex.Message, successCount, failCount);
                    Log.Error("[VISIT] Exception type: {0}", ex.GetType().FullName);

                    if (ex.InnerException is not null)
                        Log.Error("[VISIT] Inner exception: {0}: {1}", ex.InnerException.GetType().FullName, ex.InnerException.Message);
                }
            }

            Log.Information("[VISIT] Visit bot stopped. Total iterations: {0}, Success: {1}, Fail: {2}", iteration, successCount, failCount);
        }

        private static void CleanRobloxProcesses()
        {
            try
            {
                var processes = Process.GetProcessesByName(ROBLOX_PLAYER_BETA);

                Log.Information("[VISIT] Found {0} Roblox process(es) to clean up.", processes.Length);

                foreach (var process in processes)
                {
                    try
                    {
                        Log.Information("[VISIT] Killing process PID {0} ({1})...", process.Id, process.ProcessName);

                        process.Kill(true);
                        process.WaitForExit(5000);

                        Log.Information("[VISIT] Process PID {0} terminated.", process.Id);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[VISIT] Failed to kill Roblox process PID {0}: {1}", process.Id, ex.Message);
                    }
                }

                Log.Information("[VISIT] Roblox processes cleaned up.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[VISIT] Failed to enumerate/clean Roblox processes: {0}", ex.Message);
            }
        }
    }
}
