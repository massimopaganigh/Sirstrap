namespace Sirstrap.UI.Views.Common
{
    public class SettingsRow : ContentControl
    {
        public static readonly StyledProperty<string?> LabelProperty = AvaloniaProperty.Register<SettingsRow, string?>(nameof(Label));

        public string? Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
    }
}
