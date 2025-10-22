namespace Sirstrap.Core.Interfaces
{
    public interface IRegistryService
    {
        public bool RegisterProtocolHandler(string protocol, string[] arguments);
    }
}
