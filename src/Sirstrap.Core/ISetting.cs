namespace Sirstrap.Core
{
    public interface ISetting
    {
        string Key { get; }

        string Read();

        void Write(string rawValue);

        void EmitMetric();
    }
}
