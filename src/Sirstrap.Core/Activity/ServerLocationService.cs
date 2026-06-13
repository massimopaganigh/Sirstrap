namespace Sirstrap.Core.Activity
{
    public sealed class ServerLocationService(HttpClient httpClient, IPerformanceTelemetry performanceTelemetry) : IServerLocationService
    {
        private static readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(5);

        private readonly ConcurrentDictionary<string, string> _locationCache = new();

        public void ClearCache() => _locationCache.Clear();

        public async Task<string> GetServerLocationAsync(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return string.Empty;

            if (_locationCache.TryGetValue(ipAddress, out var cachedLocation))
                return cachedLocation;

            using ITelemetryScope scope = performanceTelemetry.Measure("server.location");

            try
            {
                using var timeout = new CancellationTokenSource(_requestTimeout);
                var response = await httpClient.GetAsync($"https://ipinfo.io/{ipAddress}/json", timeout.Token);

                if (!response.IsSuccessStatusCode)
                {
                    scope.MarkFailed();

                    performanceTelemetry.RecordCounter("server.location.outcome", new Dictionary<string, object> { ["value"] = "NotFound" });

                    return string.Empty;
                }

                var location = ParseLocation(await response.Content.ReadAsStringAsync(timeout.Token));

                _locationCache[ipAddress] = location;

                Log.Information("[*] Resolved the server location for IP {IpAddress}: {Location}.", ipAddress, location);

                performanceTelemetry.RecordCounter("server.location.outcome", new Dictionary<string, object> { ["value"] = "Success" });

                return location;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to resolve the server location for IP {IpAddress}.", ipAddress);

                scope.MarkFailed();

                performanceTelemetry.RecordCounter("server.location.outcome", new Dictionary<string, object> { ["value"] = "Exception" });

                return string.Empty;
            }
        }

        private static string GetString(JsonElement root, string propertyName)
            => root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
                ? property.GetString() ?? string.Empty
                : string.Empty;

        private static string ParseLocation(string json)
        {
            try
            {
                using var jsonDocument = JsonDocument.Parse(json);

                var root = jsonDocument.RootElement;
                var city = GetString(root, "city");
                var region = GetString(root, "region");
                var country = GetString(root, "country");

                if (string.IsNullOrWhiteSpace(region)
                    || string.IsNullOrWhiteSpace(country))
                    return string.Empty;

                if (string.IsNullOrWhiteSpace(city)
                    || city.Equals(region, StringComparison.InvariantCultureIgnoreCase))
                    return $"{region}, {country}";

                return $"{city}, {region}, {country}";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to parse the server location response.");

                return string.Empty;
            }
        }
    }
}
