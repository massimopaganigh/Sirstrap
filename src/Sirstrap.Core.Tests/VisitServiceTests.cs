using System.Net;

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
        public async Task GetCsrfTokenAsync_ShouldReturnTokenFromResponseHeaders()
        {
            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                response.Headers.Add("x-csrf-token", "test-csrf-token");
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);
            using var service = new VisitService(httpClient);

            string token = await service.GetCsrfTokenAsync("test-cookie");

            Assert.Equal("test-csrf-token", token);
        }

        [Fact]
        public async Task GetCsrfTokenAsync_ShouldThrowWhenNoCsrfHeader()
        {
            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);
            using var service = new VisitService(httpClient);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetCsrfTokenAsync("test-cookie"));
        }

        [Fact]
        public async Task GetAuthTicketAsync_ShouldReturnTicketFromResponseHeaders()
        {
            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Headers.Add("rbx-authentication-ticket", "test-auth-ticket");
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);
            using var service = new VisitService(httpClient);

            string ticket = await service.GetAuthTicketAsync("test-cookie", "test-csrf", 12345);

            Assert.Equal("test-auth-ticket", ticket);
        }

        [Fact]
        public async Task GetAuthTicketAsync_ShouldRetryWithFreshCsrfTokenOn403()
        {
            int callCount = 0;

            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                callCount++;

                if (callCount == 1)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                    response.Headers.Add("x-csrf-token", "fresh-csrf-token");
                    return Task.FromResult(response);
                }
                else
                {
                    Assert.Contains("fresh-csrf-token", request.Headers.GetValues("x-csrf-token"));

                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Headers.Add("rbx-authentication-ticket", "test-auth-ticket");
                    return Task.FromResult(response);
                }
            });

            using var httpClient = new HttpClient(handler);
            using var service = new VisitService(httpClient);

            string ticket = await service.GetAuthTicketAsync("test-cookie", "old-csrf", 12345);

            Assert.Equal("test-auth-ticket", ticket);
            Assert.Equal(2, callCount);
        }

        [Fact]
        public async Task GetAuthTicketAsync_ShouldThrowAfterMaxRetries()
        {
            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);
            using var service = new VisitService(httpClient);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetAuthTicketAsync("test-cookie", "test-csrf", 12345));
        }

        [Fact]
        public async Task GetCsrfTokenAsync_ShouldSendRequiredHeaders()
        {
            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Contains(".ROBLOSECURITY=test-cookie", request.Headers.GetValues("Cookie").First());
                Assert.NotNull(request.Content);
                Assert.Equal(0, request.Content!.Headers.ContentLength);

                var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                response.Headers.Add("x-csrf-token", "csrf-value");
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);
            using var service = new VisitService(httpClient);

            await service.GetCsrfTokenAsync("test-cookie");
        }

        [Fact]
        public async Task GetAuthTicketAsync_ShouldSendRequiredHeaders()
        {
            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Contains(".ROBLOSECURITY=test-cookie", request.Headers.GetValues("Cookie").First());
                Assert.Contains("test-csrf", request.Headers.GetValues("x-csrf-token"));
                Assert.NotNull(request.Headers.Referrer);
                Assert.Contains("12345", request.Headers.Referrer!.ToString());
                Assert.NotNull(request.Content);
                Assert.Equal(0, request.Content!.Headers.ContentLength);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Headers.Add("rbx-authentication-ticket", "ticket-value");
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);
            using var service = new VisitService(httpClient);

            await service.GetAuthTicketAsync("test-cookie", "test-csrf", 12345);
        }

        private class MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => handler(request, cancellationToken);
        }
    }
}
