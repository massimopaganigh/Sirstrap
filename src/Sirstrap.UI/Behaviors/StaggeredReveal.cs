using Avalonia.VisualTree;

namespace Sirstrap.UI.Behaviors
{
    public static class StaggeredReveal
    {
        private const int Steps = 25;
        private const double TravelDistance = 12d;

        public static readonly AttachedProperty<bool> IsEnabledProperty = AvaloniaProperty.RegisterAttached<Panel, bool>("IsEnabled", typeof(StaggeredReveal));

        static StaggeredReveal() => IsEnabledProperty.Changed.AddClassHandler<Panel>(OnIsEnabledChanged);

        public static bool GetIsEnabled(Panel panel) => panel.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(Panel panel, bool value) => panel.SetValue(IsEnabledProperty, value);

        private static async Task AnimateChild(Control child, int delayMs)
        {
            var transform = new TranslateTransform(0, TravelDistance);

            child.Opacity = 0;
            child.RenderTransform = transform;

            if (delayMs > 0)
                await Task.Delay(delayMs);

            var stepDelay = 250 / Steps;

            for (var i = 0; i <= Steps; i++)
            {
                var progress = (double)i / Steps;
                var easedProgress = EaseOutExpo(progress);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    child.Opacity = easedProgress;
                    transform.Y = TravelDistance * (1 - easedProgress);
                });

                await Task.Delay(stepDelay);
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                child.ClearValue(Visual.OpacityProperty);
                child.ClearValue(Visual.RenderTransformProperty);
            });
        }

        private static double EaseOutExpo(double x) => x >= 1 ? 1 : 1 - Math.Pow(2, -10 * x);

        private static void Hide(Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (!child.IsVisible)
                    continue;

                child.Opacity = 0;
                child.RenderTransform = new TranslateTransform(0, TravelDistance);
            }
        }

        private static void OnIsEnabledChanged(Panel panel, AvaloniaPropertyChangedEventArgs e)
        {
            if (!e.GetNewValue<bool>())
                return;

            if (panel.IsLoaded)
                Schedule(panel);
            else
                panel.Loaded += OnPanelLoaded;
        }

        private static void OnPanelLoaded(object? sender, RoutedEventArgs e)
        {
            if (sender is not Panel panel)
                return;

            panel.Loaded -= OnPanelLoaded;

            Schedule(panel);
        }

        private static void Reveal(Panel panel)
        {
            var index = 0;

            foreach (var child in panel.Children)
            {
                if (!child.IsVisible)
                    continue;

                _ = AnimateChild(child, index * 50);

                index++;
            }
        }

        private static void Schedule(Panel panel)
        {
            var view = panel.FindAncestorOfType<ViewBase>();

            if (view == null
                || view.OpenAnimationFinished)
            {
                Reveal(panel);

                return;
            }

            Hide(panel);

            void OnOpenAnimationCompleted(object? sender, EventArgs e)
            {
                view.OpenAnimationCompleted -= OnOpenAnimationCompleted;

                Reveal(panel);
            }

            view.OpenAnimationCompleted += OnOpenAnimationCompleted;
        }
    }
}
