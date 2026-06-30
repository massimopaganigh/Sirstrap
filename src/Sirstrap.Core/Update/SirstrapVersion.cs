namespace Sirstrap.Core.Update
{
    public sealed class SirstrapVersion(SirstrapConfiguration sirstrapConfiguration) : ISirstrapVersion
    {
        private static readonly string? _informationalVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0];

        public string Channel => sirstrapConfiguration.SirstrapChannel;

        public Version Current => new(_informationalVersion!);

        public string GetFullVersion()
        {
#if DEBUG
            return $"v{Guid.NewGuid().ToString().ToUpperInvariant()}";
#endif

#pragma warning disable CS0162
            return $"v{Current}{Channel}";
#pragma warning restore CS0162
        }
    }
}
