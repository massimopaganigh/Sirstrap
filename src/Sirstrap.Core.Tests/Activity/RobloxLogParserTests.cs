namespace Sirstrap.Core.Tests.Activity
{
    public class RobloxLogParserTests
    {
        [Theory]
        [InlineData("UDMUX server at 203.0.113.5 ready", "203.0.113.5")]
        [InlineData("GameHost bound to 198.51.100.9", "198.51.100.9")]
        public void ExtractServerIp_ReturnsIp_ForUdmuxOrGameHostLines(string line, string expected)
        {
            Assert.Equal(expected, RobloxLogParser.ExtractServerIp(line));
        }

        [Fact]
        public void ExtractServerIp_ReturnsPrivateIp_ForUdmuxLine()
        {
            Assert.Equal("192.168.1.10", RobloxLogParser.ExtractServerIp("UDMUX 192.168.1.10"));
        }

        [Fact]
        public void ExtractServerIp_ReturnsPublicIp_ForServerOrConnectLines()
        {
            Assert.Equal("8.8.8.8", RobloxLogParser.ExtractServerIp("connecting to server 8.8.8.8"));
        }

        [Theory]
        [InlineData("connecting to server 127.0.0.1")]
        [InlineData("server 192.168.0.1")]
        [InlineData("server 10.1.2.3")]
        [InlineData("server 172.16.5.5")]
        [InlineData("server 172.31.255.255")]
        public void ExtractServerIp_ReturnsNull_ForPrivateIpsInServerContext(string line)
        {
            Assert.Null(RobloxLogParser.ExtractServerIp(line));
        }

        [Fact]
        public void ExtractServerIp_ReturnsPublicIp_ForOutOfRange172()
        {
            Assert.Equal("172.32.0.1", RobloxLogParser.ExtractServerIp("server 172.32.0.1"));
            Assert.Equal("172.15.0.1", RobloxLogParser.ExtractServerIp("server 172.15.0.1"));
        }

        [Fact]
        public void ExtractServerIp_ReturnsNull_WhenNoKeywordPresent()
        {
            Assert.Null(RobloxLogParser.ExtractServerIp("random line with 1.2.3.4 in it"));
        }

        [Fact]
        public void ExtractServerIp_ReturnsNull_WhenNoIpPresent()
        {
            Assert.Null(RobloxLogParser.ExtractServerIp("connecting to server somewhere"));
        }
    }
}
