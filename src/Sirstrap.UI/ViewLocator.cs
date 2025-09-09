using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Sirstrap.UI.ViewModels;
using Sirstrap.UI.Views;

namespace Sirstrap.UI
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            return param switch
            {
                MainWindowViewModel => new MainWindow(),
                _ => new TextBlock
                {
                    Text = "Not Found: " + param.GetType().Name
                }
            };
        }

        public bool Match(object? data) => data is ViewModelBase;
    }
}