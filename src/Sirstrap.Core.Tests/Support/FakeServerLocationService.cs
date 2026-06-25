namespace Sirstrap.Core.Tests.Support
{
    public sealed class FakeServerLocationService(string location = "Somewhere") : IServerLocationService
    {
        public void ClearCache()
        {
        }

        public Task<string> GetServerLocationAsync(string ipAddress) => Task.FromResult(location);
    }
}
