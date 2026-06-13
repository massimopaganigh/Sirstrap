namespace Sirstrap.Core.Activity
{
    public interface IServerLocationService
    {
        void ClearCache();

        Task<string> GetServerLocationAsync(string ipAddress);
    }
}
