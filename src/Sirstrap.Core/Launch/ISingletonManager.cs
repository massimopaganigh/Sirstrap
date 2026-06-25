namespace Sirstrap.Core.Launch
{
    public interface ISingletonManager
    {
        event EventHandler<InstanceType>? InstanceTypeChanged;

        InstanceType CurrentInstanceType { get; }

        bool HasCapturedSingleton { get; }

        bool CaptureSingleton();

        bool ReleaseSingleton();
    }
}
