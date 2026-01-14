# MSBuild Not Found? Try This!

## The Problem

You ran `Build.bat` and got:
```
ERROR: MSBuild not found. Please install Visual Studio 2022 with C++ workload.
Or use Developer Command Prompt and run 'msbuild' directly.
```

But you **DO** have Visual Studio 2022 with C++ tools installed!

---

## Quick Solutions

### ? Solution 1: Use the PowerShell Script (Easiest)

The PowerShell script has better MSBuild detection:

```powershell
.\Build.ps1
```

This uses `vswhere.exe` (comes with VS) to find MSBuild automatically.

---

### ? Solution 2: Use Developer PowerShell

1. Press **Windows Key**
2. Type: **"Developer PowerShell for VS 2022"**
3. Click to open it
4. Navigate to the PoC folder:
   ```powershell
   cd "C:\Git Projects\GWxLauncher\Experiments\Gw2AppDataRedirectPoC"
   ```
5. Run either script:
   ```powershell
   .\Build.ps1
   # OR
   .\Build.bat
   ```

This sets up MSBuild in PATH automatically!

---

### ? Solution 3: Use Developer Command Prompt

Same as above, but search for:
**"Developer Command Prompt for VS 2022"**

---

### ? Solution 4: Find MSBuild Manually

Run this PowerShell command to find where MSBuild is installed:

```powershell
& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe"
```

This will print the path. Then you can either:
- Add that directory to your PATH
- Edit `Build.bat` to use that specific path

---

## Why This Happens

The batch script checks these specific paths:
- `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe`
- `C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe`
- `C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe`

If your Visual Studio is:
- A different edition (Build Tools, Preview)
- Installed in a different location
- An older version (2019)

...then the batch script won't find it!

The **PowerShell script** (`Build.ps1`) is smarter and uses `vswhere.exe` to locate Visual Studio dynamically.

---

## Which Solution Should I Use?

**For this project:**  
? **Use Solution 1 or 2** (PowerShell script or Developer PowerShell)

**For future projects:**  
? Always use Developer Command Prompt / Developer PowerShell when working with C++ projects

---

## Still Not Working?

If even the PowerShell script fails, check:

1. **Is C++ Desktop Development workload installed?**
   - Open Visual Studio Installer
   - Check "Desktop development with C++"
   - Make sure "MSVC v143" and "Windows SDK" are selected

2. **Try this manual build:**
   ```powershell
   # Find msbuild
   $msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
   
   # Build C++ DLL
   & $msbuild Gw2FolderHook\Gw2FolderHook.vcxproj /p:Configuration=Release /p:Platform=x64
   
   # Build C# app
   dotnet build Gw2AppDataRedirectPoC\Gw2AppDataRedirectPoC.csproj -c Release
   ```

3. **Last resort - Use Visual Studio GUI:**
   - Open `Gw2FolderHook\Gw2FolderHook.vcxproj` in Visual Studio
   - Set configuration to **Release | x64**
   - Right-click project ? Build
   - Then run: `dotnet build Gw2AppDataRedirectPoC\Gw2AppDataRedirectPoC.csproj -c Release`

---

## Summary

**TL;DR:**
1. Try `.\Build.ps1` instead of `Build.bat`
2. OR use "Developer PowerShell for VS 2022" from Start menu
3. Both have better MSBuild detection!

?? Once you get it building, everything else should work smoothly!
