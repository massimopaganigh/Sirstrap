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
                        Log.Warning("[*] Byte array request from {0} failed, trying again...", uri);

                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                    }
                    else
                    {
                        Log.Error(ex, "[!] Byte array request from {0} failed: {1}", uri, ex.Message);

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
                        Log.Warning("[*] String request from {0} failed, trying again...", uri);

                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                    }
                    else
                    {
                        Log.Error(ex, "[!] String request from {0} failed: {1}", uri, ex.Message);

                        return null;
                    }
                }

            return null;
        }
    }
}
