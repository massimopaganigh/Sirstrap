namespace Sirstrap.Core.Settings
{
    public interface ISettingsRegistry
    {
        IReadOnlyList<ISettingMigration> Migrations { get; }

        IReadOnlyList<ISetting> Settings { get; }
    }
}
