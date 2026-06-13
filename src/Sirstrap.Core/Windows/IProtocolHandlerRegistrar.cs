namespace Sirstrap.Core.Windows
{
    public interface IProtocolHandlerRegistrar
    {
        bool RegisterProtocolHandler(string protocol, string[] arguments);

        void UnregisterProtocolHandler(string protocol, ILogger? logger = null);

        void UnregisterProtocolHandlers(IEnumerable<string> protocols, ILogger? logger = null);
    }
}
