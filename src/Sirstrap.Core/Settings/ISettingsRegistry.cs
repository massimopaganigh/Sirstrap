namespace Sirstrap.Core.Settings
{
    public interface ISettingsRegistry
    {
        IReadOnlyList<SettingDefinition> Settings { get; }
    }
}
