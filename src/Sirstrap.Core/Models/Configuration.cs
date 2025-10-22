namespace Sirstrap.Core.Models
{
    public class Configuration : ConfigurationBase
    {
        #region PRIVATE METHODS
        private static string GetCacheDirectory()
        {
            var cacheDirectory = Directories.CacheDirectory;

            cacheDirectory.BetterDirectoryCreate();

            return cacheDirectory;
        }
        #endregion

        public static void ClearCacheDirectory() => GetCacheDirectory().BetterDirectoryClear();

        public string DownloadOutput => Path.Combine(GetCacheDirectory(), $"{VersionHash}.zip");

        [Obsolete]
        public bool IsMacBinary => BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) || BinaryType.Equals("MacStudio", StringComparison.OrdinalIgnoreCase); // TODO: remove Mac support

        public string LaunchUri { get; set; } = string.Empty;
    }
}
