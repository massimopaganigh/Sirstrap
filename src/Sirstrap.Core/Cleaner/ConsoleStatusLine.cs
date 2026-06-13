namespace Sirstrap.Core.Cleaner
{
    public sealed class ConsoleStatusLine : IStatusLine
    {
        private int _drawnLength;
        private readonly bool _enabled = !Console.IsOutputRedirected;
        private string _status = string.Empty;
        private readonly Lock _sync = new();

        public void Clear() => SetStatus(string.Empty);

        public void InvokeWithStatusHidden(Action action)
        {
            lock (_sync)
            {
                Erase();
                action();
                Draw();
            }
        }

        public void SetStatus(string status)
        {
            lock (_sync)
            {
                Erase();
                _status = status;
                Draw();
            }
        }

        private static int GetMaxWidth()
        {
            try
            {
                return Math.Max(Console.WindowWidth - 1, 20);
            }
            catch (IOException)
            {
                return 79;
            }
        }

        private void Draw()
        {
            if (!_enabled || _status.Length == 0)
                return;

            int maxTextWidth = GetMaxWidth() - 2;
            string text = _status.Length > maxTextWidth ? _status[..maxTextWidth] : _status;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"» {text}");
            Console.ResetColor();

            _drawnLength = text.Length + 2;
        }

        private void Erase()
        {
            if (_drawnLength == 0)
                return;

            Console.Write('\r');
            Console.Write(new string(' ', _drawnLength));
            Console.Write('\r');

            _drawnLength = 0;
        }
    }
}
