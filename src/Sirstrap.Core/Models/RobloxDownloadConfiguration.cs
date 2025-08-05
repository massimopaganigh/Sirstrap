using Sirstrap.Core.Services;

namespace Sirstrap.Core.Models
{
    public class RobloxDownloadConfiguration : RobloxDownloadConfigurationBase
    {
        public bool IsMacBinary => BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) || BinaryType.Equals("MacStudio", StringComparison.OrdinalIgnoreCase);

        public string LaunchUri { get; set; } = string.Empty;

        public string OutputPath => Path.Combine(RobloxDownloadConfigurationService.GetCacheDirectory(), $"{VersionHash}.zip");
    }
}