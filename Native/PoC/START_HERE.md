# ?? Start Here - Visual Checklist

## What You Have Now

```
? Complete C# injector application (compiles to .exe)
? Complete C++ hook DLL (compiles to .dll)
? Build automation scripts
? Comprehensive documentation
? Testing scripts and procedures
? Integration and removal plans
```

## ?? Your Action Items

### ? 1. Download MinHook (5 minutes)

**Why?** This is the only external library needed. It's a well-known, safe API hooking library.

**Where?** https://github.com/TsudaKageyu/minhook/releases

**What to do:**
1. Download the latest release ZIP (e.g., `minhook_1_3_3.zip`)
2. Extract it
3. Find the file: `lib\libMinHook.x64.lib`
4. Copy it to: `Experiments\Gw2AppDataRedirectPoC\Gw2FolderHook\MinHook\lib\`

**Verify:**
```batch
dir Experiments\Gw2AppDataRedirectPoC\Gw2FolderHook\MinHook\lib\libMinHook.x64.lib
```
You should see the file!

---

### ? 2. Build Everything (2 minutes)

**Option A - Using Batch (CMD/PowerShell):**
```batch
cd Experiments\Gw2AppDataRedirectPoC
Build.bat
```

**Option B - Using PowerShell (Recommended if batch fails):**
```powershell
cd Experiments\Gw2AppDataRedirectPoC
.\Build.ps1
```

**Expected output:**
```
================================================================
  BUILD SUCCESSFUL!
================================================================
```

**Troubleshooting:**
- "MSBuild not found" ? Try the PowerShell version (`Build.ps1`) OR open "Developer Command Prompt for VS 2022"
- "MinHook library not found" ? Complete step 1 first
- Other errors ? Check `QUICKSTART.md` troubleshooting section

---

### ? 3. Run First Test (1 minute setup + GW2 launch time)

**Option A - Quick test with Batch:**
```batch
Test.bat
```

**Option A2 - Quick test with PowerShell:**
```powershell
.\Test.ps1
```

**Option B - Custom paths:**
```batch
cd Build
Gw2AppDataRedirectPoC.exe "C:\Path\To\Gw2-64.exe" "C:\Temp\GW2Test\Profile1"
```

**Watch the console output** - it will tell you exactly what's happening!

---

### ? 4. Verify Success (2 minutes)

After GW2 launches and you log in:

**A. Check injection log:**
```batch
type C:\Temp\Gw2FolderHook.log
```
Should show hook initialization and API calls.

**B. Change a setting in GW2:**
- Go to Options ? Graphics
- Change the preset (e.g., "Best Appearance" ? "Best Performance")
- Exit GW2 completely

**C. Check for redirected files:**
```batch
dir "C:\Temp\GW2Test\Profile1\Roaming\Guild Wars 2"
```
Should show `Local.dat`, `GFXSettings*.xml`, etc.

**D. Verify real AppData is untouched:**
```batch
dir "%APPDATA%\Guild Wars 2"
```
Should be empty or unchanged.

---

### ? 5. Test Multiple Profiles (Optional, 5 minutes)

Launch two profiles with different settings:

```batch
cd Build

REM Terminal 1
Gw2AppDataRedirectPoC.exe "C:\Program Files\Guild Wars 2\Gw2-64.exe" "C:\Temp\GW2Test\Profile1"

REM Terminal 2 (after first loads)
Gw2AppDataRedirectPoC.exe "C:\Program Files\Guild Wars 2\Gw2-64.exe" "C:\Temp\GW2Test\Profile2"
```

Set different graphics presets in each, exit both, then verify:

```batch
type "C:\Temp\GW2Test\Profile1\Roaming\Guild Wars 2\GFXSettings*.xml"
type "C:\Temp\GW2Test\Profile2\Roaming\Guild Wars 2\GFXSettings*.xml"
```

Should be different!

---

### ? 6. Decide Next Steps

**If it works:**
? See `README.md` "Integration Plan" section
? This is the hard part - you'll need to integrate into your launcher

**If it fails:**
? See `README.md` "Troubleshooting" section
? Check logs in `C:\Temp\`
? Consider alternative approaches documented in README
? Can safely delete entire `Experiments/Gw2AppDataRedirectPoC/` folder

---

## ?? Documentation Guide

**Start here first:**
- `QUICKSTART.md` - 10-minute setup guide (you are here!)
- `IMPLEMENTATION_COMPLETE.md` - What was built and why

**Detailed reference:**
- `README.md` - Complete documentation (testing, integration, troubleshooting)
- `Gw2FolderHook/MinHook/README.md` - MinHook setup details

**For development:**
- `Program.cs` - Well-commented C# code
- `ProcessInjector.cs` - Injection logic
- `Gw2FolderHook.cpp` - Hook implementation

---

## ?? Visual Structure

```
Experiments/Gw2AppDataRedirectPoC/
?
??? ?? START_HERE.md ................... (you are here!)
??? ?? IMPLEMENTATION_COMPLETE.md ....... (overview of what was created)
??? ?? QUICKSTART.md .................... (detailed setup guide)
??? ?? README.md ........................ (comprehensive documentation)
?
??? ?? Build.bat ........................ (automated build)
??? ?? Test.bat ......................... (quick test)
?
??? ?? Gw2AppDataRedirectPoC/ .......... (C# injector)
?   ??? Program.cs ...................... (entry point)
?   ??? ProcessInjector.cs .............. (injection logic)
?   ??? NativeMethods.cs ................ (Windows API)
?
??? ?? Gw2FolderHook/ .................. (C++ hook DLL)
?   ??? Gw2FolderHook.cpp ............... (hook implementation)
?   ??? MinHook/
?       ??? lib/
?           ??? ?? libMinHook.x64.lib ... (YOU MUST DOWNLOAD THIS)
?
??? ?? Build/ .......................... (created after building)
    ??? Gw2AppDataRedirectPoC.exe ....... (injector)
    ??? Gw2FolderHook.dll ............... (hook DLL)
```

---

## ?? Common Mistakes to Avoid

? **Don't skip downloading MinHook** - the build will fail without it

? **Don't run on 32-bit GW2** - this PoC is x64 only (Gw2-64.exe)

? **Don't test without reading logs** - the logs tell you what went wrong

? **Don't commit build outputs** - they're in .gitignore for a reason

? **Don't modify main GWxLauncher code yet** - test the PoC first!

---

## ? Success Criteria

You'll know it's working when:

? Console shows "? SUCCESS!" after injection
? `C:\Temp\Gw2FolderHook.log` exists and shows hook calls
? GW2 settings files appear in custom profile folder
? Real AppData is untouched
? Multiple instances have isolated settings
? No UAC prompts or admin requirements
? GW2 runs normally without crashes

---

## ?? Getting Stuck?

1. **Read the logs**: `C:\Temp\Gw2FolderHook.log` and `*_error.log`
2. **Check QUICKSTART.md**: Has troubleshooting section
3. **Review README.md**: Comprehensive troubleshooting
4. **Examine the code**: It's well-commented
5. **Test incrementally**: Don't skip verification steps

---

## ?? Ready to Start?

**Your first command:**

```batch
cd Experiments\Gw2AppDataRedirectPoC
```

**Then follow the checklist above!**

Good luck! You've got this. ??

---

**Tips:**
- Take it step by step
- Read the console output - it guides you
- Don't panic if it doesn't work first try - troubleshooting is part of PoC work
- Document your findings (add notes to README.md)
- This branch is for experimentation - it's safe to try things!
