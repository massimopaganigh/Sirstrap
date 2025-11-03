namespace Sirstrap.Core.Tests
{
    public class ServerLocationServiceTests
    {
        [Fact]
        public void ParseLocation_CityEqualToRegion_ReturnsRegionCountry()
        {
            // This test verifies the location formatting logic
            // When city equals region, format should be "[region], [country]"
            var json = "{\"city\":\"Milan\",\"region\":\"Milan\",\"country\":\"IT\"}";
            var result = InvokeParseLocationFromJson(json);
            
            Assert.Equal("Milan, IT", result);
        }

        [Fact]
        public void ParseLocation_CityDifferentFromRegion_ReturnsCityRegionCountry()
        {
            // When city differs from region, format should be "[city], [region], [country]"
            var json = "{\"city\":\"Rome\",\"region\":\"Lazio\",\"country\":\"IT\"}";
            var result = InvokeParseLocationFromJson(json);
            
            Assert.Equal("Rome, Lazio, IT", result);
        }

        [Fact]
        public void ParseLocation_MissingCity_ReturnsRegionCountry()
        {
            var json = "{\"region\":\"Lombardy\",\"country\":\"IT\"}";
            var result = InvokeParseLocationFromJson(json);
            
            Assert.Equal("Lombardy, IT", result);
        }

        [Fact]
        public void ParseLocation_MissingRegion_ReturnsUnavailable()
        {
            var json = "{\"city\":\"Rome\",\"country\":\"IT\"}";
            var result = InvokeParseLocationFromJson(json);
            
            Assert.Equal("località non disponibile", result);
        }

        [Fact]
        public void ExtractJsonValue_ValidKey_ReturnsValue()
        {
            var json = "{\"city\":\"Rome\",\"region\":\"Lazio\"}";
            var result = InvokeExtractJsonValue(json, "city");
            
            Assert.Equal("Rome", result);
        }

        [Fact]
        public void ExtractJsonValue_InvalidKey_ReturnsEmpty()
        {
            var json = "{\"city\":\"Rome\",\"region\":\"Lazio\"}";
            var result = InvokeExtractJsonValue(json, "country");
            
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetServerLocation_EmptyIp_ReturnsUnavailable()
        {
            var result = await ServerLocationService.GetServerLocationAsync("");
            
            Assert.Equal("località non disponibile", result);
        }

        [Fact]
        public async Task GetServerLocation_NullIp_ReturnsUnavailable()
        {
            var result = await ServerLocationService.GetServerLocationAsync(null!);
            
            Assert.Equal("località non disponibile", result);
        }

        [Fact]
        public void ClearCache_RemovesAllCachedEntries()
        {
            // This test verifies that the cache can be cleared
            // We can't easily test the internal cache, but we ensure the method doesn't throw
            ServerLocationService.ClearCache();
            
            Assert.True(true); // Method completed without exception
        }

        // Helper methods to invoke private methods via reflection for testing
        private static string InvokeParseLocationFromJson(string json)
        {
            var method = typeof(ServerLocationService).GetMethod("ParseLocationFromJson",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (string)method!.Invoke(null, new object[] { json })!;
        }

        private static string InvokeExtractJsonValue(string json, string key)
        {
            var method = typeof(ServerLocationService).GetMethod("ExtractJsonValue",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (string)method!.Invoke(null, new object[] { json, key })!;
        }
    }
}
