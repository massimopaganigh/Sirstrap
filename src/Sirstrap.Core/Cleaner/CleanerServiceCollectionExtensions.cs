namespace Sirstrap.Core.Cleaner
{
    public static class CleanerServiceCollectionExtensions
    {
        public static IServiceCollection AddSirstrapCleaner(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<CleanerConfig>();
            services.TryAddSingleton<IFileSystem, FileSystemService>();
            services.TryAddSingleton<IProcessManager, ProcessManager>();
            services.TryAddSingleton<IUserProfileProvider, UserProfileProvider>();
            services.TryAddSingleton<IFolderDeleter, FolderDeleter>();
            services.TryAddSingleton<ISelectiveFolderCleaner, SelectiveFolderCleaner>();
            services.TryAddSingleton<IStatusLine, ConsoleStatusLine>();
            services.TryAddSingleton<IUserInteraction, ConsoleUserInteraction>();
            services.TryAddSingleton<IRegistryManager, RegistryManager>();

            services.AddSingleton<ICleanupStep, ProcessCloser>();
            services.AddSingleton<ICleanupStep, SystemFoldersCleanupStep>();
            services.AddSingleton<ICleanupStep, UserFoldersCleanupStep>();
            services.AddSingleton<ICleanupStep, RegistryCleanupStep>();
            services.AddSingleton<ICleanupStep, TempFolderCleaner>();

            services.TryAddSingleton<ICleanupOrchestrator, CleanupOrchestrator>();

            return services;
        }
    }
}
