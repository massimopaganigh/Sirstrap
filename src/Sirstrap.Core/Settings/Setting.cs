namespace Sirstrap.Core.Settings
{
    public static class Setting
    {
        public static SettingDefinition Bool(string key, Func<bool> getter, Action<bool> setter, Action? metric = null, IReadOnlyList<string>? legacyKeys = null, Func<string, string>? valueMigrator = null) => new(key, SettingsSection.Settings, () => getter().ToString(), value =>
        {
            if (bool.TryParse(value, out var v))
                setter(v);
        }, legacyKeys ?? [], valueMigrator, metric);

        public static SettingDefinition Enum<T>(string key, Func<T> getter, Action<T> setter, Action? metric = null, IReadOnlyList<string>? legacyKeys = null, Func<string, string>? valueMigrator = null) where T : struct, Enum => new(key, SettingsSection.Settings, () => getter().ToString(), value =>
        {
            if (System.Enum.TryParse<T>(value, true, out var v))
                setter(v);
        }, legacyKeys ?? [], valueMigrator, metric);

        public static SettingDefinition StateString(string key, Func<string> getter, Action<string> setter, Action? metric = null, IReadOnlyList<string>? legacyKeys = null, Func<string, string>? valueMigrator = null) => new(key, SettingsSection.State, getter, setter, legacyKeys ?? [], valueMigrator, metric);

        public static SettingDefinition String(string key, Func<string> getter, Action<string> setter, Action? metric = null, IReadOnlyList<string>? legacyKeys = null, Func<string, string>? valueMigrator = null) => new(key, SettingsSection.Settings, getter, setter, legacyKeys ?? [], valueMigrator, metric);
    }
}
