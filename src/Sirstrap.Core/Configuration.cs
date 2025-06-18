namespace Sirstrap.Core
{
    public class Configuration : ConfigurationBase
    {
        public string LaunchUri { get; set; } = string.Empty;

        public static void ClearCacheDirectory()
        {
            string cacheDirectory = GetCacheDirectory();

            try
            {

                foreach (string file in Directory.GetFiles(cacheDirectory))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception) { /* Sybau 🥀 */ }
                }
            }
            catch (Exception) { /* Sybau 🥀 */ }

            Directory.CreateDirectory(cacheDirectory);
        }

        public static string GetCacheDirectory()
        {
            string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Cache");

            Directory.CreateDirectory(cacheDirectory);

            return cacheDirectory;
        }

        public string GetOutputPath() => Path.Combine(GetCacheDirectory(), $"{VersionHash}.zip");

        public bool IsMacBinary() => BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase)
            || BinaryType.Equals("MacStudio", StringComparison.OrdinalIgnoreCase);
    }
}