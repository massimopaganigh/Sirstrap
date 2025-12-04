namespace Sirstrap.Core
{
    public class SirHurtService
    {
        public static string GetSirHurtPath()
        {
            try
            {
                var dllPathDat = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sirhurt", "sirhui", "dllpath.dat");

                if (File.Exists(dllPathDat))
                {
                    var parent = Directory.GetParent(File.ReadAllText(dllPathDat));

                    if (parent != null
                        && Directory.Exists(parent.FullName))
                        return parent.FullName;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
