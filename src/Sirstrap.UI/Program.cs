using Avalonia;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Sirstrap.UI
{
    internal sealed class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();

        [STAThread]
        public static void Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

                if (Directory.Exists(binPath))
                    SetDllDirectory(binPath);
            }

            Args = args.Length > 0 ? args : null;

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static string[]? Args { get; set; }
    }
}