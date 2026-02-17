namespace Sirstrap.UI.Views
{
    public class ViewBase : Window
    {
        public ViewBase()
        {
            Opacity = 0;
            RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            RenderTransform = new ScaleTransform(0.1, 0.1);

            Loaded += OnLoaded;
            Closing += OnClosing;
        }

        private async Task AnimateWindowClose()
        {
            var animationDuration = 150;
            var steps = 15;
            var stepDelay = animationDuration / steps;

            for (int i = 0; i <= steps; i++)
            {
                var progress = (double)i / steps;
                var easedProgress = EaseInExpo(progress);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (RenderTransform is ScaleTransform scaleTransform)
                    {
                        scaleTransform.ScaleX = 1.0 - (0.9 * easedProgress);
                        scaleTransform.ScaleY = 1.0 - (0.9 * easedProgress);
                    }

                    Opacity = 0.5 * (1 - progress);
                });

                await Task.Delay(stepDelay);
            }
        }

        private async Task AnimateWindowOpen()
        {
            var animationDuration = 300;
            var steps = 30;
            var stepDelay = animationDuration / steps;

            for (int i = 0; i <= steps; i++)
            {
                var progress = (double)i / steps;
                var easedProgress = EaseOutExpo(progress);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (RenderTransform is ScaleTransform scaleTransform)
                    {
                        scaleTransform.ScaleX = 0.1 + (0.9 * easedProgress);
                        scaleTransform.ScaleY = 0.1 + (0.9 * easedProgress);
                    }

                    Opacity = 0.5 * progress;
                });

                await Task.Delay(stepDelay);
            }
        }

        private static double EaseInExpo(double x) => x <= 0 ? 0 : Math.Pow(2, 10 * (x - 1));

        private static double EaseOutExpo(double x) => x >= 1 ? 1 : 1 - Math.Pow(2, -10 * x);

        protected async void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            e.Cancel = true;

            await AnimateWindowClose();

            Closing -= OnClosing;

            Close();
        }

        protected async void OnLoaded(object? sender, RoutedEventArgs e) => await AnimateWindowOpen();
    }
}
