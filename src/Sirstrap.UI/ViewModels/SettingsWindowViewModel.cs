namespace Sirstrap.UI.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _currentFullVersion = SirstrapUpdateService.GetCurrentFullVersion();

        [ObservableProperty]
        private ObservableCollection<string> _fontFamilies = [];

        [ObservableProperty]
        private Settings _settings = new();

        public SettingsWindowViewModel() => GetFontFamilies();

        private void GetFontFamilies()
        {
            try
            {
                var fontFamilies = new List<string>
                {
                    "JetBrains Mono"
                };
                fontFamilies.AddRange(FontManager.Current.SystemFonts.Select(x => x.Name).Distinct().OrderBy(x => x));

                FontFamilies = new ObservableCollection<string>(fontFamilies);
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetFontFamilies));
            }
        }

        [RelayCommand]
        private async Task OpenIniFileAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var iniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Sirstrap.ini");

                    if (!File.Exists(iniPath))
                        File.Create(iniPath).Close();

                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = iniPath,
                        UseShellExecute = true,
                        Verb = "open"
                    };

                    using var process = new Process
                    {
                        StartInfo = processStartInfo
                    };

                    process.Start();
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(OpenIniFileAsync));
            }
        }

        [RelayCommand]
        private async Task RunSirHurtAsync()
        {
            try
            {
                await Task.Run(() =>
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
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(RunSirHurtAsync));
            }
        }


        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    Settings.Set();

                    App.ApplyFontFamily();

                    CloseSpecificWindow<SettingsWindow>();
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(SaveAsync));
            }
        }
    }
}
