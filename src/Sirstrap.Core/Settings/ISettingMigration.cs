namespace Sirstrap.Core.Settings
{
    public interface ISettingMigration
    {
        string LegacyKey { get; }

        string TargetKey { get; }

        bool ShouldMigrate(IReadOnlyCollection<string> existingKeys);

        void Apply(string legacyValue);
    }
}
