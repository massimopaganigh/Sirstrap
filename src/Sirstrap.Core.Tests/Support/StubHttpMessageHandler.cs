namespace Sirstrap.Core.Tests.Support
{
    public sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;

        public StubHttpMessageHandler(HttpStatusCode statusCode, string content)
            : this(_ => new HttpResponseMessage(statusCode) { Content = new StringContent(content) })
        {
        }

        public List<string> RequestedUris { get; } = [];

        public int CallCount { get; private set; }

        public static HttpClient Client(Func<HttpRequestMessage, HttpResponseMessage> responder) => new(new StubHttpMessageHandler(responder));

        public static HttpClient Client(HttpStatusCode statusCode, string content) => new(new StubHttpMessageHandler(statusCode, content));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            RequestedUris.Add(request.RequestUri?.ToString() ?? string.Empty);

            return Task.FromResult(_responder(request));
        }
    }
}
