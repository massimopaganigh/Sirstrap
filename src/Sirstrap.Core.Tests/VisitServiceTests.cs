namespace Sirstrap.Core.Tests
{
    public class VisitServiceTests
    {
        [Fact]
        public void BuildLaunchUrl_ShouldContainAllRequiredComponents()
        {
            string authTicket = "test-auth-ticket";
            long browserId = 123456;
            long placeId = 789;

            string result = VisitService.BuildLaunchUrl(authTicket, browserId, placeId);

            Assert.StartsWith("roblox-player:1", result);
            Assert.Contains("launchmode:play", result);
            Assert.Contains($"gameinfo:{authTicket}", result);
            Assert.Contains($"launchtime:{browserId}", result);
            Assert.Contains($"placeId%3D{placeId}", result);
            Assert.Contains($"browsertrackerid:{browserId}", result);
            Assert.Contains("robloxLocale:en_us", result);
            Assert.Contains("gameLocale:en_us", result);
        }

        [Fact]
        public void BuildLaunchUrl_ShouldContainEncodedPlaceLauncherUrl()
        {
            string result = VisitService.BuildLaunchUrl("ticket", 100, 200);

            Assert.Contains("placelauncherurl:https%3A%2F%2Fassetgame.roblox.com%2Fgame%2FPlaceLauncher.ashx", result);
            Assert.Contains("request%3DRequestGame", result);
            Assert.Contains("isPlayTogetherGame%3Dfalse", result);
        }

        [Fact]
        public void ReadCookies_WithValidFile_ShouldReturnNonEmptyLines()
        {
            string tempFile = Path.GetTempFileName();

            try
            {
                File.WriteAllLines(tempFile, ["cookie1", "", "cookie2", "  ", "cookie3"]);

                string[] cookies = VisitService.ReadCookies(tempFile);

                Assert.Equal(3, cookies.Length);
                Assert.Equal("cookie1", cookies[0]);
                Assert.Equal("cookie2", cookies[1]);
                Assert.Equal("cookie3", cookies[2]);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ReadCookies_WithMissingFile_ShouldThrowFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => VisitService.ReadCookies("/nonexistent/path/cookies.txt"));
        }

        [Fact]
        public void ReadCookies_WithEmptyFile_ShouldReturnEmptyArray()
        {
            string tempFile = Path.GetTempFileName();

            try
            {
                File.WriteAllText(tempFile, string.Empty);

                string[] cookies = VisitService.ReadCookies(tempFile);

                Assert.Empty(cookies);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ReadCookies_WithWhitespaceOnlyLines_ShouldReturnEmptyArray()
        {
            string tempFile = Path.GetTempFileName();

            try
            {
                File.WriteAllLines(tempFile, ["", "  ", "\t"]);

                string[] cookies = VisitService.ReadCookies(tempFile);

                Assert.Empty(cookies);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
