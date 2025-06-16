using Serilog;

namespace Sirstrap.Core
{
    public static class BetterHttpClient
    {
        /// <summary>
        /// Attempts to retrieve a byte array from the specified URI using the provided <see cref="HttpClient"/>.
        /// </summary>
        /// <remarks>If the request fails, the method retries up to the specified number of attempts, with an exponential backoff delay between retries.
        /// The delay increases as 2^n seconds, where n is the current attempt number.</remarks>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance used to send the request. Cannot be <c>null</c>.</param>
        /// <param name="uri">The URI of the resource to retrieve. Cannot be <c>null</c> or empty.</param>
        /// <param name="attempts">The maximum number of retry attempts in case of failure. Must be greater than or equal to 1. Defaults to 3.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the byte array of the resource if the request succeeds, or <c>null</c> if all attempts fail.</returns>
        public static async Task<byte[]?> GetByteArrayAsync(HttpClient httpClient, string uri, int attempts = 3)
        {
            for (int attempt = 1; attempt <= attempts; attempt++)
                try
                {
                    return await httpClient.GetByteArrayAsync(uri);
                }
                catch (Exception ex)
                {
                    if (attempt < attempts)
                    {
                        Log.Warning("[*] Failed to request byte array from {0}, retrying...", uri);

                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                    }
                    else
                    {
                        Log.Error(ex, "[!] Failed to request byte array from {0}: {1}.", uri, ex.Message);

                        return null;
                    }
                }

            return null;
        }

        /// <summary>
        /// Attempts to retrieve the string content from the specified URI using the provided <see cref="HttpClient"/>.
        /// </summary>
        /// <remarks>This method implements an exponential backoff strategy for retries.
        /// If a request fails, it waits for an increasing amount of time (2^n seconds, where n is the attempt number) before retrying, up to the specified number of attempts.</remarks>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance used to send the request. Cannot be <see langword="null"/>.</param>
        /// <param name="uri">The URI of the resource to retrieve. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="attempts">The maximum number of retry attempts in case of a failure. Must be greater than or equal to 1. Defaults to 3.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result is the string content of the response if the request succeeds, or <see langword="null"/> if all retry attempts fail.</returns>
        public static async Task<string?> GetStringAsync(HttpClient httpClient, string uri, int attempts = 3)
        {
            for (int attempt = 1; attempt <= attempts; attempt++)
                try
                {
                    return await httpClient.GetStringAsync(uri);
                }
                catch (Exception ex)
                {
                    if (attempt < attempts)
                    {
                        Log.Warning("[*] Failed to request string from {0}, retrying...", uri);

                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                    }
                    else
                    {
                        Log.Error(ex, "[!] Failed to request string from {0}: {1}.", uri, ex.Message);

                        return null;
                    }
                }

            return null;
        }
    }
}