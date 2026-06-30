using System.Net;

namespace Sirstrap.Core.Weao
{
    public class WeaoException(string message, Exception? innerException = null) : Exception(message, innerException)
    {
    }

    public sealed class WeaoRequestException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null) : WeaoException(message, innerException)
    {
        public HttpStatusCode? StatusCode { get; } = statusCode;
    }

    public sealed class WeaoRateLimitException(RateLimitInfo? info) : WeaoException(BuildMessage(info))
    {
        private static string BuildMessage(RateLimitInfo? info) => info is null ? "WEAO rate limit exceeded (HTTP 429)." : $"WEAO rate limit exceeded; retry in {info.RemainingTime}s ({info.RequestsRemaining} requests remaining).";

        public RateLimitInfo? RateLimitInfo { get; } = info;

        public TimeSpan? RetryAfter => RateLimitInfo is { RemainingTime: > 0 } rateLimitInfo ? TimeSpan.FromSeconds(rateLimitInfo.RemainingTime) : null;
    }
}
