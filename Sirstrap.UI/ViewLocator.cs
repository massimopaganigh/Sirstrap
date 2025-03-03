using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Sirstrap.UI.ViewModels;
using System;

namespace Sirstrap.UI
{
    /// <summary>
    /// Locates views for view models using a naming convention.
    /// </summary>
    /// <remarks>
    /// This class implements Avalonia's <see cref="IDataTemplate"/> interface to provide
    /// view resolution based on a naming convention. It locates a view for a view model
    /// by replacing "ViewModel" with "View" in the view model's full type name.
    /// </remarks>
    public class ViewLocator : IDataTemplate
    {
        /// <summary>
        /// Builds a control for the specified view model.
        /// </summary>
        /// <param name="param">The view model for which to build a view.</param>
        /// <returns>
        /// A control representing the view, or a <see cref="TextBlock"/> with an error message 
        /// if the view cannot be found.
        /// </returns>
        /// <remarks>
        /// The method attempts to find a view type by replacing "ViewModel" with "View" in the
        /// full name of the view model's type. If a matching type is found, an instance is created
        /// using <see cref="Activator.CreateInstance(Type)"/>.
        /// </remarks>
        public Control? Build(object? param)
        {
            if (param is null)
            {
                return null;
            }

            var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        /// <summary>
        /// Determines whether this data template matches the specified data.
        /// </summary>
        /// <param name="data">The data to check.</param>
        /// <returns>
        /// <c>true</c> if the data is a <see cref="ViewModelBase"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}