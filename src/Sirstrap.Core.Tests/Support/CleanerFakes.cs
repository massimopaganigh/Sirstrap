using Microsoft.Win32;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Sirstrap.Core.Tests.Support
{
    public sealed class FakeStatusLine : IStatusLine
    {
        public List<string> Statuses { get; } = [];

        public int ClearCalls { get; private set; }

        public int HiddenInvocations { get; private set; }

        public void Clear() => ClearCalls++;

        public void InvokeWithStatusHidden(Action action)
        {
            HiddenInvocations++;
            action();
        }

        public void SetStatus(string status) => Statuses.Add(status);
    }

    public sealed class FakeUserInteraction(bool confirmResult = true) : IUserInteraction
    {
        public List<string> Messages { get; } = [];

        public bool Confirm(string message, bool defaultAnswer = false)
        {
            Messages.Add(message);

            return confirmResult;
        }
    }

    public sealed class FakeFolderDeleter : IFolderDeleter
    {
        public List<string> Deleted { get; } = [];

        public void DeleteFolder(string path) => Deleted.Add(path);
    }

    public sealed class FakeProcessManager : IProcessManager
    {
        private readonly HashSet<string> _running;

        public FakeProcessManager(params string[] running) => _running = new(running, StringComparer.OrdinalIgnoreCase);

        public List<string> Killed { get; } = [];

        public HashSet<string> KillFailures { get; } = new(StringComparer.OrdinalIgnoreCase);

        public bool IsProcessRunning(string processName) => _running.Contains(processName);

        public bool TryKillProcess(string processName)
        {
            Killed.Add(processName);

            return !KillFailures.Contains(processName);
        }
    }

    public sealed class FakeUserProfileProvider(params string[] profiles) : IUserProfileProvider
    {
        public IEnumerable<string> GetOtherUserProfileDirectories() => profiles;
    }

    public sealed class FakeSelectiveFolderCleaner : ISelectiveFolderCleaner
    {
        public List<string> Cleaned { get; } = [];

        public void CleanFolderContents(string folderPath) => Cleaned.Add(folderPath);
    }

    public sealed class FakeCleanupRegistryManager : IRegistryManager
    {
        public List<string> CurrentUser { get; } = [];

        public List<string> AllUsers { get; } = [];

        public List<string> LocalMachine { get; } = [];

        public void CleanAllUsersRegistry(IEnumerable<string> keyPaths, ILogger? logger = null) => AllUsers.AddRange(keyPaths);

        public void CleanCurrentUserRegistry(IEnumerable<string> keyPaths, ILogger? logger = null) => CurrentUser.AddRange(keyPaths);

        public void CleanLocalMachineRegistry(IEnumerable<string> keyPaths, ILogger? logger = null) => LocalMachine.AddRange(keyPaths);

        public void CleanRegistryKeys(RegistryKey registryHive, IEnumerable<string> keyPaths, ILogger? logger = null) => CurrentUser.AddRange(keyPaths);

        public void DeleteRegistryKey(RegistryKey registryHive, string keyPath, ILogger? logger = null) => CurrentUser.Add(keyPath);
    }

    public sealed class FakeCleanupStep(string name, Action? onExecute = null) : ICleanupStep
    {
        public int Executions { get; private set; }

        public string Name => name;

        public void Execute()
        {
            Executions++;
            onExecute?.Invoke();
        }
    }

    public sealed class RecordingLogSink : ILogEventSink
    {
        public List<string> Messages { get; } = [];

        public void Emit(LogEvent logEvent) => Messages.Add(logEvent.RenderMessage());
    }
}
