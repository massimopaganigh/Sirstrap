namespace Sirstrap.Core.Extensions
{
    public static class HttpClientExtension
    {
        public static async Task<byte[]?> BetterGetByteArrayAsync(this HttpClient httpClient, string uri, int attempts = 3)
        {
            try
            {
                foreach (int attempt in Enumerable.Range(1, attempts))
                    try
                    {
                        return await httpClient.GetByteArrayAsync(uri);
                    }
                    catch (HttpRequestException)
                    {
                        if (attempt == attempts)
                            throw;

                        Thread.Sleep(100 * attempt);
                    }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting byte array: {0}.", ex.Message);

                throw;
            }

            return null;
        }

        public static async Task<string?> BetterGetStringAsync(this HttpClient httpClient, string uri, int attempts = 3)
        {
            try
            {
                foreach (int attempt in Enumerable.Range(1, attempts))
                    try
                    {
                        return await httpClient.GetStringAsync(uri);
                    }
                    catch (HttpRequestException)
                    {
                        if (attempt == attempts)
                            throw;

                        Thread.Sleep(100 * attempt);
                    }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error getting string: {0}.", ex.Message);

                throw;
            }

            return null;
        }
    }
}
