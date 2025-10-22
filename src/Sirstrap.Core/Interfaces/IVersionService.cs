namespace Sirstrap.Core.Interfaces
{
    public interface IVersionService
    {
        public Task<string> GetVersionAsync();
    }
}
