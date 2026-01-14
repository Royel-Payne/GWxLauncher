# Quick Start Guide

This guide will get you from zero to running the PoC in ~10 minutes.

## Prerequisites Checklist

- [ ] Windows 10/11 (64-bit)
- [ ] Visual Studio 2022 with "Desktop development with C++" workload
- [ ] .NET 8 SDK installed
- [ ] Guild Wars 2 installed (64-bit client: Gw2-64.exe)

## Step-by-Step Setup

### 1. Download MinHook Library

**This is the only external dependency you need to manually obtain.**

1. Open your web browser and go to:  
   **https://github.com/TsudaKageyu/minhook/releases**

2. Download the latest release ZIP file  
   (e.g., `minhook_1_3_3.zip`)

3. Extract the ZIP file

4. Copy **ONE FILE** from the extracted folder to your project:
   ```
   FROM: minhook_1_3_3\lib\libMinHook.x64.lib
   TO:   Experiments\Gw2AppDataRedirectPoC\Gw2FolderHook\MinHook\lib\libMinHook.x64.lib
   ```

5. Verify the file is in the right place:
   ```batch
   dir Experiments\Gw2AppDataRedirectPoC\Gw2FolderHook\MinHook\lib\libMinHook.x64.lib
   ```

### 2. Build the PoC

Open a command prompt or PowerShell in the project root, then:

```batch
cd Experiments\Gw2AppDataRedirectPoC
Build.bat
```

You should see:
```
================================================================
  BUILD SUCCESSFUL!
================================================================

Outputs are in: ...\Build
```

**Troubleshooting Build Issues:**

- **"MSBuild not found"**: Open "Developer Command Prompt for VS 2022" instead
- **"MinHook library not found"**: Re-check Step 1 above
- **Linker errors**: Ensure you downloaded the x64 version of MinHook

### 3. Run the First Test

**Option A: Use the quick test script**

```batch
Test.bat
```

This automatically uses `C:\Program Files\Guild Wars 2\Gw2-64.exe` and creates a test profile in `C:\Temp`.

**Option B: Manual test with custom paths**

```batch
cd Build
Gw2AppDataRedirectPoC.exe "C:\Path\To\Gw2-64.exe" "C:\Temp\GW2Test\Profile1"
```

### 4. Verify It's Working

After GW2 launches:

1. **Check the injection log** (wait a few seconds for it to be created):
   ```batch
   type C:\Temp\Gw2FolderHook.log
   ```
   
   You should see lines like:
   ```
   [2024-12-XX 10:30:45] Gw2FolderHook v0.1.0 - Initializing
   [2024-12-XX 10:30:45] Config: ROAMING -> C:\Temp\GW2Test\Profile1\Roaming
   [2024-12-XX 10:30:45] All hooks enabled successfully
   ```

2. **Log in to GW2**, change a setting (e.g., graphics quality), and **exit GW2 completely**

3. **Check for redirected files**:
   ```batch
   dir "C:\Temp\GW2Test\Profile1\Roaming\Guild Wars 2"
   dir "C:\Temp\GW2Test\Profile1\Local\ArenaNet\Guild Wars 2"
   ```
   
   You should see files like:
   - `Local.dat`
   - `GFXSettings.*.xml`
   - `Settings.json`

4. **Confirm files are NOT in your real AppData**:
   ```batch
   dir "%APPDATA%\Guild Wars 2"
   ```
   
   This should either not exist or be empty (assuming you had no previous GW2 installation).

### 5. Test Multiple Profiles (Optional)

To really prove isolation works:

```batch
cd Build

REM Launch Profile 1
Gw2AppDataRedirectPoC.exe "C:\Program Files\Guild Wars 2\Gw2-64.exe" "C:\Temp\GW2Test\Profile1"

REM Wait for it to load, then launch Profile 2
Gw2AppDataRedirectPoC.exe "C:\Program Files\Guild Wars 2\Gw2-64.exe" "C:\Temp\GW2Test\Profile2"
```

Set **different** graphics presets in each instance, exit both, and verify:

```batch
type "C:\Temp\GW2Test\Profile1\Roaming\Guild Wars 2\GFXSettings*.xml"
type "C:\Temp\GW2Test\Profile2\Roaming\Guild Wars 2\GFXSettings*.xml"
```

They should have different contents!

## What Success Looks Like

? **Injection succeeds**: Console shows "? SUCCESS!"  
? **Hook log exists**: `C:\Temp\Gw2FolderHook.log` is created  
? **Hook intercepts calls**: Log shows SHGetKnownFolderPath calls  
? **Files redirected**: Settings appear in custom profile folder, not real AppData  
? **Multiple profiles isolated**: Each profile maintains separate settings  
? **No crashes**: GW2 runs normally  
? **No UAC prompts**: Everything runs without admin rights  

## Common Issues

### "Failed to inject DLL"

- Ensure both DLL and EXE are x64 architecture
- Check antivirus isn't blocking the injection
- Try running from a Developer Command Prompt

### "Hook log not created"

- DLL failed to initialize
- Check for error log: `C:\Temp\Gw2FolderHook_error.log`
- Verify environment variables are set (Program.cs does this automatically)

### "Files still in real AppData"

- GW2 may have cached the path before injection (shouldn't happen with suspended launch)
- Check hook log to see if calls were intercepted
- Verify you're checking the right AppData folders

### "GW2 crashes immediately"

- Check error log for exceptions
- Ensure MinHook library is correct version (x64)
- Try Debug build of DLL for more verbose logging

### "LoadLibrary returned NULL"

- DLL dependencies missing (unlikely - we only use system DLLs)
- DLL is 32-bit instead of 64-bit
- Verify Gw2FolderHook.dll is in same folder as EXE

## Next Steps

If the PoC works:

1. **Document your findings**: What worked, what didn't, any crashes or issues?
2. **Test edge cases**: Multiple instances, account switching, GW2 updates
3. **Consider anti-cheat**: GW2 doesn't have invasive anti-cheat, but monitor for issues
4. **Review integration plan**: See main README.md for how to integrate into launcher

If the PoC fails:

1. **Document why**: Exact error messages, logs, crash dumps
2. **Consider alternatives**: Registry virtualization, symlinks, separate user accounts
3. **Consult the community**: GW2 multi-launch tools might have insights

## Getting Help

If you're stuck:

1. Check `C:\Temp\Gw2FolderHook.log` and `C:\Temp\Gw2FolderHook_error.log`
2. Run with Visual Studio debugger attached to see where it fails
3. Review the main README.md for more detailed troubleshooting

---

**Good luck with your PoC!** ??
