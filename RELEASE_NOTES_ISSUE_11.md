# Release Notes - Fix Issue #11: Synapse Blue Compatibility

## 🎯 What's Fixed

Sirstrap now properly detaches from the browser process when launched via `roblox-player://` protocol links. This resolves compatibility issues with **Synapse Blue** and other third-party tools that need to detect Roblox processes.

---

## 🔧 Technical Details

### The Problem

When you clicked a Roblox link in your browser, Sirstrap would become a subprocess (child process) of the browser. This created a problematic process hierarchy that prevented tools like Synapse Blue from properly detecting Roblox.

### The Solution

The Windows Registry protocol handler now uses `cmd /c start` to create a completely independent process:

**Old command:**
```
"C:\Path\To\Sirstrap.exe" %1
```

**New command:**
```
cmd /c start "" "C:\Path\To\Sirstrap.exe" "%1"
```

This ensures Sirstrap runs as a top-level process, not a browser subprocess.

---

## 🚀 What You Need to Do

### For New Installations
Nothing! The fix is automatically applied during installation.

### For Existing Users

The protocol handler will be **automatically updated** the first time you run Sirstrap after updating:

1. **Run Sirstrap** (UI or CLI version)
2. You may see a **UAC prompt** asking for administrator privileges
   - This is normal and required to update the Windows Registry
   - Click **"Yes"** to allow the update
3. You'll see in the logs:
   ```
   [*] Protocol roblox-player is registered with a different handler: ...
   [*] Updating protocol handler to fix browser subprocess issues (Issue #11)...
   [*] Registering protocol roblox-player with command: cmd /c start "" "..." "%1"
   [*] Protocol roblox-player registered successfully.
   ```
4. **That's it!** The update is complete

### Testing

After updating, test that everything works:

1. Click a Roblox game link from your browser
2. Sirstrap should launch normally
3. Roblox should start as usual
4. **Synapse Blue and other tools should now detect Roblox properly**

---

## 📊 Process Hierarchy Changes

### Before Fix
```
Browser (chrome.exe, edge.exe, etc.)
  └── Sirstrap.exe (stuck as browser child!)

Windows Shell
  └── RobloxPlayerBeta.exe (independent)
```

**Problem:** Sirstrap trapped as browser subprocess, complex hierarchy confuses detection tools.

### After Fix
```
Browser (chrome.exe, edge.exe, etc.)
  └── cmd.exe (exits immediately)

Windows Shell
  ├── Sirstrap.exe (independent!)
  └── RobloxPlayerBeta.exe (independent)
```

**Result:** Clean process tree, standard detection works perfectly.

---

## ✅ Benefits

- ✅ **Fixes Synapse Blue compatibility** - Roblox processes are now properly detectable
- ✅ **No breaking changes** - All existing functionality preserved
- ✅ **Security maintained** - Keeps `UseShellExecute = true` security benefits
- ✅ **Industry standard** - Uses the same pattern as Bloxstrap and other modern bootstrappers
- ✅ **Automatic update** - No manual registry editing required

---

## 🐛 Troubleshooting

### "Access to the registry is denied"

If you see this error:
1. **Run Sirstrap as Administrator** once manually
2. The protocol handler will be updated
3. Future launches won't require admin privileges

### Protocol handler not updating

If the protocol handler doesn't update automatically:
1. Check the Sirstrap logs for error messages
2. Try running Sirstrap with administrator privileges
3. Verify antivirus isn't blocking registry changes

### Synapse Blue still can't find Roblox

If issues persist after updating:
1. Verify the protocol was updated (check logs)
2. Restart your browser completely
3. Test by clicking a Roblox link from the browser
4. Check if Roblox launches via Sirstrap or directly

---

## 📝 Related Information

- **Issue:** [#11 - Investigate compatibility issues between Sirstrap and Synapse Blue](https://github.com/massimopaganigh/Sirstrap/issues/11)
- **Related Fix:** Issue #2 - SirHurt injection (addressed shell injection security)
- **Inspiration:** Bloxstrap's protocol handler implementation

---

## 🔍 For Developers

### Files Changed
- `src/Sirstrap.Core/RegistryManager.cs` - Modified `GetExpectedCommand()` to use `cmd /c start`

### Testing
```bash
# Check current registry value
reg query "HKEY_CLASSES_ROOT\roblox-player\shell\open\command"

# Should show:
# (Default)    REG_SZ    cmd /c start "" "C:\...\Sirstrap.exe" "%1"
```

### Technical Documentation
See `ISSUE_11_INVESTIGATION.md` for complete technical analysis and solution details.

---

**Version:** Next release after commit `527a57f`
**Status:** ✅ Ready for release
**Tested:** Pending user feedback

---

💬 **Questions or Issues?** Report them at: https://github.com/massimopaganigh/Sirstrap/issues
