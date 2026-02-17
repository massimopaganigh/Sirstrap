using System.Net;

namespace Sirstrap.Core.Tests
{
    public class RobloxAuthClientTests
    {
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
            using var client = new RobloxAuthClient(httpClient);

            string token = await client.GetCsrfTokenAsync("test-cookie");

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
            using var client = new RobloxAuthClient(httpClient);

            await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetCsrfTokenAsync("test-cookie"));
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
            using var client = new RobloxAuthClient(httpClient);

            await client.GetCsrfTokenAsync("test-cookie");
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
            using var client = new RobloxAuthClient(httpClient);

            string ticket = await client.GetAuthTicketAsync("test-cookie", "test-csrf", 12345);

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
            using var client = new RobloxAuthClient(httpClient);

            string ticket = await client.GetAuthTicketAsync("test-cookie", "old-csrf", 12345);

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
            using var client = new RobloxAuthClient(httpClient);

            await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetAuthTicketAsync("test-cookie", "test-csrf", 12345));
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
            using var client = new RobloxAuthClient(httpClient);

            await client.GetAuthTicketAsync("test-cookie", "test-csrf", 12345);
        }

        [Fact]
        public async Task GetCsrfTokenAsync_ShouldPostToCorrectUrl()
        {
            Uri? capturedUri = null;

            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                capturedUri = request.RequestUri;

                var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                response.Headers.Add("x-csrf-token", "token");
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);
            using var client = new RobloxAuthClient(httpClient);

            await client.GetCsrfTokenAsync("cookie");

            Assert.Equal("https://friends.roblox.com/v1/users/1/request-friendship", capturedUri?.ToString());
        }

        [Fact]
        public async Task GetAuthTicketAsync_ShouldPostToCorrectUrl()
        {
            Uri? capturedUri = null;

            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                capturedUri = request.RequestUri;

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Headers.Add("rbx-authentication-ticket", "ticket");
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);
            using var client = new RobloxAuthClient(httpClient);

            await client.GetAuthTicketAsync("cookie", "csrf", 999);

            Assert.Equal("https://auth.roblox.com/v1/authentication-ticket", capturedUri?.ToString());
        }

        [Fact]
        public async Task GetCsrfTokenAsync_ShouldIncludeUserAgentHeader()
        {
            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                Assert.True(request.Headers.TryGetValues("User-Agent", out var values));
                Assert.Contains("Mozilla/5.0", values.First());

                var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                response.Headers.Add("x-csrf-token", "token");
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);
            using var client = new RobloxAuthClient(httpClient);

            await client.GetCsrfTokenAsync("cookie");
        }

        [Fact]
        public async Task GetCsrfTokenAsync_ShouldIncludeJsonContentType()
        {
            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                Assert.Equal("application/json", request.Content!.Headers.ContentType!.MediaType);

                var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                response.Headers.Add("x-csrf-token", "token");
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);
            using var client = new RobloxAuthClient(httpClient);

            await client.GetCsrfTokenAsync("cookie");
        }

        [Fact]
        public async Task GetAuthTicketAsync_ShouldIncludeReferrerWithGameUrl()
        {
            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                Assert.Equal("https://www.roblox.com/games/42", request.Headers.Referrer?.ToString());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Headers.Add("rbx-authentication-ticket", "ticket");
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);
            using var client = new RobloxAuthClient(httpClient);

            await client.GetAuthTicketAsync("cookie", "csrf", 42);
        }

        [Fact]
        public async Task GetAuthTicketAsync_ShouldSucceedOnThirdAttemptAfterTwo403s()
        {
            int callCount = 0;

            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                callCount++;

                if (callCount <= 2)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                    response.Headers.Add("x-csrf-token", $"csrf-{callCount}");
                    return Task.FromResult(response);
                }
                else
                {
                    Assert.Contains("csrf-2", request.Headers.GetValues("x-csrf-token"));

                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Headers.Add("rbx-authentication-ticket", "final-ticket");
                    return Task.FromResult(response);
                }
            });

            using var httpClient = new HttpClient(handler);
            using var client = new RobloxAuthClient(httpClient);

            string ticket = await client.GetAuthTicketAsync("cookie", "initial-csrf", 1);

            Assert.Equal("final-ticket", ticket);
            Assert.Equal(3, callCount);
        }

        [Fact]
        public async Task GetAuthTicketAsync_ShouldThrowAfterMaxRetriesWith403AndNoCsrfHeader()
        {
            int callCount = 0;

            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                callCount++;

                var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);
            using var client = new RobloxAuthClient(httpClient);

            await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetAuthTicketAsync("cookie", "csrf", 1));

            Assert.Equal(3, callCount);
        }

        [Fact]
        public void Dispose_ShouldNotDisposeInjectedHttpClient()
        {
            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Headers.Add("x-csrf-token", "token");
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(handler);

            var client = new RobloxAuthClient(httpClient);
            client.Dispose();

            var newRequest = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            Assert.NotNull(httpClient.SendAsync(newRequest));
        }

        [Fact]
        public async Task GetAuthTicketAsync_ShouldPreserveCookieAcrossRetries()
        {
            var capturedCookies = new List<string>();

            var handler = new MockHttpMessageHandler((request, ct) =>
            {
                capturedCookies.Add(request.Headers.GetValues("Cookie").First());

                if (capturedCookies.Count == 1)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                    response.Headers.Add("x-csrf-token", "new-csrf");
                    return Task.FromResult(response);
                }
                else
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Headers.Add("rbx-authentication-ticket", "ticket");
                    return Task.FromResult(response);
                }
            });

            using var httpClient = new HttpClient(handler);
            using var client = new RobloxAuthClient(httpClient);

            await client.GetAuthTicketAsync("my-secret-cookie", "csrf", 1);

            Assert.All(capturedCookies, cookie => Assert.Equal(".ROBLOSECURITY=my-secret-cookie", cookie));
        }

        private class MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => handler(request, cancellationToken);
        }
    }
}
