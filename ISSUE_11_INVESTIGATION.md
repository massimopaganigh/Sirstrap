# Investigation: Sirstrap Compatibility with Synapse Blue (Issue #11)

**Issue:** Synapse Blue cannot reliably detect Roblox processes when launched via Sirstrap
**Investigation Date:** 2025-10-27
**Status:** Analysis Complete

---

## Executive Summary

Sirstrap's unique process management architecture creates challenges for third-party tools like Synapse Blue that rely on standard process detection methods. This investigation identifies **5 key architectural differences** that likely cause the incompatibility and proposes **4 potential solutions** with varying levels of invasiveness.

---

## Background Context

### What is Sirstrap?
Sirstrap is a Roblox bootstrapper that manages Roblox installation, updates, and process launching. Unlike standard bootstrappers, it employs sophisticated process coordination mechanisms.

### What is Synapse Blue?
Synapse Blue is a custom UI for SirHurt (a Roblox exploit/scripting tool) that needs to detect and interact with Roblox processes for injection purposes.

### Related History
- **Issue #2**: SirHurt injection failed with Sirstrap.UI but worked with CLI
- **Fix (PR #3)**: Changed to `UseShellExecute = true` and added UAC helper
- **Current Issue**: Synapse Blue cannot locate Roblox processes launched by Sirstrap

---

## Sirstrap's Unique Process Management Architecture

### 1. **Mutex-Based Singleton Control**
**File:** `src/Sirstrap.Core/SingletonManager.cs`

```csharp
// Uses Windows mutex for exclusive control
private const string MUTEX_NAME = "ROBLOX_singletonMutex";
```

**Behavior:**
- **Master Instance:** Holds mutex, kills all existing Roblox processes before launch
- **Slave Instance:** Defers to Master, allows concurrent execution
- **Impact:** Roblox processes are terminated and restarted, changing PIDs frequently

**Relevance to Issue #11:**
- Process IDs change when Sirstrap kills and relaunches Roblox
- Third-party tools tracking PIDs will lose the process reference
- Timing-sensitive detection can fail during singleton transitions

---

### 2. **UseShellExecute = true (Critical)**
**File:** `src/Sirstrap.Core/RobloxLauncher.cs:33`

```csharp
ProcessStartInfo robloxPlayerBetaExeStartInfo = new()
{
    FileName = robloxPlayerBetaExePath,
    WorkingDirectory = Path.GetDirectoryName(robloxPlayerBetaExePath),
    UseShellExecute = true  // KEY DIFFERENCE
};
```

**What This Does:**
- Delegates process creation to Windows shell (`ShellExecute` Win32 API)
- Creates an **independent process tree** (no parent-child relationship)
- Roblox process appears as a **top-level process**, not a child of Sirstrap

**Standard Bootstrapper Approach:**
Most bootstrappers use `UseShellExecute = false`:
- Creates direct parent-child process relationship
- Child process handle maintained by parent
- Easy to enumerate via `GetChildProcesses()` or similar APIs

**Relevance to Issue #11:**
- Synapse Blue likely enumerates child processes of the bootstrapper
- With independent process tree, Roblox is NOT a child of Sirstrap
- Standard process tree traversal fails to find Roblox

---

### 3. **Process Handle Release After WaitForInputIdle**
**File:** `src/Sirstrap.Core/RobloxLauncher.cs:58-62`

```csharp
Process? robloxPlayerBetaExeProcess = Process.Start(robloxPlayerBetaExeStartInfo);
robloxPlayerBetaExeProcess.WaitForInputIdle();

// After this point, Sirstrap only monitors via polling
// No persistent handle to the process is maintained
```

**Behavior:**
- Launches process, waits for UI to be ready
- Then relies on **name-based polling** (`Process.GetProcessesByName("RobloxPlayerBeta")`)
- Does not maintain a `Process` object reference

**Standard Bootstrapper Approach:**
- Maintains process handle throughout lifetime
- Can provide PID, handle, or other metadata to third-party tools

**Relevance to Issue #11:**
- No IPC mechanism or shared memory for process metadata
- Third-party tools cannot query Sirstrap for process information
- Must independently discover Roblox process by name

---

### 4. **Adaptive Polling Monitoring**
**File:** `src/Sirstrap.UI/ViewModels/MainWindowViewModel.cs:54-119`

```csharp
// Adaptive polling interval: 100ms (active) to 10000ms (idle)
private void GetRobloxProcesses()
{
    int processCount = Process.GetProcessesByName("RobloxPlayerBeta").Length;
    // ...
}
```

**Behavior:**
- Polls for `RobloxPlayerBeta` by name periodically
- No event-driven process tracking
- No synchronization with external tools

**Relevance to Issue #11:**
- Race conditions possible if Synapse Blue checks during process transition
- No notification mechanism for external tools

---

### 5. **Incognito Mode (Folder Isolation)**
**File:** `src/Sirstrap.Core/IncognitoManager.cs`

```csharp
// Moves %LocalAppData%\Roblox to cache before launch
MoveRobloxFolderToCache()
// Restores after Roblox closes
RestoreRobloxFolderFromCache()
```

**Behavior:**
- When enabled, relocates Roblox profile folder
- Roblox runs with fresh/temporary profile
- Folder restored on exit

**Relevance to Issue #11:**
- Profile path changes might affect Synapse Blue's detection heuristics
- Temporary profile state might confuse tools expecting standard folder structure

---

## Why Standard Bootstrappers Work Differently

### Typical Bootstrapper Pattern:
```csharp
ProcessStartInfo psi = new()
{
    FileName = "RobloxPlayerBeta.exe",
    UseShellExecute = false,  // Direct parent-child relationship
    CreateNoWindow = false
};

Process robloxProcess = Process.Start(psi);
// Keep process handle alive
// Provide API/IPC for third-party tools to query PID
```

### Sirstrap's Pattern:
```csharp
ProcessStartInfo psi = new()
{
    FileName = "RobloxPlayerBeta.exe",
    UseShellExecute = true,  // Independent process via shell
    WorkingDirectory = versionFolder
};

Process.Start(psi).WaitForInputIdle();
// Release handle, monitor by name polling
// No API for external process queries
```

---

## Root Cause Analysis

### Why Synapse Blue Cannot Find Roblox:

1. **Process Tree Disconnect**
   - Synapse Blue likely uses `GetChildProcesses()` or `Process.GetProcessById(parentPID)`
   - Roblox launched with `UseShellExecute = true` is NOT a child process
   - Parent process is `explorer.exe` or `shell32.dll`, not Sirstrap

2. **No Process Metadata Sharing**
   - Sirstrap doesn't maintain persistent process handles
   - No IPC (named pipes, shared memory, registry) to communicate PID
   - Third-party tools must independently discover process

3. **PID Volatility**
   - Master instance kills existing Roblox processes on launch
   - PIDs change frequently during singleton transitions
   - Cached PIDs become stale quickly

4. **Timing Race Conditions**
   - Singleton capture/release creates brief windows where no Roblox process exists
   - If Synapse Blue checks during these gaps, detection fails
   - `WaitForInputIdle()` adds unpredictable delay

5. **Working Directory Obfuscation**
   - Roblox launched from version-specific folder (e.g., `%LocalAppData%\Roblox\Versions\version-abc123`)
   - Standard tools may expect launch from `%LocalAppData%\Roblox` or install directory
   - Working directory mismatch might fail heuristic detection

---

## Proposed Solutions

### Solution 1: Process Metadata Export (Recommended - Low Risk)
**Invasiveness:** Low
**Compatibility:** High
**Effectiveness:** High

**Implementation:**
Create a shared metadata mechanism for external tools:

```csharp
// New file: src/Sirstrap.Core/ProcessMetadataExporter.cs
public static class ProcessMetadataExporter
{
    private const string METADATA_FILE = "roblox_process.json";

    public static void ExportProcessInfo(int pid, string exePath)
    {
        string metadataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Sirstrap",
            METADATA_FILE
        );

        var metadata = new
        {
            ProcessId = pid,
            ExecutablePath = exePath,
            LaunchTime = DateTime.UtcNow,
            LaunchedBy = "Sirstrap"
        };

        File.WriteAllText(metadataPath, JsonSerializer.Serialize(metadata));
    }

    public static void ClearProcessInfo()
    {
        string metadataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Sirstrap",
            METADATA_FILE
        );

        if (File.Exists(metadataPath))
            File.Delete(metadataPath);
    }
}
```

**Changes to RobloxLauncher.cs:**
```csharp
Process? robloxPlayerBetaExeProcess = Process.Start(robloxPlayerBetaExeStartInfo);
robloxPlayerBetaExeProcess.WaitForInputIdle();

// NEW: Export process metadata
ProcessMetadataExporter.ExportProcessInfo(
    robloxPlayerBetaExeProcess.Id,
    robloxPlayerBetaExePath
);

// ... existing monitoring logic ...

// In finally block:
ProcessMetadataExporter.ClearProcessInfo();
```

**Benefits:**
- Non-breaking change
- Synapse Blue can read metadata file instead of complex process enumeration
- Works with all third-party tools
- Metadata includes PID, path, timestamp

**Drawbacks:**
- Requires Synapse Blue to update their detection logic
- File I/O introduces minor overhead
- Metadata can be stale if Sirstrap crashes

---

### Solution 2: Configuration Option for UseShellExecute
**Invasiveness:** Medium
**Compatibility:** Medium
**Effectiveness:** High

**Implementation:**
Add `UseShellExecute` as a configurable option:

```ini
# settings.ini
UseShellExecute=true  # Default: true for security, false for compatibility
```

**Changes to RobloxLauncher.cs:**
```csharp
bool useShellExecute = SettingsManager.GetSettings().UseShellExecute;

ProcessStartInfo robloxPlayerBetaExeStartInfo = new()
{
    FileName = robloxPlayerBetaExePath,
    WorkingDirectory = Path.GetDirectoryName(robloxPlayerBetaExePath),
    UseShellExecute = useShellExecute
};
```

**Benefits:**
- Users can opt-in to compatibility mode
- Preserves security for users who don't need third-party tools
- Simple implementation

**Drawbacks:**
- `UseShellExecute = false` reintroduces the shell injection vulnerability from Issue #2
- Users must manually configure
- Bifurcates code paths (testing burden)

---

### Solution 3: Named Event Signaling
**Invasiveness:** Medium
**Compatibility:** High
**Effectiveness:** Medium

**Implementation:**
Use Windows named events to signal process lifecycle:

```csharp
// New file: src/Sirstrap.Core/ProcessEventSignaler.cs
public static class ProcessEventSignaler
{
    private static EventWaitHandle? _processLaunchedEvent;

    public static void SignalProcessLaunched(int pid)
    {
        _processLaunchedEvent = new EventWaitHandle(
            false,
            EventResetMode.ManualReset,
            $"Global\\Sirstrap_Roblox_PID_{pid}"
        );
        _processLaunchedEvent.Set();
    }

    public static void ClearSignal()
    {
        _processLaunchedEvent?.Reset();
        _processLaunchedEvent?.Dispose();
    }
}
```

**Benefits:**
- Real-time signaling (no polling required by Synapse Blue)
- Standard Windows IPC mechanism
- Low overhead

**Drawbacks:**
- Requires Synapse Blue to implement event monitoring
- Security implications (global events visible to all processes)
- Cleanup complexity if Sirstrap crashes

---

### Solution 4: Registry-Based Process Registration
**Invasiveness:** Medium
**Compatibility:** High
**Effectiveness:** Medium

**Implementation:**
Write process information to registry (similar to protocol handler):

```csharp
// In RobloxLauncher.cs after launch
Registry.SetValue(
    @"HKEY_CURRENT_USER\SOFTWARE\Sirstrap",
    "RobloxProcessId",
    robloxPlayerBetaExeProcess.Id,
    RegistryValueKind.DWord
);

Registry.SetValue(
    @"HKEY_CURRENT_USER\SOFTWARE\Sirstrap",
    "RobloxExecutablePath",
    robloxPlayerBetaExePath,
    RegistryValueKind.String
);
```

**Benefits:**
- Persistent storage (survives crashes)
- Standard Windows mechanism
- Easy for third-party tools to query

**Drawbacks:**
- Registry I/O overhead
- Requires cleanup logic
- Admin permissions may be required (depends on key)

---

## Recommended Approach

**Primary Solution:** Implement **Solution 1 (Process Metadata Export)** + **Solution 2 (Configuration Option)**

**Rationale:**
1. **Solution 1** provides immediate compatibility without breaking existing security
2. **Solution 2** gives advanced users control over process model
3. Combined approach balances security, compatibility, and user choice

**Implementation Priority:**
1. **Phase 1:** Implement metadata export (non-breaking, immediate benefit)
2. **Phase 2:** Add `UseShellExecute` configuration (advanced users)
3. **Phase 3:** Document in README for third-party tool developers

---

## Testing Strategy

### Test Cases:
1. **Baseline:** Verify Synapse Blue detection with metadata export
2. **Singleton Transitions:** Test detection during Master/Slave switches
3. **Incognito Mode:** Verify detection with folder isolation enabled
4. **Multi-Instance:** Test detection with multiple Roblox instances
5. **Process Restart:** Test detection after Sirstrap kills/restarts Roblox

### Validation Criteria:
- [ ] Synapse Blue successfully detects Roblox process 100% of the time
- [ ] No regression in Issue #2 (SirHurt injection must still work)
- [ ] No security vulnerabilities introduced
- [ ] Performance impact < 10ms per launch

---

## Documentation Updates Required

1. **README.md:**
   - Document process management architecture
   - Explain compatibility considerations for third-party tools
   - Provide metadata file specification

2. **Settings Documentation:**
   - Document new `UseShellExecute` configuration option
   - Explain security trade-offs

3. **Developer Guide:**
   - Create guide for third-party tool integration
   - Provide sample code for reading metadata

---

## Open Questions

1. **Does Synapse Blue use process enumeration or other detection methods?**
   - Need to investigate Synapse Blue's actual detection logic
   - May require collaboration with Synapse Blue developers

2. **Can we test with Synapse Blue directly?**
   - Need access to Synapse Blue for validation
   - Alternative: Create mock tool that replicates detection behavior

3. **Are there other tools with similar issues?**
   - Should we survey other third-party tools?
   - Create comprehensive compatibility matrix

---

## Next Steps

1. **Immediate:**
   - [ ] Implement Solution 1 (metadata export)
   - [ ] Test with mock detection tool
   - [ ] Update documentation

2. **Short-term:**
   - [ ] Reach out to Synapse Blue developers for collaboration
   - [ ] Implement Solution 2 (configuration option)
   - [ ] Create compatibility guide

3. **Long-term:**
   - [ ] Monitor for similar compatibility reports
   - [ ] Consider standardized IPC protocol for bootstrappers
   - [ ] Build automated compatibility test suite

---

## References

- **Issue #2:** SirHurt injection problem (resolved via PR #3)
- **PR #3:** Introduced `UseShellExecute = true` for security
- **Synapse-S Project:** Referenced in original issue for context
- **Files Modified:** SingletonManager.cs, RobloxLauncher.cs, IncognitoManager.cs

---

## Conclusion

Sirstrap's unique process management architecture—particularly the use of `UseShellExecute = true` and lack of process metadata sharing—creates challenges for third-party tools like Synapse Blue. The recommended solution combines metadata export for immediate compatibility with configuration options for advanced users, balancing security, compatibility, and maintainability.

**Estimated Implementation Time:** 4-6 hours
**Risk Level:** Low (non-breaking changes)
**User Impact:** Positive (enables third-party tool compatibility)
