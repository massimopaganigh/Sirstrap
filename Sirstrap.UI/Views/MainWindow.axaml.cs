using Avalonia.Controls;

namespace Sirstrap.UI.Views
{
    /// <summary>
    /// The main application window for the Sirstrap UI. Serves as the primary container
    /// for all user interface elements and interactions in the application.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Calls InitializeComponent to load and initialize the XAML components
        /// defined in the associated XAML file.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}