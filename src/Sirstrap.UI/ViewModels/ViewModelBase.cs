namespace Sirstrap.UI.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        public static void CloseSpecificWindow<T>() where T : Window
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow })
                foreach (var window in mainWindow.OwnedWindows)
                    if (window is T)
                    {
                        window.Close();

                        break;
                    }
        }

        public static Window? GetMainWindow() => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow } ? mainWindow : null;

        //public static TopLevel? GetTopLevel() => (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow }) ? TopLevel.GetTopLevel(mainWindow) : null;

        //public static void SetMainWindow<T>(T window) where T : Window
        //{
        //    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        //    {
        //        var oldMainWindow = desktop.MainWindow;

        //        if (oldMainWindow != null)
        //        {
        //            foreach (var ownedWindow in oldMainWindow.OwnedWindows.ToList())
        //                ownedWindow.Close();

        //            desktop.MainWindow = window;

        //            desktop.MainWindow.Show();
        //            oldMainWindow.Close();
        //        }
        //    }
        //}
    }
}
