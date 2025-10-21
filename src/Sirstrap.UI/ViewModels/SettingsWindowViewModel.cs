namespace Sirstrap.UI.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Settings _settings = new();

        public ObservableCollection<string> AvailableFonts { get; }

        public SettingsWindowViewModel()
        {
            AvailableFonts = new ObservableCollection<string>(GetSystemFonts());
        }

        private IEnumerable<string> GetSystemFonts()
        {
            var fonts = new List<string> { "Minecraft" };

            try
            {
                var systemFonts = FontManager.Current.SystemFonts
                    .Select(f => f.Name)
                    .Distinct()
                    .OrderBy(f => f)
                    .ToList();

                fonts.AddRange(systemFonts);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to retrieve system fonts: {0}", ex.Message);
            }

            return fonts;
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
