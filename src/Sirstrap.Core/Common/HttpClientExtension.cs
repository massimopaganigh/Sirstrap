namespace Sirstrap.Core.Common
{
    public static class HttpClientExtension
    {
        #region PRIVATE METHODS
        private static async Task<T?> GetWithRetryAsync<T>(Func<Task<T>> request, string uri, string requestDescription, int attempts) where T : class
        {
            for (var attempt = 1; attempt <= attempts; attempt++)
                try
                {
                    return await request();
                }
                catch (Exception ex)
                {
                    if (attempt < attempts)
                    {
                        Log.Warning("[!] The {RequestDescription} request to {Uri} failed, retrying...", requestDescription, uri);
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                    }
                    else
                    {
                        Log.Error(ex, "[!] The {RequestDescription} request to {Uri} failed.", requestDescription, uri);
                    }
                }

            return null;
        }
        #endregion

        public static Task<byte[]?> GetByteArrayAsync(HttpClient httpClient, string uri, int attempts = 3) => GetWithRetryAsync(() => httpClient.GetByteArrayAsync(uri), uri, "Byte array", attempts);

        public static Task<string?> GetStringAsync(HttpClient httpClient, string uri, int attempts = 3) => GetWithRetryAsync(() => httpClient.GetStringAsync(uri), uri, "String", attempts);
    }
}
