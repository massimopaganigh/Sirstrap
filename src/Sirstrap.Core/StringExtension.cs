namespace Sirstrap.Core
{
    public static class StringExtension
    {
        public static void BetterDirectoryCreate(this string directoryPath, int attempts = 5)
        {
            try
            {
                foreach (int attempt in Enumerable.Range(1, attempts))
                    try
                    {
                        if (!Directory.Exists(directoryPath))
                            Directory.CreateDirectory(directoryPath);

                        return;
                    }
                    catch (IOException)
                    {
                        if (attempt == attempts)
                            throw;

                        Thread.Sleep(100 * attempt);
                    }
            }
            catch (Exception)
            {
                Log.Error("[!] Error creating directory: {0}.", directoryPath);

                throw;
            }
        }

        public static void BetterDirectoryDelete(this string directoryPath, int attempts = 5)
        {
            try
            {
                foreach (int attempt in Enumerable.Range(1, attempts))
                    try
                    {
                        if (Directory.Exists(directoryPath))
                            Directory.Delete(directoryPath, true);

                        return;
                    }
                    catch (IOException)
                    {
                        if (attempt == attempts)
                            throw;

                        Thread.Sleep(100 * attempt);
                    }
            }
            catch (Exception)
            {
                Log.Error("[!] Error deleting directory: {0}.", directoryPath);

                throw;
            }
        }

        public static void BetterFileDelete(this string filePath, int attempts = 5)
        {
            try
            {
                foreach (int attempt in Enumerable.Range(1, attempts))
                    try
                    {
                        if (File.Exists(filePath))
                            File.Delete(filePath);

                        return;
                    }
                    catch (IOException)
                    {
                        if (attempt == attempts)
                            throw;

                        Thread.Sleep(100 * attempt);
                    }
            }
            catch (Exception)
            {
                Log.Error("[!] Error deleting file: {0}.", filePath);

                throw;
            }
        }
    }
}
