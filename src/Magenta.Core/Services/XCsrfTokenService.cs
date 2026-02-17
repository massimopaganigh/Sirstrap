namespace Magenta.Core.Services
{
    public class XCsrfTokenService : BaseService, IXCsrfTokenService
    {
        public async Task<(bool, string)> GetXCsrfToken(string roblosecurityCookie)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                Log.Information("[{0}.{1}] Getting {2}...", nameof(RbxAuthenticationTicketService), nameof(GetXCsrfToken), Constants.X_CSRF_TOKEN_HEADER_NAME);

                HttpResponseMessage xCsrfTokenResponse = await _httpClient.SendAsync(GetRequest(Constants.X_CSRF_TOKEN_REQUEST_URI, roblosecurityCookie));

                if (!xCsrfTokenResponse.Headers.TryGetValues(Constants.X_CSRF_TOKEN_HEADER_NAME, out IEnumerable<string>? xCsrfTokenResponseValues))
                {
                    Log.Error("[{0}.{1}] Failed to get {2}.", nameof(RbxAuthenticationTicketService), nameof(GetXCsrfToken), Constants.X_CSRF_TOKEN_HEADER_NAME);

                    return (false, string.Empty);
                }

                Log.Information("[{0}.{1}] Done in {2} ms.", nameof(RbxAuthenticationTicketService), nameof(GetXCsrfToken), stopwatch.ElapsedMilliseconds);

                return (true, xCsrfTokenResponseValues.FirstOrDefault(string.Empty));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[{0}.{1}] Exception message: {2}", nameof(RbxAuthenticationTicketService), nameof(GetXCsrfToken), ex.Message);

                return (false, string.Empty);
            }
        }
    }
}
