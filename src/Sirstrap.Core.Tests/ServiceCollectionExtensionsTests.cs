using Microsoft.Extensions.DependencyInjection;

namespace Sirstrap.Core.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private static ServiceProvider BuildProvider() => new ServiceCollection().AddSirstrapCore().BuildServiceProvider();

        [Fact]
        public void AddSirstrapCore_Throws_OnNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddSirstrapCore(null!));
        }

        [Theory]
        [InlineData(typeof(SirstrapConfiguration))]
        [InlineData(typeof(IPerformanceTelemetry))]
        [InlineData(typeof(ICdnTelemetry))]
        [InlineData(typeof(ILastLogSink))]
        [InlineData(typeof(ICdnUriNormalizer))]
        [InlineData(typeof(ICdnCandidateProvider))]
        [InlineData(typeof(ICdnProbeUriFactory))]
        [InlineData(typeof(ICdnProber))]
        [InlineData(typeof(ICdnResolver))]
        [InlineData(typeof(IRobloxUriFactory))]
        [InlineData(typeof(IRobloxVersionService))]
        [InlineData(typeof(IPackageManager))]
        [InlineData(typeof(IInstaller))]
        [InlineData(typeof(IRobloxDownloader))]
        [InlineData(typeof(IRobloxProcessService))]
        [InlineData(typeof(ISingletonManager))]
        [InlineData(typeof(IIncognitoManager))]
        [InlineData(typeof(IRobloxLauncher))]
        [InlineData(typeof(IServerLocationService))]
        [InlineData(typeof(RobloxActivityWatcher))]
        [InlineData(typeof(ISettingsRegistry))]
        [InlineData(typeof(ISettingsService))]
        [InlineData(typeof(ISirstrapVersion))]
        [InlineData(typeof(ISirstrapUpdateService))]
        [InlineData(typeof(IUacService))]
        [InlineData(typeof(IProtocolHandlerRegistrar))]
        [InlineData(typeof(IRegistryManager))]
        [InlineData(typeof(IUninstallService))]
        [InlineData(typeof(ISirHurtService))]
        [InlineData(typeof(IPathManager))]
        [InlineData(typeof(IIpcService))]
        public void AddSirstrapCore_ResolvesService(Type serviceType)
        {
            using ServiceProvider provider = BuildProvider();

            Assert.NotNull(provider.GetRequiredService(serviceType));
        }

        [Fact]
        public void AddSirstrapCore_SharesSingleConfigurationInstance()
        {
            using ServiceProvider provider = BuildProvider();

            Assert.Same(provider.GetRequiredService<SirstrapConfiguration>(), provider.GetRequiredService<SirstrapConfiguration>());
        }

        [Fact]
        public void AddSirstrapCore_RegistersSharedHttpClient()
        {
            using ServiceProvider provider = BuildProvider();

            Assert.NotNull(provider.GetRequiredService<HttpClient>());
        }
    }
}
