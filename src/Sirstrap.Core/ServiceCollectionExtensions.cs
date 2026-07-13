namespace Sirstrap.Core
{
    public static class ServiceCollectionExtensions
    {
        private const int HTTP_TIMEOUT_MINUTES = 5;
        private const string USER_AGENT = "Sirstrap";

        public static IServiceCollection AddSirstrapCore(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton(CreateHttpClient());
            services.TryAddSingleton<SirstrapConfiguration>();

            AddTelemetry(services);
            AddSettings(services);
            AddWeao(services);
            AddCdn(services);
            AddDeployment(services);
            AddLaunch(services);
            AddActivity(services);
            AddWindows(services);
            AddUpdate(services);

            services.TryAddSingleton<ISirHurtService, SirHurtService>();
            services.TryAddSingleton<IPathManager, PathManager>();
            services.TryAddSingleton<IIpcService, IpcService>();

            return services;
        }

        private static void AddActivity(IServiceCollection services)
        {
            services.TryAddSingleton<IServerLocationService, ServerLocationService>();
            services.TryAddTransient<RobloxActivityWatcher>();
        }

        private static void AddCdn(IServiceCollection services)
        {
            services.TryAddSingleton<ICdnUriNormalizer, CdnUriNormalizer>();
            services.TryAddSingleton<ICdnCandidateProvider, DefaultCdnCandidateProvider>();
            services.TryAddSingleton<ICdnProbeUriFactory, CdnProbeUriFactory>();
            services.TryAddSingleton<ICdnProber, HttpCdnProber>();
            services.TryAddSingleton<ICdnResolver, CdnResolver>();
        }

        private static void AddDeployment(IServiceCollection services)
        {
            services.TryAddSingleton<IRobloxUriFactory, RobloxUriFactory>();
            services.TryAddSingleton<RobloxClientVersionApi>();
            services.TryAddSingleton<SirHurtVersionApi>();
            services.TryAddSingleton<IRobloxVersionService, RobloxVersionService>();
            services.TryAddSingleton<IPackageManager, PackageManager>();
            services.TryAddSingleton<IInstaller, Installer>();
            services.TryAddSingleton<IRobloxDownloader, RobloxDownloader>();
        }

        private static void AddLaunch(IServiceCollection services)
        {
            services.TryAddSingleton<IFastFlagService, FastFlagService>();
            services.TryAddSingleton<IRobloxProcessService, RobloxProcessService>();
            services.TryAddSingleton<ISingletonManager, SingletonManager>();
            services.TryAddSingleton<IIncognitoManager, IncognitoManager>();
            services.TryAddSingleton<IRobloxLauncher, RobloxLauncher>();
        }

        private static void AddSettings(IServiceCollection services)
        {
            services.TryAddSingleton<ISettingsRegistry, SettingsRegistry>();
            services.TryAddSingleton<ISettingsService, SettingsService>();
        }

        private static void AddWeao(IServiceCollection services)
        {
            services.TryAddSingleton<IWeaoService, WeaoService>();
        }

        private static void AddTelemetry(IServiceCollection services)
        {
            services.TryAddSingleton<IPerformanceTelemetry, SentryPerformanceTelemetry>();
            services.TryAddSingleton<ICdnTelemetry, SentryCdnTelemetry>();
            services.TryAddSingleton<ILastLogSink, LastLogSink>();
        }

        private static void AddUpdate(IServiceCollection services)
        {
            services.TryAddSingleton<ISirstrapVersion, SirstrapVersion>();
            services.TryAddSingleton<GitHubReleaseClient>();
            services.TryAddSingleton<UpdateApplier>();
            services.TryAddSingleton<ISirstrapUpdateService, SirstrapUpdateService>();
        }

        private static void AddWindows(IServiceCollection services)
        {
            services.TryAddSingleton<IUacService, UacService>();
            services.TryAddSingleton<IProtocolHandlerRegistrar, ProtocolHandlerRegistrar>();
            services.TryAddSingleton<IRegistryManager, RegistryManager>();
            services.TryAddSingleton<IUninstallService, UninstallService>();
        }

        private static HttpClient CreateHttpClient()
        {
            HttpClient httpClient = new()
            {
                Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT_MINUTES)
            };

            httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

            return httpClient;
        }
    }
}
