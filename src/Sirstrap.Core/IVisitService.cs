namespace Sirstrap.Core
{
    public interface IVisitService
    {
        Task GenerateVisitAsync(Configuration configuration, string roblosecurityToken, long placeId, int timeoutSeconds);
        Task RunVisitLoopAsync(Configuration configuration, string cookiesFilePath, long placeId, int timeoutSeconds, CancellationToken cancellationToken = default);
    }
}
