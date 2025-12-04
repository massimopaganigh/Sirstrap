namespace Sirstrap.UI
{
    public partial class App : Application
    {
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "<In sospeso>")]
        private void DisableAvaloniaDataAnnotationValidation()
        {
            var dataValidationPluginsToRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            foreach (var plugin in dataValidationPluginsToRemove)
                BindingPlugins.DataValidators.Remove(plugin);
        }

        public static void ApplyFontFamily()
        {
            try
            {
                if (Current is not App app)
                    return;

                var fontFamilyName = SirstrapConfiguration.FontFamily;

                FontFamily fontFamily;

                if (fontFamilyName == "Minecraft")
                    fontFamily = new FontFamily("avares://Sirstrap/Assets#Minecraft");
                else
                    fontFamily = new FontFamily(fontFamilyName);

                app.Resources["AppFontFamily"] = fontFamily;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(ApplyFontFamily));
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
                DisableAvaloniaDataAnnotationValidation();

                desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };

                ApplyFontFamily();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
