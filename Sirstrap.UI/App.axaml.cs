using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Sirstrap.UI.ViewModels;
using Sirstrap.UI.Views;
using System.Linq;

namespace Sirstrap.UI
{
    /// <summary>
    /// Main entry point for the Sirstrap UI Avalonia application that handles
    /// application initialization, framework configuration, and lifecycle events.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the XAML components of the application.
        /// This method loads and processes the XAML markup associated with the application.
        /// </summary>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Called when the Avalonia framework initialization is completed.
        /// This method configures the application's main window, sets up the data context,
        /// and disables duplicate data validation functionality.
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DisableAvaloniaDataAnnotationValidation();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }
            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// Disables the built-in Avalonia data annotation validation to prevent
        /// duplicate validations when using the CommunityToolkit validation system.
        /// This prevents validation rules from being executed twice for the same properties.
        /// </summary>
        private void DisableAvaloniaDataAnnotationValidation()
        {
            var dataValidationPluginsToRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}