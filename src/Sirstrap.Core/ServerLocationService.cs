namespace Sirstrap.Core
{
    public class ServerLocationService
    {
        private const string IPINFO_BASE_URL = "https://ipinfo.io";
        private const string LOCATION_UNAVAILABLE = "località non disponibile";

        private static readonly Dictionary<string, string> _locationCache = new();
        private static readonly HttpClient _httpClient = new();

        public static async Task<string> GetServerLocationAsync(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return LOCATION_UNAVAILABLE;

            // Check cache first
            if (_locationCache.TryGetValue(ipAddress, out var cachedLocation))
            {
                Log.Debug("[*] Returning cached location for IP {0}: {1}", ipAddress, cachedLocation);
                return cachedLocation;
            }

            try
            {
                var url = $"{IPINFO_BASE_URL}/{ipAddress}/json";
                Log.Debug("[*] Querying ipinfo.io for IP {0}...", ipAddress);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("[!] Failed to get location for IP {0}: HTTP {1}", ipAddress, response.StatusCode);
                    return LOCATION_UNAVAILABLE;
                }

                var json = await response.Content.ReadAsStringAsync();
                var location = ParseLocationFromJson(json);

                // Cache the result
                _locationCache[ipAddress] = location;

                Log.Information("[*] Server location for IP {0}: {1}", ipAddress, location);

                return location;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Exception while getting server location for IP {0}: {1}", ipAddress, ex.Message);
                return LOCATION_UNAVAILABLE;
            }
        }

        private static string ParseLocationFromJson(string json)
        {
            try
            {
                // Parse JSON manually to avoid dependencies
                var city = ExtractJsonValue(json, "city");
                var region = ExtractJsonValue(json, "region");
                var country = ExtractJsonValue(json, "country");

                if (string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(country))
                    return LOCATION_UNAVAILABLE;

                // If city equals region, format as "[region], [country]"
                // Otherwise format as "[city], [region], [country]"
                if (string.IsNullOrWhiteSpace(city) || city.Equals(region, StringComparison.OrdinalIgnoreCase))
                    return $"{region}, {country}";
                else
                    return $"{city}, {region}, {country}";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Exception while parsing location JSON: {0}", ex.Message);
                return LOCATION_UNAVAILABLE;
            }
        }

        private static string ExtractJsonValue(string json, string key)
        {
            try
            {
                var searchKey = $"\"{key}\"";
                var startIndex = json.IndexOf(searchKey);

                if (startIndex == -1)
                    return string.Empty;

                startIndex = json.IndexOf(":", startIndex);
                if (startIndex == -1)
                    return string.Empty;

                startIndex = json.IndexOf("\"", startIndex);
                if (startIndex == -1)
                    return string.Empty;

                startIndex++; // Move past the opening quote

                var endIndex = json.IndexOf("\"", startIndex);
                if (endIndex == -1)
                    return string.Empty;

                return json.Substring(startIndex, endIndex - startIndex);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void ClearCache()
        {
            _locationCache.Clear();
            Log.Debug("[*] Server location cache cleared.");
        }
    }
}
