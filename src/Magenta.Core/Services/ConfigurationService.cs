namespace Magenta.Core.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public bool GetConfiguration()
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                Log.Information("[{0}.{1}] Getting configuration...", nameof(ConfigurationService), nameof(GetConfiguration));

                if (!File.Exists(Paths.ConfigurationPath))
                    SetConfiguration();

                INIFile configuration = new(Paths.ConfigurationPath);

                if (int.TryParse(configuration.GetValue("CONFIGURATION", "DELAY", "5000"), out int value))
                    Configuration.Delay = value;

                if (int.TryParse(configuration.GetValue("CONFIGURATION", "PLACEID", "-1"), out int placeId))
                    Configuration.PlaceId = placeId;

                string roblosecurityCookies = configuration.GetValue("CONFIGURATION", "ROBLOSECURITYCOOKIES", string.Empty);

                if (!string.IsNullOrEmpty(roblosecurityCookies))
                    Configuration.RoblosecurityCookies = [.. roblosecurityCookies.Split(';')];

                if (int.TryParse(configuration.GetValue("CONFIGURATION", "THREADS", "1"), out int threads))
                    Configuration.Threads = threads;

                Log.Information("[{0}.{1}] Done in {2} ms.", nameof(ConfigurationService), nameof(GetConfiguration), stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[{0}.{1}] Exception message: {2}", nameof(ConfigurationService), nameof(GetConfiguration), ex.Message);

                return false;
            }
        }

        public bool SetConfiguration()
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                Log.Information("[{0}.{1}] Setting configuration...", nameof(ConfigurationService), nameof(SetConfiguration));

                INIFile configuration = new(Paths.ConfigurationPath);

                configuration.SetValue("CONFIGURATION", "DELAY", Configuration.Delay.ToString());
                configuration.SetValue("CONFIGURATION", "PLACEID", Configuration.PlaceId.ToString());
                configuration.SetValue("CONFIGURATION", "ROBLOSECURITYCOOKIES", string.Join(';', Configuration.RoblosecurityCookies));
                configuration.SetValue("CONFIGURATION", "THREADS", Configuration.Threads.ToString());

                Log.Information("[{0}.{1}] Done in {2} ms.", nameof(ConfigurationService), nameof(SetConfiguration), stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[{0}.{1}] Exception message: {2}", nameof(ConfigurationService), nameof(SetConfiguration), ex.Message);

                return false;
            }
        }
    }
}
