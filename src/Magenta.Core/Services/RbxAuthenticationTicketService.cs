namespace Magenta.Core.Services
{
    public class RbxAuthenticationTicketService : BaseService, IRbxAuthenticationTicketService
    {
        public async Task<(bool, string)> GetRbxAuthenticationTicket(string roblosecurityCookie, string xCsrfToken)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                Log.Information("[{0}.{1}] Getting {2}...", nameof(RbxAuthenticationTicketService), nameof(GetRbxAuthenticationTicket), Constants.RBX_AUTHENTICATION_TICKET_HEADER_NAME);

                HttpRequestMessage rbxAuthenticationTicketRequest = GetRequest(Constants.RBX_AUTHENTICATION_TICKET_REQUEST_URI, roblosecurityCookie);

                rbxAuthenticationTicketRequest.Headers.Add(Constants.X_CSRF_TOKEN_HEADER_NAME, xCsrfToken);
                rbxAuthenticationTicketRequest.Headers.Referrer = new Uri(string.Format(Constants.GAMES_URI, Configuration.PlaceId));

                HttpResponseMessage rbxAuthenticationTicketResponse = await _httpClient.SendAsync(rbxAuthenticationTicketRequest);

                if (!rbxAuthenticationTicketResponse.Headers.TryGetValues(Constants.RBX_AUTHENTICATION_TICKET_HEADER_NAME, out IEnumerable<string>? rbxAuthenticationTicketResponseValues))
                {
                    Log.Error("[{0}.{1}] Failed to get {2}.", nameof(RbxAuthenticationTicketService), nameof(GetRbxAuthenticationTicket), Constants.RBX_AUTHENTICATION_TICKET_HEADER_NAME);

                    return (false, string.Empty);
                }

                Log.Information("[{0}.{1}] Done in {2} ms.", nameof(RbxAuthenticationTicketService), nameof(GetRbxAuthenticationTicket), stopwatch.ElapsedMilliseconds);

                return (true, rbxAuthenticationTicketResponseValues.FirstOrDefault(string.Empty));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[{0}.{1}] Exception message: {2}", nameof(RbxAuthenticationTicketService), nameof(GetRbxAuthenticationTicket), ex.Message);

                return (false, string.Empty);
            }
        }
    }
}
