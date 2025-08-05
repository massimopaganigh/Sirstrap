using Sirstrap.Core.Models;

namespace Sirstrap.Core.Interfaces
{
    public interface IRobloxDownloadConfigurationService
    {
        static abstract string GetCacheDirectory();

        public void ClearCacheDirectory();

        public RobloxDownloadConfiguration ParseConfiguration(string[] arguments);
    }
}