# GW2 AppData Redirection Proof of Concept

## Overview

This PoC tests whether we can redirect Guild Wars 2's AppData folder access on a per-process basis using DLL injection and API hooking. This would allow multiple GW2 profiles to each have their own settings without requiring system-wide changes, admin rights, or separate Windows user accounts.

## The Problem

GW2 stores its settings in:
- `%APPDATA%\Roaming\Guild Wars 2\Local.dat` (account data)
- `%APPDATA%\Roaming\Guild Wars 2\GFXSettings.<hash>.xml` (graphics settings)
- `%APPDATA%\Local\ArenaNet\Guild Wars 2\Settings.json` (game settings)

All GW2 instances share these files, making true profile isolation impossible with standard approaches.

## The Solution (Hypothesis)

1. Launch GW2 in suspended mode
2. Inject a custom DLL before GW2 initializes
3. Hook Windows API calls that resolve AppData paths
4. Return custom paths for that specific process only
5. Resume GW2 - it should now use the redirected folders

## Project Structure

```
Experiments/Gw2AppDataRedirectPoC/
??? README.md                          (this file)
??? Gw2AppDataRedirectPoC/             (C# console app - injector)
?   ??? Gw2AppDataRedirectPoC.csproj
?   ??? Program.cs
?   ??? ProcessInjector.cs
?   ??? NativeMethods.cs
??? Gw2FolderHook/                     (C++ DLL - hooks)
?   ??? Gw2FolderHook.cpp
?   ??? Gw2FolderHook.vcxproj
?   ??? MinHook/                       (header-only or prebuilt)
?   ??? dllmain.cpp
??? Build/                             (build outputs)
?   ??? Gw2AppDataRedirectPoC.exe
?   ??? Gw2FolderHook.dll
??? TestResults/                       (created during testing)
    ??? Profile1/
    ?   ??? Roaming/
    ?   ??? Local/
    ??? Profile2/
        ??? Roaming/
        ??? Local/
```

## Prerequisites

- Visual Studio 2022 with C++ workload (Desktop development with C++)
- .NET 8 SDK
- Guild Wars 2 installed (64-bit version: Gw2-64.exe)
- MinHook library (included as source or nuget)

## Build Instructions

### Option 1: Using Visual Studio

1. Open `Experiments/Gw2AppDataRedirectPoC/Gw2AppDataRedirectPoC.sln` in Visual Studio
2. Set solution configuration to **Release** and platform to **x64**
3. Build the entire solution (this builds both the C# app and C++ DLL)
4. Outputs will be in `Experiments/Gw2AppDataRedirectPoC/Build/`

### Option 2: Using Command Line

```batch
# From Experiments/Gw2AppDataRedirectPoC/ directory

# Build the C++ DLL first
cd Gw2FolderHook
msbuild Gw2FolderHook.vcxproj /p:Configuration=Release /p:Platform=x64
cd ..

# Build the C# injector
dotnet build Gw2AppDataRedirectPoC/Gw2AppDataRedirectPoC.csproj -c Release -r win-x64

# Copy outputs to Build folder
xcopy /Y Gw2FolderHook\x64\Release\Gw2FolderHook.dll Build\
xcopy /Y Gw2AppDataRedirectPoC\bin\Release\net8.0\win-x64\*.exe Build\
```

## Testing the PoC

### Test 1: Single Profile Redirection

1. Create a test folder:
   ```
   mkdir C:\Temp\GW2Test\Profile1
   ```

2. Run the injector:
   ```
   cd Experiments\Gw2AppDataRedirectPoC\Build
   Gw2AppDataRedirectPoC.exe "C:\Program Files\Guild Wars 2\Gw2-64.exe" "C:\Temp\GW2Test\Profile1"
   ```

3. GW2 should launch normally. Check the injection log:
   ```
   type C:\Temp\Gw2FolderHook.log
   ```

4. Log in to GW2, change a graphics setting, and exit

5. Verify files were created in redirected location:
   ```
   dir "C:\Temp\GW2Test\Profile1\Roaming\Guild Wars 2"
   dir "C:\Temp\GW2Test\Profile1\Local\ArenaNet\Guild Wars 2"
   ```

6. Verify files were NOT created in your real AppData:
   ```
   dir "%APPDATA%\Roaming\Guild Wars 2"
   dir "%LOCALAPPDATA%\ArenaNet\Guild Wars 2"
   ```

### Test 2: Multiple Isolated Profiles

1. Create a second profile folder:
   ```
   mkdir C:\Temp\GW2Test\Profile2
   ```

2. Launch two instances with different redirections:
   ```
   # Terminal 1
   Gw2AppDataRedirectPoC.exe "C:\Program Files\Guild Wars 2\Gw2-64.exe" "C:\Temp\GW2Test\Profile1"
   
   # Terminal 2 (after first instance loads)
   Gw2AppDataRedirectPoC.exe "C:\Program Files\Guild Wars 2\Gw2-64.exe" "C:\Temp\GW2Test\Profile2"
   ```

3. In each GW2 instance, set DIFFERENT graphics presets (e.g., "Best Performance" vs "Best Appearance")

4. Exit both instances

5. Check that each has its own settings:
   ```
   type "C:\Temp\GW2Test\Profile1\Roaming\Guild Wars 2\GFXSettings*.xml"
   type "C:\Temp\GW2Test\Profile2\Roaming\Guild Wars 2\GFXSettings*.xml"
   ```

6. Re-launch each profile - verify settings persist correctly per profile

## Expected Results

### Success Indicators

? `C:\Temp\Gw2FolderHook.log` shows hook was installed  
? Log shows GW2 requested `FOLDERID_RoamingAppData` and `FOLDERID_LocalAppData`  
? Log shows our DLL returned the custom redirected paths  
? `Local.dat` and other files appear in redirected folders, not real AppData  
? Each profile maintains separate settings across launches  
? No UAC prompts or admin requirements  

### Failure Indicators

? Gw2FolderHook.log not created ? DLL not injected or hook failed  
? Files still appear in real AppData ? Hook not intercepting correctly  
? GW2 crashes on launch ? Hook broke something critical  
? Settings mix between profiles ? Isolation not working  

## Technical Details

### API Hooking Strategy

We hook these Windows Shell API functions:
- `SHGetKnownFolderPath` (modern, Vista+)
- `SHGetFolderPathW` (legacy fallback)

When GW2 requests:
- `FOLDERID_RoamingAppData` ? return `{profileRoot}\Roaming`
- `FOLDERID_LocalAppData` ? return `{profileRoot}\Local`
- Any other folder ? pass through to original function

### Injection Timing

Critical: The hook MUST be active before GW2 reads the registry for folder paths. We achieve this by:
1. CreateProcess with `CREATE_SUSPENDED` flag
2. Inject DLL while process is suspended
3. DLL hooks APIs during `DllMain(PROCESS_ATTACH)`
4. Resume main thread

### Memory Management

`SHGetKnownFolderPath` returns memory allocated with `CoTaskMemAlloc`. Our hook must:
1. Allocate return strings with `CoTaskMemAlloc` (caller frees with `CoTaskMemFree`)
2. Not leak memory for passed-through calls

### Thread Safety

- Hooks are installed once during DLL_PROCESS_ATTACH
- No global state modifications after initialization
- Original function pointers are read-only after setup

## Troubleshooting

### "Failed to inject DLL"
- Ensure DLL and EXE are both x64
- Ensure GW2 path is correct
- Check antivirus isn't blocking injection
- Run VS as admin and rebuild (for PDB generation issues)

### "Hook log not created"
- DLL didn't load - check `C:\Temp\Gw2FolderHook_error.log`
- Hook initialization failed - check error log for details

### "Files still in real AppData"
- GW2 may cache the path before our hook activates (rare)
- Check if log shows our hook was called
- Try deleting `%APPDATA%\Roaming\Guild Wars 2` and re-test

### "GW2 crashes immediately"
- Hook broke a critical API call
- Check error log for exceptions
- Ensure MinHook is properly linked/initialized

## Integration Plan (If Successful)

If this PoC works, integration into main launcher:

1. **Copy working code**:
   - Move `ProcessInjector.cs` and `NativeMethods.cs` to `Services/Injection/`
   - Add DLL as embedded resource or deploy alongside EXE

2. **Modify Gw2LaunchOrchestrator**:
   - Add injection step before launching GW2
   - Pass profile-specific folder paths based on `GameProfile.Id`

3. **Update LauncherConfig**:
   - Add `ProfileIsolation` setting
   - Add `ProfileStorageRoot` path setting

4. **UI Changes**:
   - Add checkbox: "Enable profile isolation (experimental)"
   - Show each profile's AppData location
   - Add "Open Profile Folder" button

5. **Testing**:
   - Add unit tests for injection logic
   - Add integration tests for multi-instance scenarios

## Removal Plan (If Failed)

If this PoC doesn't work:

1. Delete entire `Experiments/Gw2AppDataRedirectPoC/` folder
2. Remove any commits from this branch (or abandon branch)
3. Document why it failed in main project docs
4. Consider alternative approaches (symlinks, registry virtualization, etc.)

## Known Limitations

- **Only works for GW2**: Each game has different API usage
- **Anti-cheat risk**: Some games might detect hooking as cheating (GW2 appears OK)
- **Maintenance burden**: Windows updates could break hooks
- **32-bit support**: Would need separate 32-bit DLL for GW1
- **Not foolproof**: Some edge case APIs might bypass our hooks

## References

- [MinHook - x64/x86 API Hooking Library](https://github.com/TsudaKageyu/minhook)
- [CreateRemoteThread Injection Tutorial](https://www.codeproject.com/Articles/20084/A-More-Complete-DLL-Injection-Solution-Using-Crea)
- [SHGetKnownFolderPath Documentation](https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shgetknownfolderpath)

## License

This experimental code is part of GWxLauncher and follows the same license as the parent project.

---

**Last Updated**: 2024-12-XX  
**Status**: PoC in Development  
**Primary Developer**: Your Name
