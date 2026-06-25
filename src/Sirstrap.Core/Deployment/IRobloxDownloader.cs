namespace Sirstrap.Core.Deployment
{
    public interface IRobloxDownloader
    {
        Task ExecuteAsync(string[] args, SirstrapType sirstrapType);
    }
}
