namespace Sirstrap.Core.Update
{
    public sealed class UpdateApplier(HttpClient httpClient)
    {
        private const string SIRSTRAP_EXE_FILENAME = "Sirstrap.exe";
        private const string SIRSTRAP_ZIP_FILENAME = "Sirstrap.zip";
        private const string UPDATE_BATCH_FILENAME = "update.bat";
        private const string UPDATE_FOLDER_NAME = "Update";

        public async Task ApplyAsync(string downloadUri, string[] args)
        {
            var updateDirectory = PrepareUpdateDirectory();

            await DownloadAndExtractAsync(downloadUri, updateDirectory);
            await CreateAndExecuteUpdateBatchAsync(updateDirectory, Path.GetDirectoryName(GetCurrentExecutablePath()) ?? AppDomain.CurrentDomain.BaseDirectory, args);
        }

        private static string BuildArgumentsString(string[] args)
        {
            if (args == null || args.Length == 0)
                return string.Empty;

            return " " + string.Join(" ", args.Select(arg => arg.Contains(' ') ? $"\"{arg.Replace("\"", "\"\"")}\"" : arg));
        }

        private static async Task CreateAndExecuteUpdateBatchAsync(string updateDirectory, string exeDirectory, string[] args)
        {
            var batchPath = Path.Combine(updateDirectory, UPDATE_BATCH_FILENAME);
            var batchContent = $@"
@echo off
echo Updating Sirstrap...
timeout /t 2 /nobreak >nul
xcopy ""{updateDirectory}\*"" ""{exeDirectory}"" /E /Y
start """" ""{Path.Combine(exeDirectory, SIRSTRAP_EXE_FILENAME)}""{BuildArgumentsString(args)}
exit
";

            await File.WriteAllTextAsync(batchPath, batchContent);

            ProcessStartInfo updateBatStartInfo = new()
            {
                FileName = batchPath,
                CreateNoWindow = true,
                UseShellExecute = true
            };

            Log.Information("[*] Applying the Sirstrap update to {ExeDirectory}...", exeDirectory);
            Process.Start(updateBatStartInfo);
            Environment.Exit(0);
        }

        private async Task DownloadAndExtractAsync(string downloadUri, string updateDirectory)
        {
            Log.Information("[*] Downloading the Sirstrap update from {DownloadUri}...", downloadUri);

            var zipPath = Path.Combine(updateDirectory, SIRSTRAP_ZIP_FILENAME);

            await File.WriteAllBytesAsync(zipPath, await httpClient.GetByteArrayAsync(downloadUri));

            await Task.Run(() => ZipFile.ExtractToDirectory(zipPath, updateDirectory, overwriteFiles: true));
            File.Delete(zipPath);
        }

        private static string GetCurrentExecutablePath() => Process.GetCurrentProcess().MainModule?.FileName ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SIRSTRAP_EXE_FILENAME);

        private static string PrepareUpdateDirectory()
        {
            var updateDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", UPDATE_FOLDER_NAME);

            if (Directory.Exists(updateDirectory))
            {
                Log.Information("[*] Cleaning the update directory {UpdateDirectory}...", updateDirectory);

                try
                {
                    Directory.Delete(updateDirectory, recursive: true);
                    Directory.CreateDirectory(updateDirectory);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[!] Failed to clean the update directory {UpdateDirectory}.", updateDirectory);
                }
            }
            else
            {
                Log.Information("[*] Creating the update directory {UpdateDirectory}...", updateDirectory);
                Directory.CreateDirectory(updateDirectory);
            }

            return updateDirectory;
        }
    }
}
