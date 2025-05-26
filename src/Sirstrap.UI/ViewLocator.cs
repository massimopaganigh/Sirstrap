using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Sirstrap.UI.ViewModels;
using System;

namespace Sirstrap.UI
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
#pragma warning disable IL2057 // Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.
            var type = Type.GetType(name);
#pragma warning restore IL2057 // Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.

            if (type != null)
                return (Control)Activator.CreateInstance(type)!;

            return new TextBlock
            {
                Text = $"Not Found: {name}"
            };
        }

        public bool Match(object? data) => data is ViewModelBase;
    }
}