using Serilog;

namespace Sirstrap.Core
{
    /// <summary>
    /// Provides HTTP utility methods for downloading strings and byte arrays from URLs,
    /// with integrated error handling and logging.
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// Asynchronously downloads binary content from the specified URL.
        /// </summary>
        /// <param name="httpClient">The HTTP client used to make the request.</param>
        /// <param name="url">The URL to download the binary content from.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the downloaded byte array, or null if the download fails.
        /// </returns>
        /// <remarks>
        /// Exceptions that occur during the download are caught, logged, and null is returned.
        /// This method does not throw exceptions to the caller.
        /// </remarks>
        public static async Task<byte[]> GetBytesAsync(HttpClient httpClient, string url)
        {
            try
            {
                return await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error requesting binary from {Url}: {ErrorMessage}", url, ex.Message);

                return null!;
            }
        }

        /// <summary>
        /// Asynchronously downloads text content from the specified URL.
        /// </summary>
        /// <param name="httpClient">The HTTP client used to make the request.</param>
        /// <param name="url">The URL to download the text content from.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the downloaded string, or an empty string if the download fails.
        /// </returns>
        /// <remarks>
        /// Exceptions that occur during the download are caught, logged, and an empty string is returned.
        /// This method does not throw exceptions to the caller.
        /// </remarks>
        public static async Task<string> GetStringAsync(HttpClient httpClient, string url)
        {
            try
            {
                return await httpClient.GetStringAsync(url).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error requesting {Url}: {ErrorMessage}", url, ex.Message);

                return string.Empty;
            }
        }
    }
}