namespace Sirstrap.Core.Tests.Common
{
    public class HttpClientExtensionTests
    {
        [Fact]
        public async Task GetStringAsync_ReturnsBody_OnSuccess()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, "hello");

            Assert.Equal("hello", await HttpClientExtension.GetStringAsync(client, "https://example.com"));
        }

        [Fact]
        public async Task GetByteArrayAsync_ReturnsBytes_OnSuccess()
        {
            HttpClient client = StubHttpMessageHandler.Client(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([1, 2, 3])
            });

            Assert.Equal([1, 2, 3], await HttpClientExtension.GetByteArrayAsync(client, "https://example.com"));
        }

        [Fact]
        public async Task GetStringAsync_ReturnsNull_AfterExhaustingAttempts()
        {
            HttpClient client = StubHttpMessageHandler.Client(_ => throw new HttpRequestException("down"));

            Assert.Null(await HttpClientExtension.GetStringAsync(client, "https://example.com", attempts: 1));
        }

        [Fact]
        public async Task GetStringAsync_RetriesThenSucceeds()
        {
            int calls = 0;
            HttpClient client = StubHttpMessageHandler.Client(_ =>
            {
                calls++;

                return calls == 1
                    ? throw new HttpRequestException("transient")
                    : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("recovered") };
            });

            Assert.Equal("recovered", await HttpClientExtension.GetStringAsync(client, "https://example.com", attempts: 2));
            Assert.Equal(2, calls);
        }

        [Fact]
        public async Task GetByteArrayAsync_ReturnsNull_AfterExhaustingAttempts()
        {
            HttpClient client = StubHttpMessageHandler.Client(_ => throw new HttpRequestException("down"));

            Assert.Null(await HttpClientExtension.GetByteArrayAsync(client, "https://example.com", attempts: 1));
        }
    }
}
