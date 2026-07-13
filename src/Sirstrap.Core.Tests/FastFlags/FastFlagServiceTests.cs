namespace Sirstrap.Core.Tests.FastFlags
{
    public class FastFlagServiceTests
    {
        private static FastFlagService NewService(bool enabled = true) => new(new SirstrapConfiguration { RobloxFastFlagsEnabled = enabled });

        [Fact]
        public void GetFlags_ReturnsEmpty_WhenFileMissing()
        {
            using TempDirectory temp = new();

            Assert.Empty(NewService().GetFlags(temp.Combine("FastFlags.json")));
        }

        [Fact]
        public void SetFlags_RoundTrips_DisplayValues()
        {
            using TempDirectory temp = new();
            FastFlagService service = NewService();
            string flagsPath = temp.Combine("FastFlags.json");

            service.SetFlags(new Dictionary<string, string>
            {
                ["FFlagB"] = "True",
                ["DFIntA"] = "123",
                ["DFFloatC"] = "1.5",
                ["FStringD"] = "hello"
            }, flagsPath);

            IReadOnlyDictionary<string, string> flags = service.GetFlags(flagsPath);

            Assert.Equal("True", flags["FFlagB"]);
            Assert.Equal("123", flags["DFIntA"]);
            Assert.Equal("1.5", flags["DFFloatC"]);
            Assert.Equal("hello", flags["FStringD"]);
        }

        [Fact]
        public void SetFlags_WritesTypedJson_SortedByName()
        {
            using TempDirectory temp = new();
            FastFlagService service = NewService();
            string flagsPath = temp.Combine("FastFlags.json");

            service.SetFlags(new Dictionary<string, string>
            {
                ["ZFlag"] = "true",
                ["AInt"] = "42"
            }, flagsPath);

            string json = File.ReadAllText(flagsPath);

            Assert.True(json.IndexOf("AInt", StringComparison.Ordinal) < json.IndexOf("ZFlag", StringComparison.Ordinal));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.Equal(JsonValueKind.True, document.RootElement.GetProperty("ZFlag").ValueKind);
            Assert.Equal(JsonValueKind.Number, document.RootElement.GetProperty("AInt").ValueKind);
        }

        [Fact]
        public void SetFlags_SkipsEntriesWithEmptyNames_AndTrims()
        {
            using TempDirectory temp = new();
            FastFlagService service = NewService();
            string flagsPath = temp.Combine("FastFlags.json");

            service.SetFlags(new Dictionary<string, string>
            {
                [" FFlagA "] = " True ",
                ["   "] = "ignored"
            }, flagsPath);

            IReadOnlyDictionary<string, string> flags = service.GetFlags(flagsPath);

            Assert.Equal("True", Assert.Single(flags).Value);
            Assert.True(flags.ContainsKey("FFlagA"));
        }

        [Fact]
        public void GetFlags_IgnoresNonScalarValues_AndCorruptedFiles()
        {
            using TempDirectory temp = new();
            FastFlagService service = NewService();

            string mixedPath = temp.WriteFile("Mixed.json", """{"FFlagA":"True","FFlagB":{"nested":1},"FFlagC":[1],"FFlagD":null}""");
            Assert.Equal(["FFlagA"], [.. service.GetFlags(mixedPath).Keys]);

            string corruptedPath = temp.WriteFile("Corrupted.json", "not json");
            Assert.Empty(service.GetFlags(corruptedPath));

            string arrayPath = temp.WriteFile("Array.json", "[]");
            Assert.Empty(service.GetFlags(arrayPath));
        }

        [Fact]
        public void Apply_WritesClientAppSettings()
        {
            using TempDirectory temp = new();
            FastFlagService service = NewService();
            string flagsPath = temp.WriteFile("FastFlags.json", """{"DFIntTaskSchedulerTargetFps":144}""");
            string versionDirectory = temp.Combine("version-abc123");

            service.Apply(versionDirectory, flagsPath);

            string clientAppSettingsPath = Path.Combine(versionDirectory, "ClientSettings", "ClientAppSettings.json");

            Assert.True(File.Exists(clientAppSettingsPath));

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(clientAppSettingsPath));

            Assert.Equal(144, document.RootElement.GetProperty("DFIntTaskSchedulerTargetFps").GetInt64());
        }

        [Fact]
        public void Apply_RemovesClientAppSettings_WhenDisabled()
        {
            using TempDirectory temp = new();
            FastFlagService service = NewService(enabled: false);
            string flagsPath = temp.WriteFile("FastFlags.json", """{"FFlagA":"True"}""");
            string versionDirectory = temp.Combine("version-abc123");
            string clientAppSettingsPath = temp.WriteFile(Path.Combine("version-abc123", "ClientSettings", "ClientAppSettings.json"), "{}");

            service.Apply(versionDirectory, flagsPath);

            Assert.False(File.Exists(clientAppSettingsPath));
        }

        [Fact]
        public void Apply_RemovesClientAppSettings_WhenNoFlags()
        {
            using TempDirectory temp = new();
            FastFlagService service = NewService();
            string versionDirectory = temp.Combine("version-abc123");
            string clientAppSettingsPath = temp.WriteFile(Path.Combine("version-abc123", "ClientSettings", "ClientAppSettings.json"), "{}");

            service.Apply(versionDirectory, temp.Combine("FastFlags.json"));

            Assert.False(File.Exists(clientAppSettingsPath));
        }

        [Fact]
        public void Apply_RemovesClientAppSettings_WhenFlagsFileCorrupted()
        {
            using TempDirectory temp = new();
            FastFlagService service = NewService();
            string flagsPath = temp.WriteFile("FastFlags.json", "not json");
            string versionDirectory = temp.Combine("version-abc123");
            string clientAppSettingsPath = temp.WriteFile(Path.Combine("version-abc123", "ClientSettings", "ClientAppSettings.json"), "{}");

            service.Apply(versionDirectory, flagsPath);

            Assert.False(File.Exists(clientAppSettingsPath));
        }
    }
}
