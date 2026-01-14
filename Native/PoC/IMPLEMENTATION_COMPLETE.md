# PoC Implementation Complete - Summary

## What Was Created

A complete, isolated proof-of-concept for GW2 AppData redirection via DLL injection. The structure keeps all experimental code separate from your main launcher application.

### Folder Structure

```
GWxLauncher/                           (your main project - UNCHANGED)
?
??? Experiments/                        (NEW - isolated experimental code)
    ??? README.md                       (explains experiments folder purpose)
    ?
    ??? Gw2AppDataRedirectPoC/         (this PoC)
        ??? README.md                   (comprehensive documentation)
        ??? QUICKSTART.md               (10-minute setup guide)
        ??? Build.bat                   (automated build script)
        ??? Test.bat                    (quick test script)
        ??? .gitignore                  (excludes build artifacts)
        ?
        ??? Gw2AppDataRedirectPoC/     (C# injector console app)
        ?   ??? Gw2AppDataRedirectPoC.csproj
        ?   ??? Program.cs              (entry point, user-friendly output)
        ?   ??? ProcessInjector.cs      (injection logic)
        ?   ??? NativeMethods.cs        (P/Invoke definitions)
        ?
        ??? Gw2FolderHook/             (C++ hook DLL)
        ?   ??? Gw2FolderHook.vcxproj   (VS2022 C++ project)
        ?   ??? Gw2FolderHook.cpp       (API hooking implementation)
        ?   ??? MinHook/
        ?       ??? README.md           (download instructions)
        ?       ??? include/
        ?           ??? MinHook.h       (header file)
        ?       ??? lib/
        ?           ??? (you need to download libMinHook.x64.lib)
        ?
        ??? Build/                      (created during build)
            ??? Gw2AppDataRedirectPoC.exe
            ??? Gw2FolderHook.dll
```

## How This Stays Clean

### ? Isolated from Main Project

- Lives entirely in `Experiments/` folder
- Uses separate project files
- Doesn't reference or modify main launcher code
- Can be deleted without affecting GWxLauncher

### ? Documented for Both Outcomes

**If PoC Succeeds:**
- Integration plan in main README.md shows how to move code into Services/
- Clear steps for UI integration
- Testing checklist

**If PoC Fails:**
- Easy to delete entire Experiments/Gw2AppDataRedirectPoC/ folder
- Git history preserved (you can review what was tried)
- Failure reasons documented for future reference

### ? Self-Contained Testing

- Doesn't require modifying GWxLauncher
- Runs independently via command line
- Clear success/failure indicators
- No risk of breaking your production code

## Your Next Steps

### Step 1: Download MinHook (Required)

This is the **only** external dependency:

1. Visit: https://github.com/TsudaKageyu/minhook/releases
2. Download latest release ZIP
3. Extract and copy `lib\libMinHook.x64.lib` to:
   ```
   Experiments/Gw2AppDataRedirectPoC/Gw2FolderHook/MinHook/lib/libMinHook.x64.lib
   ```

### Step 2: Build

```batch
cd Experiments\Gw2AppDataRedirectPoC
Build.bat
```

### Step 3: Test

```batch
Test.bat
```

Or manually:

```batch
cd Build
Gw2AppDataRedirectPoC.exe "C:\Program Files\Guild Wars 2\Gw2-64.exe" "C:\Temp\GW2Test\Profile1"
```

### Step 4: Verify Results

Check these files to confirm it worked:

1. **Injection log**: `C:\Temp\Gw2FolderHook.log`
2. **Redirected files**: `C:\Temp\GW2Test\Profile1\Roaming\Guild Wars 2\Local.dat`
3. **Real AppData**: Should be empty/unchanged

See `QUICKSTART.md` for detailed testing instructions.

### Step 5: Decide Path Forward

**If successful:**

Follow integration plan in `README.md` to:
- Move `ProcessInjector.cs` to `Services/Injection/`
- Update `Gw2LaunchOrchestrator` to call injection
- Add UI for profile isolation settings
- Deploy DLL with launcher

**If unsuccessful:**

1. Document what failed in README.md
2. Consider alternatives:
   - Registry virtualization
   - Symbolic links
   - Separate Windows user accounts
   - App-V or similar containerization
3. Delete this folder or keep for reference

## Important Notes

### Git Branching Strategy

You mentioned concern about "contaminating" the branch. Here's what I recommend:

**Current branch: `research-profile-isolation`**

This is perfect for this work! The PoC is isolated, so:

- ? You CAN safely commit this to `research-profile-isolation`
- ? Main code is untouched, so no contamination risk
- ? Easy to merge or discard based on results

**If you want even more isolation:**

```batch
# Create a sub-branch for just this PoC
git checkout -b research-profile-isolation-poc

# Do your testing and commits here
git add Experiments/
git commit -m "Add GW2 AppData redirection PoC structure"

# If successful, merge back to research-profile-isolation
git checkout research-profile-isolation
git merge research-profile-isolation-poc

# If unsuccessful, just delete the branch
git branch -D research-profile-isolation-poc
```

### What's Gitignored

The `.gitignore` in the PoC folder excludes:
- Build outputs (`Build/`, `bin/`, `obj/`)
- Visual Studio temp files (`.vs/`, `*.user`)
- Test logs (`*.log`)
- MinHook library (user must download)

So you can safely commit the source code without bloating the repo.

## Technical Highlights

### Why This Approach?

1. **No Admin Required**: Uses user-mode injection only
2. **Process-Specific**: Only affects the injected GW2 instance
3. **Early Injection**: Suspended process ensures hooks active before GW2 initializes
4. **Standard APIs**: Uses well-understood CreateRemoteThread technique
5. **Safe Hooking**: MinHook is mature, widely-used library
6. **Comprehensive Logging**: Easy to diagnose what went wrong

### How It Works

```
1. Launch GW2 suspended
   ??> Process created but main thread not running

2. Inject Gw2FolderHook.dll
   ??> DLL loaded into GW2's address space
   ??> DllMain runs, installs API hooks

3. Resume main thread
   ??> GW2 starts normal initialization
   ??> Calls SHGetKnownFolderPath for AppData
   ??> Our hook intercepts and returns custom path
   ??> GW2 uses redirected folder

4. Result
   ??> Settings written to profile-specific folder
   ??> Multiple instances can have separate settings
```

### Limitations & Risks

**Known Limitations:**
- Only works for GW2 (game-specific testing needed)
- Requires 64-bit GW2 (separate 32-bit DLL needed for GW1)
- May not catch ALL filesystem operations (e.g., direct CreateFile calls)
- Anti-cheat detection risk (low for GW2, but possible)

**Testing Needed:**
- Multiple instances simultaneously
- Account switching within game
- Game updates and patching
- Long-term stability
- Anti-cheat compatibility

## Questions?

Refer to these documents:

- **Quick Setup**: `QUICKSTART.md` (start here!)
- **Detailed Info**: `README.md` (comprehensive guide)
- **Troubleshooting**: `README.md` has troubleshooting section
- **MinHook Setup**: `Gw2FolderHook/MinHook/README.md`

## Summary

You now have a clean, isolated, well-documented PoC environment that:

? Doesn't contaminate your main launcher code  
? Has clear success/failure criteria  
? Is easy to test and verify  
? Can be cleanly integrated or removed  
? Is properly structured for Git  

**The only thing left to do is download MinHook and run the build!**

Good luck! ??
