namespace Magenta.Core.Interfaces
{
    public interface IXCsrfTokenService
    {
        public Task<(bool, string)> GetXCsrfToken(string roblosecurityCookie);
    }
}
