namespace Sirstrap.Core.Tests.Support
{
    public sealed class FakeSirstrapUpdateService : ISirstrapUpdateService
    {
        public int UpdateCalls { get; private set; }

        public Task<string> GetLatestChangelogAsync() => Task.FromResult(string.Empty);

        public Task UpdateAsync(SirstrapType sirstrapType, string[] args)
        {
            UpdateCalls++;

            return Task.CompletedTask;
        }
    }

    public sealed class FakeRobloxVersionService(string version) : IRobloxVersionService
    {
        public int Calls { get; private set; }

        public Task<string> GetLatestVersionAsync()
        {
            Calls++;

            return Task.FromResult(version);
        }
    }

    public sealed class FakePackageManager : IPackageManager
    {
        public int WindowsCalls { get; private set; }

        public int MacCalls { get; private set; }

        public Task DownloadMacArchiveAsync(Configuration configuration)
        {
            MacCalls++;

            return Task.CompletedTask;
        }

        public Task DownloadWindowsArchiveAsync(Configuration configuration)
        {
            WindowsCalls++;

            return Task.CompletedTask;
        }
    }

    public sealed class FakeCdnResolver : ICdnResolver
    {
        public int Calls { get; private set; }

        public Task<string> ResolveAsync(Configuration configuration, CancellationToken cancellationToken = default)
        {
            Calls++;

            return Task.FromResult(RobloxCdnService.DefaultBaseUri);
        }
    }

    public sealed class FakeInstaller : IInstaller
    {
        public int Calls { get; private set; }

        public void Install(Configuration configuration) => Calls++;
    }

    public sealed class FakeRobloxLauncher(bool result) : IRobloxLauncher
    {
        public int Calls { get; private set; }

        public bool Launch(Configuration configuration)
        {
            Calls++;

            return result;
        }
    }

    public sealed class FakeFFlagManager : IFFlagManager
    {
        public int DeployCalls { get; private set; }

        public string GetFFlagsPath() => "fake/path/ClientAppSettings.json";

        public Dictionary<string, object> LoadFFlags() => [];

        public void SaveFFlags(Dictionary<string, object> flags) { }

        public void DeployFFlags(string targetDirectory)
        {
            DeployCalls++;
        }
    }
}
