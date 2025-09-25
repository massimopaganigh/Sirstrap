namespace Sirstrap.UI.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        public static Window? GetMainWindow() => App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;

        public static Window? GetWindow<T>() where T : Window
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                foreach (var window in desktop.Windows)
                    if (window is T tWindow)
                        return tWindow;

            return null;
        }
    }
}
