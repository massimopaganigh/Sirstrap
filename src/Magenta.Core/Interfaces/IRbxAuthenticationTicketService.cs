namespace Magenta.Core.Interfaces
{
    public interface IRbxAuthenticationTicketService
    {
        public Task<(bool, string)> GetRbxAuthenticationTicket(string roblosecurityCookie, string xCsrfToken);
    }
}
