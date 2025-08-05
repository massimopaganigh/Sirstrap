namespace Sirstrap.Core.Interfaces
{
    public interface IRobloxDownloadService
    {
        public Task DownloadForMacAsync();

        public Task DownloadForWindowsAsync();
    }
}