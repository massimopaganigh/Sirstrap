using Sirstrap.UI.Models;

namespace Sirstrap.UI.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Settings _settings = new();

        [RelayCommand]
        private void Save()
        {
            Settings.Set();

            GetWindow<SettingsWindow>()?.Close();
        }
    }
}
