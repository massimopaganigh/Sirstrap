namespace Sirstrap.Core.Tests.Settings
{
    public class IniFormatTests
    {
        [Fact]
        public void IsSectionHeader_DetectsHeaders_AndMatchesTarget()
        {
            Assert.True(IniFormat.IsSectionHeader("[SETTINGS]", "[SETTINGS]", out var matching));
            Assert.True(matching);

            Assert.True(IniFormat.IsSectionHeader("[OTHER]", "[SETTINGS]", out var other));
            Assert.False(other);

            Assert.False(IniFormat.IsSectionHeader("KEY=value", "[SETTINGS]", out var notHeader));
            Assert.False(notHeader);
        }

        [Fact]
        public void IsSectionHeader_IsCaseInsensitive()
        {
            Assert.True(IniFormat.IsSectionHeader("[settings]", "[SETTINGS]", out var matching));
            Assert.True(matching);
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

        [Fact]
        public void ExtractSectionKeys_ReturnsOnlyKeysInsideTargetSection()
        {
            string[] rows =
            [
                "[OTHER]",
                "FOO=1",
                "[SETTINGS]",
                "AUTO_UPDATE=True",
                "CHANNEL_NAME=-beta",
                "[ANOTHER]",
                "BAR=2"
            ];

            var keys = IniFormat.ExtractSectionKeys(rows, "[SETTINGS]");

            Assert.Equal(["AUTO_UPDATE", "CHANNEL_NAME"], keys.OrderBy(k => k));
        }
    }
}
