namespace Sirstrap.Core.Update
{
    public interface ISirstrapVersion
    {
        string Channel { get; }

        Version Current { get; }

        string GetFullVersion();
    }
}
