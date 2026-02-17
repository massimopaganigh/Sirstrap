namespace Sirstrap.Core
{
    public interface IRobloxAuthClient : IDisposable
    {
        Task<string> GetCsrfTokenAsync(string roblosecurityToken);
        Task<string> GetAuthTicketAsync(string roblosecurityToken, string csrfToken, long placeId);
    }
}
