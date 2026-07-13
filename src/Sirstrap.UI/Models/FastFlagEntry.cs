namespace Sirstrap.UI.Models
{
    public partial class FastFlagEntry : ModelBase
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _value = string.Empty;
    }
}
