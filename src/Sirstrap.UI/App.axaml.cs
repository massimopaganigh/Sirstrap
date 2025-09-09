using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Sirstrap.UI.ViewModels;
using Sirstrap.UI.Views;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sirstrap.UI
{
    public partial class App : Application
    {
        [RequiresUnreferencedCode("Calls Avalonia.Data.Core.Plugins.BindingPlugins.DataValidators")]
        private void DisableAvaloniaDataAnnotationValidation()
        {
            DataAnnotationsValidationPlugin[] dataValidationPluginsToRemove = [.. BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>()];

            foreach (DataAnnotationsValidationPlugin plugin in dataValidationPluginsToRemove)
                BindingPlugins.DataValidators.Remove(plugin);
        }

        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DisableAvaloniaDataAnnotationValidation();

                MainWindowViewModel mwvm = new();
                MainWindow mw = new()
                {
                    DataContext = mwvm,
                };

                mwvm.SetMainWindow(mw);

                desktop.MainWindow = mw;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}