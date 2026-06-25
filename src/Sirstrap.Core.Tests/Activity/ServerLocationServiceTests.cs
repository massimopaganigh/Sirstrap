namespace Sirstrap.Core.Tests.Activity
{
    public class ServerLocationServiceTests
    {
        private static ServerLocationService NewService(HttpClient client) => new(client, NullPerformanceTelemetry.Instance);

        [Fact]
        public async Task GetServerLocationAsync_ReturnsEmpty_ForBlankIp()
        {
            ServerLocationService service = NewService(StubHttpMessageHandler.Client(HttpStatusCode.OK, "{}"));

            Assert.Equal(string.Empty, await service.GetServerLocationAsync("  "));
        }

        [Fact]
        public async Task GetServerLocationAsync_ReturnsCityRegionCountry()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, """{"city":"Milan","region":"Lombardy","country":"IT"}""");

            Assert.Equal("Milan, Lombardy, IT", await NewService(client).GetServerLocationAsync("203.0.113.1"));
        }

        [Fact]
        public async Task GetServerLocationAsync_OmitsCity_WhenEqualToRegion()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, """{"city":"Berlin","region":"Berlin","country":"DE"}""");

            Assert.Equal("Berlin, DE", await NewService(client).GetServerLocationAsync("203.0.113.2"));
        }

        [Fact]
        public async Task GetServerLocationAsync_ReturnsEmpty_WhenRegionOrCountryMissing()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, """{"city":"Nowhere"}""");

            Assert.Equal(string.Empty, await NewService(client).GetServerLocationAsync("203.0.113.3"));
        }

        [Fact]
        public async Task GetServerLocationAsync_CachesResult()
        {
            StubHttpMessageHandler handler = new(HttpStatusCode.OK, """{"city":"Rome","region":"Lazio","country":"IT"}""");
            ServerLocationService service = new(new HttpClient(handler), NullPerformanceTelemetry.Instance);

            string first = await service.GetServerLocationAsync("203.0.113.4");
            string second = await service.GetServerLocationAsync("203.0.113.4");

            Assert.Equal(first, second);
            Assert.Equal(1, handler.CallCount);

            service.ClearCache();
            await service.GetServerLocationAsync("203.0.113.4");
            Assert.Equal(2, handler.CallCount);
        }

        [Fact]
        public async Task GetServerLocationAsync_ReturnsEmpty_OnHttpError()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.InternalServerError, "error");

            Assert.Equal(string.Empty, await NewService(client).GetServerLocationAsync("203.0.113.5"));
        }

        [Fact]
        public async Task GetServerLocationAsync_ReturnsEmpty_OnException()
        {
            HttpClient client = StubHttpMessageHandler.Client(_ => throw new HttpRequestException("down"));

            Assert.Equal(string.Empty, await NewService(client).GetServerLocationAsync("203.0.113.6"));
        }

        [Fact]
        public async Task GetServerLocationAsync_ReturnsEmpty_OnMalformedJson()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, "not-json");

            Assert.Equal(string.Empty, await NewService(client).GetServerLocationAsync("203.0.113.7"));
        }
    }
}
