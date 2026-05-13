namespace Sirstrap.Core
{
    public static class SirHurtService
    {
        private static readonly string _sirhui = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sirhurt", "sirhui");

        public static string GetSirHurtPath()
        {
            try
            {
                Log.Information("[*] Attempting to retrieve SirHurt path from dllpath.dat...");

                var dllPathDat = Path.Combine(_sirhui, "dllpath.dat");

                if (File.Exists(dllPathDat))
                {
                    var parent = Directory.GetParent(File.ReadAllText(dllPathDat));

                    if (parent != null
                        && Directory.Exists(parent.FullName))
                    {
                        Log.Information("[*] Successfully retrieved SirHurt path: {0}", parent.FullName);

                        return parent.FullName;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[*] Failed to retrieve SirHurt path: {0}", ex.Message);

                return string.Empty;
            }
        }

        public static string GetSirHurtUser()
        {
            try
            {
                Log.Information("[*] Attempting to retrieve SirHurt user from sirhurta.dat...");

                var sirhurtaDat = Path.Combine(_sirhui, "sirhurta.dat");

                if (File.Exists(sirhurtaDat))
                {
                    var user = File.ReadAllText(sirhurtaDat).Trim();

                    if (!string.IsNullOrWhiteSpace(user))
                    {
                        Log.Information("[*] Successfully retrieved SirHurt user: {0}", user);

                        return user;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[*] Failed to retrieve SirHurt user: {0}", ex.Message);

                return string.Empty;
            }
        }

        public static bool Login() => throw new NotImplementedException(nameof(Login));

        public static bool Logout()
        {
            try
            {
                string sirHurtADat = Path.Combine(_sirhui, "sirhurta.dat");
                string sirHurtPDat = Path.Combine(_sirhui, "sirhurtp.dat");
                List<string> deletedFiles = [];
                List<string> notFoundFiles = [];

                if (File.Exists(sirHurtADat))
                {
                    File.Delete(sirHurtADat);

                    deletedFiles.Add("sirhurta.dat");

                    Log.Information("[{0}] Deleted file: {1}.", nameof(Logout), sirHurtADat);
                }
                else
                {
                    notFoundFiles.Add("sirhurta.dat");

                    Log.Warning("[{0}] File not found: {1}.", nameof(Logout), sirHurtADat);
                }

                if (File.Exists(sirHurtPDat))
                {
                    File.Delete(sirHurtPDat);

                    deletedFiles.Add("sirhurtp.dat");

                    Log.Information("[{0}] Deleted file: {1}.", nameof(Logout), sirHurtPDat);
                }
                else
                {
                    notFoundFiles.Add("sirhurtp.dat");

                    Log.Warning("[{0}] File not found: {1}.", nameof(Logout), sirHurtPDat);
                }

                string result;

                if (deletedFiles.Count > 0)
                {
                    result = $"[{nameof(Logout)}] Logout completed successfully. Deleted files: {string.Join(", ", deletedFiles)}.";

                    if (notFoundFiles.Count > 0)
                        result += $" Files not found: {string.Join(", ", notFoundFiles)}.";
                }
                else
                    result = $"[{nameof(Logout)}] No files were deleted. Files not found: {string.Join(", ", notFoundFiles)}.";

                Log.Information(result);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(Logout));

                return false;
            }
        }
    }
}
