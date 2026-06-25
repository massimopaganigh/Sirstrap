namespace Sirstrap.Core.Settings
{
    public sealed class SettingMigration : ISettingMigration
    {
        private readonly Action<string> _apply;

        public SettingMigration(string legacyKey, string targetKey, Action<string> apply)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(legacyKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(targetKey);
            ArgumentNullException.ThrowIfNull(apply);

            LegacyKey = legacyKey;
            TargetKey = targetKey;
            _apply = apply;
        }

        public string LegacyKey { get; }

        public string TargetKey { get; }

        public bool ShouldMigrate(IReadOnlyCollection<string> existingKeys)
        {
            ArgumentNullException.ThrowIfNull(existingKeys);

            return !existingKeys.Any(key => string.Equals(key, TargetKey, StringComparison.OrdinalIgnoreCase));
        }

        public void Apply(string legacyValue) => _apply(legacyValue);
    }
}
