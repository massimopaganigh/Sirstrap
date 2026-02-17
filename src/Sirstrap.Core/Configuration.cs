namespace Sirstrap.Core
{
    public class Configuration : ConfigurationBase
    {
        public static void ClearCacheDirectory()
        {
            try
            {
                string cacheDir = GetCacheDir();

                foreach (string file in Directory.GetFiles(cacheDir))
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception)
                    {
                        //Sybau 🥀
                    }

                Directory.CreateDirectory(cacheDir);
            }
            catch (Exception)
            {
                //Sybau 🥀
            }
        }

        private static string GetCacheDir()
        {
            string cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Cache");

            Directory.CreateDirectory(cacheDir);

            return cacheDir;
        }

        public string GetOutputPath() => Path.Combine(GetCacheDir(), $"{VersionHash}.zip");

        public bool IsMacBinary() => BinaryType.Equals("MacPlayer", StringComparison.OrdinalIgnoreCase) || BinaryType.Equals("MacStudio", StringComparison.OrdinalIgnoreCase);

        public bool IsVisitMode() => Mode.Equals("visit", StringComparison.OrdinalIgnoreCase);

        public string LaunchUri { get; set; } = string.Empty;

        public string Mode { get; set; } = string.Empty;

        public string CookiesFile { get; set; } = string.Empty;

        public long PlaceId { get; set; }

        public int Timeout { get; set; } = 30;
    }
}
