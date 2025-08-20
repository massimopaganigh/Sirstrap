namespace Sirstrap.Core.Extensions
{
    public static class StringExtension
    {
        public static void BetterDirectoryCreate(this string directoryPath, int attempts = 5)
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

        public static void BetterDirectoryDelete(this string directoryPath, int attempts = 5)
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

        public static void BetterFileDelete(this string filePath, int attempts = 5)
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
    }
}