namespace Sirstrap.UI.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        private readonly IUninstallService _uninstallService;
        private readonly IWeaoService _weaoService;

        [ObservableProperty]
        private string _currentFullVersion;

        [ObservableProperty]
        private ObservableCollection<string> _fontFamilies = [];

        [ObservableProperty]
        private ObservableCollection<VersionSourceOption> _versionSources = [];

        [ObservableProperty]
        private VersionSourceOption? _selectedVersionSource;

        [ObservableProperty]
        private Settings _settings;

        public SettingsWindowViewModel(Settings settings, ISirstrapVersion sirstrapVersion, IUninstallService uninstallService, IWeaoService weaoService)
        {
            _settings = settings;
            _currentFullVersion = sirstrapVersion.GetFullVersion();
            _uninstallService = uninstallService;
            _weaoService = weaoService;

            GetFontFamilies();

            _ = LoadVersionSourcesAsync();
        }

        private async Task LoadVersionSourcesAsync()
        {
            try
            {
                var options = await VersionSourceCatalog.BuildAsync(_weaoService);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    VersionSources = new ObservableCollection<VersionSourceOption>(options);
                    SelectedVersionSource = VersionSources.FirstOrDefault(option => string.Equals(option.Value, Settings.RobloxVersionSource, StringComparison.OrdinalIgnoreCase))
                        ?? VersionSources.FirstOrDefault();
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(LoadVersionSourcesAsync));
            }
        }

        partial void OnSelectedVersionSourceChanged(VersionSourceOption? value)
        {
            if (value != null)
                Settings.RobloxVersionSource = value.Value;
        }

        [RelayCommand]
        private async Task BrowseInstallationPathAsync()
        {
            try
            {
                var mainWindow = GetMainWindow();

                if (mainWindow == null)
                    return;

                var storageProvider = mainWindow.StorageProvider;

                var startFolder = await storageProvider.TryGetFolderFromPathAsync(Settings.RobloxInstallationPath);

                var result = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Roblox installation path",
                    AllowMultiple = false,
                    SuggestedStartLocation = startFolder
                });

                if (result.Count > 0)
                    Settings.RobloxInstallationPath = result[0].Path.LocalPath;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(BrowseInstallationPathAsync));
            }
        }

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


        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

        [RelayCommand]
        private async Task UninstallAsync()
        {
            try
            {
                const uint MB_YESNO = 0x00000004;
                const uint MB_ICONWARNING = 0x00000030;
                const int IDYES = 6;

                var result = await Task.Run(() =>
                    MessageBoxW(
                        IntPtr.Zero,
                        "This will:\n  • Remove Sirstrap protocol handler from the registry\n  • Delete the Sirstrap data folder (%LocalAppData%\\Sirstrap)\n  • Delete the Sirstrap executable\n\nThis action cannot be undone. Are you sure?",
                        "Uninstall Sirstrap",
                        MB_YESNO | MB_ICONWARNING));

                if (result != IDYES)
                    return;

                await Task.Run(_uninstallService.Uninstall);

                await Dispatcher.UIThread.InvokeAsync(() => Environment.Exit(0));
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(UninstallAsync));
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

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        App.SetTray(Settings.SirstrapTrayMode != TrayMode.None);

                        CloseSpecificWindow<SettingsWindow>();
                    });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(SaveAsync));
            }
        }

        public IReadOnlyList<TrayMode> TrayModes { get; } = Enum.GetValues<TrayMode>();
    }
}
