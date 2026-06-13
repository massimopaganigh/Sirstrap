namespace Sirstrap.UI
{
    public partial class App : Application
    {
        private static TrayIcon? _trayIcon;

        public static void ApplyFontFamily()
        {
            try
            {
                if (Current is not App app)
                    return;

                Program.Services.GetRequiredService<ISettingsService>().LoadSettings();

                var fontFamilyName = Program.Services.GetRequiredService<SirstrapConfiguration>().FontFamily;

                FontFamily fontFamily;

                if (fontFamilyName == "JetBrains Mono")
                    fontFamily = new FontFamily("avares://Sirstrap/Assets#JetBrains Mono");
                else
                    fontFamily = new FontFamily(fontFamilyName);

                app.Resources["AppFontFamily"] = fontFamily;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to apply the font family.");
            }
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

#if DEBUG
            this.AttachDeveloperTools();
#endif
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow { DataContext = Program.Services.GetRequiredService<MainWindowViewModel>() };

                ApplyFontFamily();

                var trayMode = Program.Services.GetRequiredService<SirstrapConfiguration>().TrayMode;

                if (trayMode != TrayMode.None)
                    SetTray(true);

                if (trayMode == TrayMode.OnLaunch)
                {
                    void OnFirstOpen(object? sender, EventArgs e)
                    {
                        desktop.MainWindow!.Opened -= OnFirstOpen;

                        desktop.MainWindow.Hide();

                        SetTrayIconVisible(true);
                    }

                    desktop.MainWindow.Opened += OnFirstOpen;
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static void SetTray(bool enabled)
        {
            if (Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            if (enabled
                && _trayIcon == null)
            {
                var showNativeMenuItem = new NativeMenuItem("Show");

                showNativeMenuItem.Click += (_, _) =>
                {
                    if (desktop.MainWindow is { } window)
                    {
                        SetTrayIconVisible(false);

                        window.Show();

                        window.WindowState = WindowState.Normal;
                    }
                };

                var exitNativeMenuItem = new NativeMenuItem("Exit");

                exitNativeMenuItem.Click += (_, _) =>
                {
                    _trayIcon?.Dispose();

                    _trayIcon = null;

                    desktop.Shutdown();
                };

                var trayNativeMenu = new NativeMenu();

                trayNativeMenu.Items.Add(showNativeMenuItem);
                trayNativeMenu.Items.Add(new NativeMenuItemSeparator());
                trayNativeMenu.Items.Add(exitNativeMenuItem);

#pragma warning disable S1075 // avares:// resource URI, not a local path
                _trayIcon = new TrayIcon
                {
                    Icon = new WindowIcon(Avalonia.Platform.AssetLoader.Open(new Uri("avares://Sirstrap/Assets/favicon.ico"))),
                    IsVisible = false,
                    Menu = trayNativeMenu,
                    ToolTipText = "Sirstrap"
                };
#pragma warning restore S1075

                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }
            else if (!enabled
                && _trayIcon != null)
            {
                _trayIcon.Dispose();

                _trayIcon = null;

                desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
            }
        }

        public static void SetTrayIconVisible(bool visible) => _trayIcon?.IsVisible = visible;
    }
}
