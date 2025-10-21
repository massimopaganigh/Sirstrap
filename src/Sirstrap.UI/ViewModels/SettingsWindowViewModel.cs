namespace Sirstrap.UI.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<string> _fonts = [];

        [ObservableProperty]
        private Settings _settings = new();

        public SettingsWindowViewModel() => GetFonts();

        private void GetFonts()
        {
            try
            {
                var fonts = new List<string>
                {
                    "Minecraft"
                };

                fonts.AddRange(FontManager.Current.SystemFonts.Select(x => x.Name).Distinct().OrderBy(x => x));

                Fonts = new ObservableCollection<string>(fonts);
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetFonts));
            }
        }

        [RelayCommand]
        private void Save()
        {
            Settings.Set();

            App.ApplyFontFamily();

            CloseSpecificWindow<SettingsWindow>();
        }
    }
}
