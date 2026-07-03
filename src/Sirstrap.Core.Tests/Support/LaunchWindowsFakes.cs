using Serilog;

namespace Sirstrap.Core.Tests.Support
{
    public sealed class FakeSirstrapVersion(Version current, string channel = "-beta") : ISirstrapVersion
    {
        public string Channel { get; } = channel;

        public Version Current { get; } = current;

        public string GetFullVersion() => $"v{Current}{Channel}";
    }

    public sealed class FakeSingletonManager : ISingletonManager
    {
        public event EventHandler<InstanceType>? InstanceTypeChanged;

        public InstanceType CurrentInstanceType { get; private set; } = InstanceType.None;

        public bool HasCapturedSingleton { get; private set; }

        public int CaptureCalls { get; private set; }

        public int ReleaseCalls { get; private set; }

        public bool CaptureSingleton()
        {
            CaptureCalls++;
            HasCapturedSingleton = true;
            RaiseInstanceTypeChanged(InstanceType.Master);

            return true;
        }

        public bool ReleaseSingleton()
        {
            ReleaseCalls++;
            HasCapturedSingleton = false;
            RaiseInstanceTypeChanged(InstanceType.None);

            return true;
        }

        public void RaiseInstanceTypeChanged(InstanceType type)
        {
            CurrentInstanceType = type;
            InstanceTypeChanged?.Invoke(this, type);
        }
    }

    public sealed class FakeIncognitoManager : IIncognitoManager
    {
        public int MoveCalls { get; private set; }

        public int RestoreCalls { get; private set; }

        public bool MoveRobloxFolderToCache()
        {
            MoveCalls++;

            return true;
        }

        public bool RestoreRobloxFolderFromCache()
        {
            RestoreCalls++;

            return true;
        }
    }

    public sealed class FakeRobloxProcessService : IRobloxProcessService
    {
        public int KillAllCalls { get; private set; }

        public bool AnyGameProcessRunning(IEnumerable<int> processIds) => false;

        public List<int> FindNewGameProcessIds(HashSet<int> knownIds, int attempts = 20) => [];

        public int GetRunningGameProcessCount() => 0;

        public void KillAll() => KillAllCalls++;

        public void LogRunningGameProcesses()
        {
        }

        public HashSet<int> SnapshotGameProcessIds() => [];

        public bool WaitForExit(int timeoutMs = 5000) => true;
    }

    public sealed class FakeUacService : IUacService
    {
        public bool EnsureAdministratorPrivileges(Func<bool> operation, string[] arguments, string operationDescription) => operation();

        public bool IsRunningAsAdministrator() => false;

        public bool RestartAsAdministrator(string[] arguments) => false;
    }

    public sealed class FakeProtocolHandlerRegistrar : IProtocolHandlerRegistrar
    {
        public List<string> Registered { get; } = [];

        public List<string> Unregistered { get; } = [];

        public bool RegisterProtocolHandler(string protocol, string[] arguments)
        {
            Registered.Add(protocol);

            return true;
        }

        public void UnregisterProtocolHandler(string protocol, ILogger? logger = null) => Unregistered.Add(protocol);

        public void UnregisterProtocolHandlers(IEnumerable<string> protocols, ILogger? logger = null) => Unregistered.AddRange(protocols);
    }
}
