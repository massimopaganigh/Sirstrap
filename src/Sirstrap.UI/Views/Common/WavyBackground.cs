namespace Sirstrap.UI.Views.Common
{
    public class WavyBackground : Control
    {
        private const int LineCount = 18;
        private const double Speed = 0.0006;

        private static readonly Color _defaultAccentColor = Color.Parse("#454ee6");

        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly DispatcherTimer _timer;

        public WavyBackground()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
            _timer.Tick += (_, _) => InvalidateVisual();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            _timer.Start();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _timer.Stop();

            base.OnDetachedFromVisualTree(e);
        }

        public override void Render(DrawingContext context)
        {
            var w = Bounds.Width;
            var h = Bounds.Height;

            if (w <= 0
                || h <= 0)
                return;

            var color = Application.Current?.TryGetResource("SystemAccentColor", null, out var resource) == true && resource is Color accentColor
                ? accentColor
                : _defaultAccentColor;

            var t = _stopwatch.ElapsedMilliseconds * Speed;

            for (var i = 0; i < LineCount; i++)
            {
                var progress = i / (double)(LineCount - 1);
                var baseY = h * 0.1 + progress * h * 0.8;
                var phase = i * 0.7 + t;
                var amplitude = h * (0.04 + progress * 0.03);
                var lineOpacity = 0.08 + progress * 0.08;

                var geometry = new StreamGeometry();

                using (var geometryContext = geometry.Open())
                {
                    geometryContext.BeginFigure(new Point(-10, baseY), false);

                    for (double x = 0; x <= w + 10; x += 4)
                    {
                        var xNorm = x / w;
                        var y = baseY
                            + Math.Sin(xNorm * 3 + phase) * amplitude
                            + Math.Sin(xNorm * 1.5 + phase * 0.7 + i * 0.3) * amplitude * 0.6;

                        geometryContext.LineTo(new Point(x, y));
                    }

                    geometryContext.EndFigure(false);
                }

                context.DrawGeometry(null, new Pen(new SolidColorBrush(color, lineOpacity), 2.5), geometry);
            }
        }
    }
}
