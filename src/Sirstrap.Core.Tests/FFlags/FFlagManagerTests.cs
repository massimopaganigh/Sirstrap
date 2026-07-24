namespace Sirstrap.Core.Tests.FFlags
{
    public class FFlagManagerTests
    {
        [Fact]
        public void FFlagManager_LoadSaveDeploy_WorksCorrectly()
        {
            using TempDirectory temp = new();
            FFlagManager manager = new(NullPerformanceTelemetry.Instance);

            var flags = new Dictionary<string, object>
            {
                ["DFIntTaskSchedulerTargetFps"] = 240,
                ["FFlagDebugGraphicsPreferD3D11"] = true
            };

            // Deploy to temp directory
            manager.DeployFFlags(temp.Path);

            // Save test flags to standard path
            manager.SaveFFlags(flags);
            var loaded = manager.LoadFFlags();

            Assert.True(loaded.ContainsKey("DFIntTaskSchedulerTargetFps"));
            Assert.True(loaded.ContainsKey("FFlagDebugGraphicsPreferD3D11"));

            // Deploy again to verify file output
            manager.DeployFFlags(temp.Path);
            var deployedJsonPath = Path.Combine(temp.Path, "ClientSettings", "ClientAppSettings.json");
            Assert.True(File.Exists(deployedJsonPath));

            var jsonContent = File.ReadAllText(deployedJsonPath);
            Assert.Contains("DFIntTaskSchedulerTargetFps", jsonContent);
            Assert.Contains("240", jsonContent);
        }
    }
}
