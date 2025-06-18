namespace Sirstrap.Core
{
    public static class HttpClientExtension
    {
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