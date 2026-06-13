namespace Sirstrap.Core.Update
{
    public interface ISirstrapUpdateService
    {
        Task<string> GetLatestChangelogAsync();

        Task UpdateAsync(SirstrapType sirstrapType, string[] args);
    }
}
