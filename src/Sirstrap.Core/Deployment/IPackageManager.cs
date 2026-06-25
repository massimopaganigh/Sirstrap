namespace Sirstrap.Core.Deployment
{
    public interface IPackageManager
    {
        Task DownloadMacArchiveAsync(Configuration configuration);

        Task DownloadWindowsArchiveAsync(Configuration configuration);
    }
}
