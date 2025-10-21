namespace Sirstrap.UI
{
    public partial class App : Application
    {
        private void DisableAvaloniaDataAnnotationValidation()
        {
            var dataValidationPluginsToRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            foreach (var plugin in dataValidationPluginsToRemove)
                BindingPlugins.DataValidators.Remove(plugin);
        }

        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DisableAvaloniaDataAnnotationValidation();

                desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };

                ApplyFontFamily();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static void ApplyFontFamily()
        {
            try
            {
                var app = Current as App;
                if (app == null) return;

                var fontFamilyName = SirstrapConfiguration.FontFamily;

                FontFamily fontFamily;
                if (fontFamilyName == "Minecraft")
                {
                    fontFamily = new FontFamily("avares://Sirstrap/Assets#Minecraft");
                }
                else
                {
                    fontFamily = new FontFamily(fontFamilyName);
                }

                // Update the dynamic resource
                app.Resources["AppFontFamily"] = fontFamily;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to apply font family: {0}", ex.Message);
            }
        }
    }
}
