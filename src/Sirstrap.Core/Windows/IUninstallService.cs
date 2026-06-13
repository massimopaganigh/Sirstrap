namespace Sirstrap.Core.Windows
{
    public interface IUninstallService
    {
        void ScheduleCleanup();

        void Uninstall();

        void UnregisterProtocols();
    }
}
