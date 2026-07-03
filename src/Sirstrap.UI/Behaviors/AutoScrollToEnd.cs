namespace Sirstrap.UI.Behaviors
{
    public static class AutoScrollToEnd
    {
        public static readonly AttachedProperty<bool> IsEnabledProperty = AvaloniaProperty.RegisterAttached<ScrollViewer, bool>("IsEnabled", typeof(AutoScrollToEnd));

        static AutoScrollToEnd() => IsEnabledProperty.Changed.AddClassHandler<ScrollViewer>(OnIsEnabledChanged);

        public static bool GetIsEnabled(ScrollViewer scrollViewer) => scrollViewer.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(ScrollViewer scrollViewer, bool value) => scrollViewer.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(ScrollViewer scrollViewer, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.GetNewValue<bool>())
                scrollViewer.ScrollChanged += OnScrollChanged;
            else
                scrollViewer.ScrollChanged -= OnScrollChanged;
        }

        private static void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer
                && e.ExtentDelta.Y > 0)
                scrollViewer.ScrollToEnd();
        }
    }
}
