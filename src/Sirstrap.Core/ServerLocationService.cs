namespace Sirstrap.Core
{
    public static class ServerLocationService
    {
        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        private static readonly ConcurrentDictionary<string, string> _locationCache = new();

        private static string ExtractJsonValue(string json, string key)
        {
            try
            {
                var startIndex = json.IndexOf($"\"{key}\"");

                if (startIndex == -1)
                    return string.Empty;

                startIndex = json.IndexOf(':', startIndex);

                if (startIndex == -1)
                    return string.Empty;

                startIndex = json.IndexOf('\"', startIndex);

                if (startIndex == -1)
                    return string.Empty;

                startIndex++;

                var endIndex = json.IndexOf('\"', startIndex);

                if (endIndex == -1)
                    return string.Empty;

                return json[startIndex..endIndex];
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ParseLocationFromJson(string json)
        {
            try
            {
                var city = ExtractJsonValue(json, "city");
                var region = ExtractJsonValue(json, "region");
                var country = ExtractJsonValue(json, "country");

                if (string.IsNullOrWhiteSpace(region)
                    || string.IsNullOrWhiteSpace(country))
                    return string.Empty;

                if (string.IsNullOrWhiteSpace(city)
                    || city.Equals(region, StringComparison.InvariantCultureIgnoreCase))
                    return $"{region}, {country}";
                else
                    return $"{city}, {region}, {country}";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Exception while parsing location JSON: {0}", ex.Message);

                return string.Empty;
            }
        }

        public static void ClearCache() => _locationCache.Clear();

        public static async Task<string> GetServerLocationAsync(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return string.Empty;

            if (_locationCache.TryGetValue(ipAddress, out var cachedLocation))
                return cachedLocation;

            try
            {
                var response = await _httpClient.GetAsync($"https://ipinfo.io/{ipAddress}/json");

                if (!response.IsSuccessStatusCode)
                    return string.Empty;

                var location = ParseLocationFromJson(await response.Content.ReadAsStringAsync());

                _locationCache[ipAddress] = location;

                Log.Information("[*] Server location for IP {0}: {1}", ipAddress, location);

                return location;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Exception while getting server location for IP {0}: {1}", ipAddress, ex.Message);

                return string.Empty;
            }
        }
    }
}
