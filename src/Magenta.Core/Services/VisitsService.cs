namespace Magenta.Core.Services
{
    public class VisitsService(CancellationToken cancellationToken)
    {
        private readonly ConfigurationService _configurationService = new();
        private readonly RobloxService _robloxService = new();

        private async Task GetVisit(string roblosecurityToken)
        {
            (bool success, string xCsrfToken) = await new XCsrfTokenService().GetXCsrfToken(roblosecurityToken);

            if (!success
                || string.IsNullOrEmpty(xCsrfToken))
                return;

            (success, string rbxAuthenticationTicket) = await new RbxAuthenticationTicketService().GetRbxAuthenticationTicket(roblosecurityToken, xCsrfToken);

            if (!success
                || string.IsNullOrEmpty(rbxAuthenticationTicket))
                return;

            long browserId = Random.Shared.NextInt64(100000, 1000000);

            success = _robloxService.StartRoblox($"roblox-player:1+launchmode:play+gameinfo:{rbxAuthenticationTicket}+launchtime:{browserId}+placelauncherurl:https%3A%2F%2Fassetgame.roblox.com%2Fgame%2FPlaceLauncher.ashx%3Frequest%3DRequestGame%26browserTrackerId%3D{browserId}%26placeId%3D{Configuration.PlaceId}%26isPlayTogetherGame%3Dfalse+browsertrackerid:{browserId}+robloxLocale:en_us+gameLocale:en_us+channel:");

            if (!success)
                return;

            await Task.Delay(Configuration.Delay);

            _robloxService.KillRoblox();
        }

        public async Task GetVisits()
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                Log.Information("[{0}.{1}] Getting visits...", nameof(VisitsService), nameof(GetVisits));

                while (!cancellationToken.IsCancellationRequested)
                {
                    _configurationService.GetConfiguration();

                    int taskCount = Math.Min(Configuration.RoblosecurityCookies.Count, Configuration.Threads);

                    if (taskCount <= 0)
                    {
                        Log.Error("[{0}.{1}] Failed to get visits.", nameof(VisitsService), nameof(GetVisits));

                        break;
                    }

                    List<Task> tasks = [];
                    List<string> roblosecurityCookies = [.. Configuration.RoblosecurityCookies.OrderBy(_ => Random.Shared.Next())];

                    for (int i = 0; i < taskCount; i++)
                        tasks.Add(GetVisit(roblosecurityCookies[i]));

                    await Task.WhenAll(tasks);
                }

                Log.Information("[{0}.{1}] Done in {2} ms.", nameof(VisitsService), nameof(GetVisits), stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[{0}.{1}] Exception message: {2}", nameof(VisitsService), nameof(GetVisits), ex.Message);
            }
        }
    }
}
