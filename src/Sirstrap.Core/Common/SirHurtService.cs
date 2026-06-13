namespace Sirstrap.Core.Common
{
    public sealed class SirHurtService : ISirHurtService
    {
        private static readonly string _sirhui = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sirhurt", "sirhui");

        public string GetSirHurtPath()
        {
            try
            {
                Log.Information("[*] Retrieving the SirHurt path from dllpath.dat...");

                var dllPathDat = Path.Combine(_sirhui, "dllpath.dat");

                if (File.Exists(dllPathDat))
                {
                    var parent = Directory.GetParent(File.ReadAllText(dllPathDat));

                    if (parent != null
                        && Directory.Exists(parent.FullName))
                    {
                        Log.Information("[*] Retrieved the SirHurt path {SirHurtPath}.", parent.FullName);

                        return parent.FullName;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to retrieve the SirHurt path.");

                return string.Empty;
            }
        }

        public string GetSirHurtUser()
        {
            try
            {
                Log.Information("[*] Retrieving the SirHurt user from sirhurta.dat...");

                var sirhurtaDat = Path.Combine(_sirhui, "sirhurta.dat");

                if (File.Exists(sirhurtaDat))
                {
                    var user = File.ReadAllText(sirhurtaDat).Trim();

                    if (!string.IsNullOrWhiteSpace(user))
                    {
                        Log.Information("[*] Retrieved the SirHurt user {SirHurtUser}.", user);

                        return user;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to retrieve the SirHurt user.");

                return string.Empty;
            }
        }

        public bool Logout()
        {
            try
            {
                List<string> deletedFiles = [];
                List<string> notFoundFiles = [];

                foreach (var fileName in new[] { "sirhurta.dat", "sirhurtp.dat" })
                {
                    var filePath = Path.Combine(_sirhui, fileName);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);

                        deletedFiles.Add(fileName);

                        Log.Information("[*] Deleted the file {FilePath}.", filePath);
                    }
                    else
                    {
                        notFoundFiles.Add(fileName);

                        Log.Warning("[!] The file {FilePath} was not found.", filePath);
                    }
                }

                string result;

                if (deletedFiles.Count > 0)
                {
                    result = $"[*] Logged out from SirHurt. Deleted files: {string.Join(", ", deletedFiles)}.";

                    if (notFoundFiles.Count > 0)
                        result += $" Files not found: {string.Join(", ", notFoundFiles)}.";
                }
                else
                    result = $"[*] No files were deleted during the SirHurt logout. Files not found: {string.Join(", ", notFoundFiles)}.";

                Log.Information(result);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to log out from SirHurt.");

                return false;
            }
        }
    }
}
