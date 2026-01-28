namespace Sirstrap.UI.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<string> _exploits = [];

        [ObservableProperty]
        private ObservableCollection<string> _fonts = [];

        [ObservableProperty]
        private Settings _settings = new();

        public SettingsWindowViewModel()
        {
            GetExploits();
            GetFonts();
        }

        private void GetExploits()
        {
            try
            {
                Exploits = new ObservableCollection<string>
                {
                    "SirHurt V5",
                    "Wave",
                    "Solara",
                    "Electron",
                    "Nexus",
                    "Celery"
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetExploits));
            }
        }

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
        private void RunSirHurt()
        {
            try
            {
                var sirHurt = Path.Combine(Settings.SirHurtPath, "bootstrapper.exe");

                if (!File.Exists(sirHurt))
                    return;

                ProcessStartInfo processStartInfo = new()
                {
                    FileName = sirHurt
                };

                using Process process = new()
                {
                    StartInfo = processStartInfo
                };

                process.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(RunSirHurt));
            }
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                Settings.Set();

                App.ApplyFontFamily();

                CloseSpecificWindow<SettingsWindow>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(Save));
            }
        }
    }
}
