# Native Components

This directory contains **production native code** required for GWxLauncher features.

## Contents

### `/Gw2FolderHook` - GW2 AppData Redirection Hook (C++)
**Status:** Production  
**Purpose:** DLL injection hook that redirects GW2's AppData folder paths to enable per-profile isolation.

This is the core native component that powers the GW2 Profile Isolation feature. It uses MinHook to intercept Windows API calls (`SHGetFolderPathW`, `SHGetKnownFolderPath`) and redirect them to profile-specific directories.

**Technology:**
- C++ x64 DLL
- MinHook library for API hooking
- Injected via GWxInjector.exe helper

### `/minhook` - MinHook Library
Dependency for `Gw2FolderHook`. MinHook is a minimalistic x86/x64 API hooking library.

### `/Build` - Build Output
Contains compiled `Gw2FolderHook.dll` and related build artifacts.

### `/PoC` - Proof of Concept Archive
Contains the original C# test harness used during development. Kept for historical reference and debugging.

---

## Building the Hook DLL

### Prerequisites
- Visual Studio 2022 or MSBuild Tools
- Windows SDK

### Build Steps

**Option 1: PowerShell Script (Recommended)**
```powershell
.\Build.ps1
```

**Option 2: Batch File**
```batch
.\Build.bat
```

**Option 3: Manual**
```powershell
# Build MinHook first
.\BuildMinHook.ps1

# Then build the hook DLL
msbuild Gw2FolderHook\Gw2FolderHook.vcxproj /p:Configuration=Release /p:Platform=x64
```

The compiled DLL will be placed in `Build\Gw2FolderHook.dll` and automatically copied to the launcher's output directory during build (configured in `GWxLauncher.csproj`).

---

## Integration with GWxLauncher

The hook DLL is copied to the launcher's bin directory during build via the `CopyGw2IsolationHookDll` MSBuild target in `GWxLauncher.csproj`.

**Used by:**
- `Services\Gw2IsolationService.cs` - Handles DLL injection
- `Helpers\GWxInjector\` - x64 helper process for injection (launcher is x86)

**Launch Flow:**
1. User enables GW2 Isolation in settings
2. `Gw2IsolationService` calls `GWxInjector.exe` (x64 helper)
3. GWxInjector injects `Gw2FolderHook.dll` into GW2 process
4. Hook DLL intercepts AppData API calls and redirects to profile-specific folder

---

## Troubleshooting

**"Gw2FolderHook.dll not found" warning during build:**
- Run `.\Build.ps1` in this directory first
- The DLL must exist before building GWxLauncher

**Hook not working at runtime:**
- Ensure the DLL is in the same directory as `GWxLauncher.exe`
- Check that `GWxInjector.exe` exists (built from `Helpers\GWxInjector\`)
- Verify GW2 is running as x64 process

**Rebuild after Windows/VS updates:**
```powershell
# Clean rebuild
Remove-Item -Recurse -Force Build, Gw2FolderHook\x64, minhook\build\VC17\Release
.\Build.ps1
```

---

## Architecture Notes

**Why a native DLL?**  
GW2 is a native x64 application. To intercept its Windows API calls, we need native code at the same architecture level. C# P/Invoke cannot hook another process's API calls.

**Why a separate injector process?**  
GWxLauncher is compiled as x86 (for compatibility). To inject a x64 DLL into GW2 (x64), we need a x64 helper process (GWxInjector.exe).

**API Hooking Details:**  
The hook DLL intercepts:
- `SHGetFolderPathW` (legacy API)
- `SHGetKnownFolderPath` (modern API)

When GW2 requests `%AppData%`, the hook returns the profile-specific path instead.

---

## History

This code was originally developed in `/Experiments` as a proof-of-concept but has been promoted to production status and moved here. The PoC C# test harness remains in `/PoC` for reference.

**Related Documentation:**
- `/docs/GW2 Per-Profile AppData Redirection.md` - Design document
- `/PoC/QUICKSTART.md` - Original PoC quick start guide
