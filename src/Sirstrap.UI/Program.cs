namespace Sirstrap.UI
{
    internal sealed class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetBinDirectory(string binDirectoryPath);

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();
#pragma warning restore IDE0079 // Rimuovere l'eliminazione non necessaria

        [STAThread]
        public static void Main(string[] args)
        {
            var binDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

            if (Directory.Exists(binDirectoryPath))
                SetBinDirectory(binDirectoryPath);

            Args = args.Length > 0 ? args : null;

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static string[]? Args { get; set; }
    }
}
