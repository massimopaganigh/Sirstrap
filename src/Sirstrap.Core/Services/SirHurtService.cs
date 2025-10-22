namespace Sirstrap.Core.Services
{
    public class SirHurtService
    {
        private static string GetSirHurtAppDataPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sirhurt", "sirhui");

        public static (bool Result, string SirHurtPath) GetSirHurtPath()
        {
            try
            {
                var result = Path.Combine(GetSirHurtAppDataPath(), "dllpath.dat");

                if (File.Exists(result))
                {
                    var parent = Directory.GetParent(File.ReadAllText(result));

                    if (parent != null
                        && Directory.Exists(parent.FullName))
                        return (true, parent.FullName);
                }

                Log.Warning("The SirHurt path file does not exist at: {0}.", result);

                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while trying to get the SirHurt path: {0}.", ex.Message);

                return (false, string.Empty);
            }
        }
    }
}
