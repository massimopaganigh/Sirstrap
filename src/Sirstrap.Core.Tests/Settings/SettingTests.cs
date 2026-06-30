namespace Sirstrap.Core.Tests.Settings
{
    public class SettingTests
    {
        [Fact]
        public void Bool_ReadsFormatsAndParses()
        {
            bool store = false;
            SettingDefinition definition = Setting.Bool("KEY", () => store, v => store = v);

            Assert.Equal("KEY", definition.Key);
            Assert.Equal(SettingsSection.Settings, definition.Section);
            Assert.Equal("False", definition.Getter());

            definition.Setter("True");
            Assert.True(store);

            definition.Setter("not-a-bool");
            Assert.True(store);
        }

        [Fact]
        public void Enum_ParsesCaseInsensitively_AndIgnoresGarbage()
        {
            TrayMode store = TrayMode.None;
            SettingDefinition definition = Setting.Enum("TRAY_MODE", () => store, v => store = v);

            definition.Setter("onroblox");
            Assert.Equal(TrayMode.OnRoblox, store);

            definition.Setter("garbage");
            Assert.Equal(TrayMode.OnRoblox, store);
        }

        [Fact]
        public void String_PassesValueThrough()
        {
            string store = "initial";
            SettingDefinition definition = Setting.String("KEY", () => store, v => store = v);

            Assert.Equal("initial", definition.Getter());

            definition.Setter("updated");
            Assert.Equal("updated", store);
        }

        [Fact]
        public void StateString_LivesInStateSection()
        {
            string store = string.Empty;
            SettingDefinition definition = Setting.StateString("KEY", () => store, v => store = v);

            Assert.Equal(SettingsSection.State, definition.Section);
        }

        [Fact]
        public void Factories_CarryMetricAndLegacyKeysAndMigrator()
        {
            int metricCalls = 0;
            SettingDefinition definition = Setting.String("KEY", () => "v", _ => { }, () => metricCalls++, ["OLD_KEY"], value => value.ToUpperInvariant());

            Assert.Equal(["OLD_KEY"], definition.LegacyKeys);
            Assert.Equal("ABC", definition.ValueMigrator!("abc"));

            definition.MetricEmitter!();
            Assert.Equal(1, metricCalls);
        }

        [Fact]
        public void Factories_DefaultMetadataIsEmpty()
        {
            SettingDefinition definition = Setting.Bool("KEY", () => true, _ => { });

            Assert.Empty(definition.LegacyKeys);
            Assert.Null(definition.ValueMigrator);
            Assert.Null(definition.MetricEmitter);
        }
    }
}
