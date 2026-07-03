using Avalonia.VisualTree;

namespace Sirstrap.UI.Views.Common
{
    public class SettingsList : StackPanel
    {
        public static readonly StyledProperty<string?> SearchTermProperty = AvaloniaProperty.Register<SettingsList, string?>(nameof(SearchTerm));

        private List<Control>? _baseline;

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            ApplySearch();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SearchTermProperty)
                ApplySearch();
        }

        public string? SearchTerm
        {
            get => GetValue(SearchTermProperty);
            set => SetValue(SearchTermProperty, value);
        }

        private void ApplySearch()
        {
            if (Children.Count == 0)
                return;

            _baseline ??= [.. Children.Cast<Control>()];

            var term = SearchTerm;
            var hasTerm = !string.IsNullOrWhiteSpace(term);
            var ordered = hasTerm ? [.. _baseline.OrderByDescending(x => Matches(x, term!))] : _baseline;

            for (var i = 0; i < ordered.Count; i++)
            {
                var child = ordered[i];

                child.Opacity = !hasTerm || Matches(child, term!) ? 1 : 0.4;

                var currentIndex = Children.IndexOf(child);

                if (currentIndex != i && currentIndex >= 0)
                    Children.Move(currentIndex, i);
            }

            var scrollViewer = this.FindAncestorOfType<ScrollViewer>();

            if (scrollViewer != null)
                scrollViewer.Offset = scrollViewer.Offset.WithY(0);
        }

        private static bool Matches(Control control, string term) => control is SettingsRow row && (row.Label?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false);
    }
}
