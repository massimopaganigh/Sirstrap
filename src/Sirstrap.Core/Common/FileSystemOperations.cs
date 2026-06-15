namespace Sirstrap.Core.Common
{
    public static class FileSystemOperations
    {
        #region PRIVATE METHODS
        private static void Execute(Action operation, int attempts, string description)
        {
            try
            {
                for (var attempt = 1; attempt <= attempts; attempt++)
                    try
                    {
                        operation();

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
                Log.Error(ex, "[!] Failed {Operation}.", description);

                throw new InvalidOperationException($"Error {description}.", ex);
            }
        }
        #endregion

        public static void CreateDirectory(string directoryPath, int attempts = 5) => Execute(() =>
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }, attempts, $"creating directory: {directoryPath}");

        public static void DeleteDirectory(string directoryPath, int attempts = 5) => Execute(() =>
        {
            if (Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, true);
        }, attempts, $"deleting directory: {directoryPath}");

        public static void DeleteFile(string filePath, int attempts = 5) => Execute(() =>
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }, attempts, $"deleting file: {filePath}");

        public static void MoveDirectory(string sourcePath, string destinationPath, int attempts = 5) => Execute(() => Directory.Move(sourcePath, destinationPath), attempts, $"moving directory: {sourcePath} -> {destinationPath}");
    }
}
