namespace Sirstrap.Core
{
    public sealed class Setting : ISetting
    {
        private readonly Func<string> _getter;
        private readonly Action<string> _setter;
        private readonly Action? _metricEmitter;

        public Setting(string key, Func<string> getter, Action<string> setter, Action? metricEmitter = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(getter);
            ArgumentNullException.ThrowIfNull(setter);

            Key = key;
            _getter = getter;
            _setter = setter;
            _metricEmitter = metricEmitter;
        }

        public string Key { get; }

        public string Read() => _getter();

        public void Write(string rawValue) => _setter(rawValue);

        public void EmitMetric() => _metricEmitter?.Invoke();
    }
}
