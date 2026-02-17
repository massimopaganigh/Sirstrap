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

        [Fact]
        public void ReadCookies_WithSingleCookie_ShouldReturnSingleElement()
        {
            string tempFile = Path.GetTempFileName();

            try
            {
                File.WriteAllText(tempFile, "only-cookie");

                string[] cookies = VisitService.ReadCookies(tempFile);

                Assert.Single(cookies);
                Assert.Equal("only-cookie", cookies[0]);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ReadCookies_ShouldPreserveLeadingAndTrailingSpacesInValues()
        {
            string tempFile = Path.GetTempFileName();

            try
            {
                File.WriteAllLines(tempFile, ["  cookie-with-spaces  "]);

                string[] cookies = VisitService.ReadCookies(tempFile);

                Assert.Single(cookies);
                Assert.Equal("  cookie-with-spaces  ", cookies[0]);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ReadCookies_WithMissingFile_ShouldIncludePathInExceptionMessage()
        {
            string fakePath = "/some/missing/cookies.txt";

            var ex = Assert.Throws<FileNotFoundException>(() => VisitService.ReadCookies(fakePath));

            Assert.Contains(fakePath, ex.Message);
        }

        [Fact]
        public void BuildLaunchUrl_ShouldEndWithChannelSuffix()
        {
            string result = VisitService.BuildLaunchUrl("ticket", 100, 200);

            Assert.EndsWith("channel:", result);
        }

        [Fact]
        public void BuildLaunchUrl_ShouldUsePlusAsDelimiter()
        {
            string result = VisitService.BuildLaunchUrl("ticket", 100, 200);

            string[] segments = result.Split('+');

            Assert.True(segments.Length >= 8);
            Assert.Equal("roblox-player:1", segments[0]);
            Assert.Equal("launchmode:play", segments[1]);
            Assert.Equal("gameinfo:ticket", segments[2]);
            Assert.Equal("launchtime:100", segments[3]);
        }

        [Fact]
        public void BuildLaunchUrl_WithLargePlaceId_ShouldFormatCorrectly()
        {
            long largePlaceId = 9_999_999_999;

            string result = VisitService.BuildLaunchUrl("ticket", 100, largePlaceId);

            Assert.Contains($"placeId%3D{largePlaceId}", result);
        }

        [Fact]
        public void BuildLaunchUrl_BrowserIdShouldAppearTwice()
        {
            long browserId = 555555;

            string result = VisitService.BuildLaunchUrl("ticket", browserId, 1);

            int count = result.Split(browserId.ToString()).Length - 1;

            Assert.True(count >= 2);
        }

        [Fact]
        public void ReadCookies_WithMixedValidAndBlankLines_ShouldReturnOnlyValid()
        {
            string tempFile = Path.GetTempFileName();

            try
            {
                File.WriteAllLines(tempFile, ["", "a", "", "  ", "b", "\t", "c", ""]);

                string[] cookies = VisitService.ReadCookies(tempFile);

                Assert.Equal(3, cookies.Length);
                Assert.Equal(["a", "b", "c"], cookies);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
