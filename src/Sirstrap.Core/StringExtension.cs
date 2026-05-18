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
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error creating directory: {0}.", directoryPath);

                throw new InvalidOperationException($"Error creating directory: {directoryPath}.", ex);
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
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error deleting directory: {0}.", directoryPath);

                throw new InvalidOperationException($"Error deleting directory: {directoryPath}.", ex);
            }
        }

        public static void BetterDirectoryMove(this string sourcePath, string destinationPath, int attempts = 5)
        {
            try
            {
                foreach (int attempt in Enumerable.Range(1, attempts))
                    try
                    {
                        Directory.Move(sourcePath, destinationPath);

                        return;
                    }
                    catch (IOException)
                    {
                        if (attempt == attempts)
                            throw;

                        Thread.Sleep(100 * attempt);
                    }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error moving directory: {0} -> {1}.", sourcePath, destinationPath);

                throw new InvalidOperationException($"Error moving directory: {sourcePath} -> {destinationPath}.", ex);
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
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Error deleting file: {0}.", filePath);

                throw new InvalidOperationException($"Error deleting file: {filePath}.", ex);
            }
        }
    }
}
