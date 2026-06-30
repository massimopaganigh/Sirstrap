namespace Sirstrap.Core.Tests.Settings
{
    public class IniFormatTests
    {
        [Theory]
        [InlineData("[SETTINGS]", true, SettingsSection.Settings)]
        [InlineData("[settings]", true, SettingsSection.Settings)]
        [InlineData("[STATE]", true, SettingsSection.State)]
        [InlineData("[state]", true, SettingsSection.State)]
        public void TryParseSectionHeader_RecognizesKnownSections(string row, bool expected, SettingsSection section)
        {
            bool result = IniFormat.TryParseSectionHeader(row, out var actual);

            Assert.Equal(expected, result);
            Assert.Equal(section, actual);
        }

        [Fact]
        public void TryParseSectionHeader_ReturnsTrueButNullSection_ForUnknownHeader()
        {
            Assert.True(IniFormat.TryParseSectionHeader("[OTHER]", out var section));
            Assert.Null(section);
        }

        [Fact]
        public void TryParseSectionHeader_ReturnsFalse_ForNonHeader()
        {
            Assert.False(IniFormat.TryParseSectionHeader("KEY=value", out var section));
            Assert.Null(section);
        }

        [Theory]
        [InlineData("KEY=value", true, "KEY", "value")]
        [InlineData("  KEY = value with spaces ", true, "KEY", "value with spaces")]
        [InlineData("KEY=a=b", true, "KEY", "a=b")]
        [InlineData("no-equals", false, "", "")]
        [InlineData("=value", false, "", "")]
        public void TryParseRow_ParsesKeyValuePairs(string row, bool expected, string key, string value)
        {
            bool result = IniFormat.TryParseRow(row.Trim(), out var actualKey, out var actualValue);

            Assert.Equal(expected, result);

            if (expected)
            {
                Assert.Equal(key, actualKey);
                Assert.Equal(value, actualValue);
            }
        }
    }
}
