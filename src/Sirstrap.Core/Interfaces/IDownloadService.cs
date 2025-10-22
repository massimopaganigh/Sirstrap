namespace Sirstrap.Core.Interfaces
{
    public interface IDownloadService
    {
        public Task MacDownloadAsync();

        public Task WindowsDownloadAsync();
    }
}
