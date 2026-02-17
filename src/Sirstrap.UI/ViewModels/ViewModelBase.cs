namespace Sirstrap.UI.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        public static void CloseSpecificWindow<T>() where T : Window
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow })
                mainWindow.OwnedWindows.OfType<T>().FirstOrDefault()?.Close();
        }

        public static Window? GetMainWindow() => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow } ? mainWindow : null;
    }
}
