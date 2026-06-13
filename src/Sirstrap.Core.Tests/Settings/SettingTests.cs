namespace Sirstrap.Core.Tests.Settings
{
    public class SettingTests
    {
        [Fact]
        public void Setting_ReadsWritesAndEmits()
        {
            string store = "initial";
            int metricCalls = 0;

            Setting setting = new("KEY", () => store, v => store = v, () => metricCalls++);

            Assert.Equal("KEY", setting.Key);
            Assert.Equal("initial", setting.Read());

            setting.Write("updated");
            Assert.Equal("updated", store);

            setting.EmitMetric();
            Assert.Equal(1, metricCalls);
        }

        [Fact]
        public void Setting_EmitMetric_IsNoop_WhenEmitterNull()
        {
            Setting setting = new("KEY", () => "v", _ => { });

            Assert.Null(Record.Exception(setting.EmitMetric));
        }

        [Fact]
        public void Setting_Throws_OnInvalidArguments()
        {
            Assert.Throws<ArgumentException>(() => new Setting(" ", () => "v", _ => { }));
            Assert.Throws<ArgumentNullException>(() => new Setting("KEY", null!, _ => { }));
            Assert.Throws<ArgumentNullException>(() => new Setting("KEY", () => "v", null!));
        }

        [Fact]
        public void SettingMigration_AppliesAndDecidesMigration()
        {
            string applied = string.Empty;
            SettingMigration migration = new("OLD_KEY", "NEW_KEY", v => applied = v);

            Assert.Equal("OLD_KEY", migration.LegacyKey);
            Assert.Equal("NEW_KEY", migration.TargetKey);

            migration.Apply("value");
            Assert.Equal("value", applied);
        }

        [Fact]
        public void SettingMigration_ShouldMigrate_OnlyWhenTargetAbsent()
        {
            SettingMigration migration = new("OLD_KEY", "NEW_KEY", _ => { });

            Assert.True(migration.ShouldMigrate(["OTHER"]));
            Assert.False(migration.ShouldMigrate(["new_key"]));
        }

        [Fact]
        public void SettingMigration_Throws_OnInvalidArguments()
        {
            Assert.Throws<ArgumentException>(() => new SettingMigration(" ", "NEW", _ => { }));
            Assert.Throws<ArgumentException>(() => new SettingMigration("OLD", " ", _ => { }));
            Assert.Throws<ArgumentNullException>(() => new SettingMigration("OLD", "NEW", null!));
            Assert.Throws<ArgumentNullException>(() => new SettingMigration("OLD", "NEW", _ => { }).ShouldMigrate(null!));
        }
    }
}
