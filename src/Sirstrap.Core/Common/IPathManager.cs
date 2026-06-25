namespace Sirstrap.Core.Common
{
    public interface IPathManager
    {
        void ClearCacheDirectory();

        string GetCacheDirectory();

        string GetExtractionPath(string versionHash);

        string GetLogsPath();

        string GetOutputPath(Configuration configuration);

        void PurgeOldLogs(int maxFiles = 100);

        void PurgePreviousInstallationPath();
    }
}
