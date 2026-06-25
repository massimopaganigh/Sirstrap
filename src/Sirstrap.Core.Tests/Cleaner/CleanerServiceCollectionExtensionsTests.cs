using Microsoft.Extensions.DependencyInjection;

namespace Sirstrap.Core.Tests.Cleaner
{
    public class CleanerServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddSirstrapCleaner_ResolvesOrchestratorWithFiveSteps()
        {
            using ServiceProvider provider = new ServiceCollection()
                .AddSingleton<IStatusLine>(new FakeStatusLine())
                .AddSirstrapCleaner()
                .BuildServiceProvider();

            Assert.NotNull(provider.GetRequiredService<ICleanupOrchestrator>());
            Assert.NotNull(provider.GetRequiredService<CleanerConfig>());
            Assert.NotNull(provider.GetRequiredService<IRegistryManager>());
            Assert.Equal(5, provider.GetServices<ICleanupStep>().Count());
        }

        [Fact]
        public void AddSirstrapCleaner_Throws_OnNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => CleanerServiceCollectionExtensions.AddSirstrapCleaner(null!));
        }
    }
}
