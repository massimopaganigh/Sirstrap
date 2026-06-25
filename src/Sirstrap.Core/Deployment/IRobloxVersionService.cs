namespace Sirstrap.Core.Deployment
{
    public interface IRobloxVersionService
    {
        Task<string> GetLatestVersionAsync();
    }
}
