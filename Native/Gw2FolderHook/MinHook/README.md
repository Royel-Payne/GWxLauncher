# MinHook Library

This folder should contain the MinHook library files needed to build the hook DLL.

## What is MinHook?

MinHook is a minimalistic x86/x64 API hooking library. It's used in this PoC to intercept Windows Shell API calls and redirect AppData paths.

**Official Repository**: https://github.com/TsudaKageyu/minhook

## Required Files

You need to obtain the prebuilt MinHook binaries. The project structure expects:

```
MinHook/
??? include/
?   ??? MinHook.h        (already included)
??? lib/
    ??? libMinHook.x64.lib
```

## How to Get MinHook

### Option 1: Download Prebuilt Binaries (Recommended)

1. Go to: https://github.com/TsudaKageyu/minhook/releases
2. Download the latest release (e.g., `minhook_1_3_3.zip`)
3. Extract the archive
4. Copy files to this project:
   - Copy `lib\libMinHook.x64.lib` to `Experiments/Gw2AppDataRedirectPoC/Gw2FolderHook/MinHook/lib/`
   - The header file (MinHook.h) is already included in this project

### Option 2: Build from Source

If you prefer to build MinHook yourself:

1. Clone the repository:
   ```
   git clone https://github.com/TsudaKageyu/minhook.git
   ```

2. Open `build/VC17/MinHook.sln` in Visual Studio 2022

3. Build for **Release | x64**

4. Copy the output:
   - Copy `lib\libMinHook.x64.lib` to this project's `MinHook/lib/` folder

### Option 3: Use NuGet (Alternative)

You can also use the NuGet package, but this requires modifying the .vcxproj file:

```
Install-Package MinHook -Version 1.3.3
```

Then update the include/lib paths in `Gw2FolderHook.vcxproj` accordingly.

## Verification

Before building the C++ DLL, ensure you have:

? `MinHook/include/MinHook.h` (already present)  
? `MinHook/lib/libMinHook.x64.lib` (you must download this)

If `libMinHook.x64.lib` is missing, the build will fail with linker errors.

## License

MinHook is licensed under the 2-Clause BSD License. See the official repository for full license text.

## Build Command (After Setup)

Once you have the library file:

```batch
# From the Gw2FolderHook directory
msbuild Gw2FolderHook.vcxproj /p:Configuration=Release /p:Platform=x64
```

Output will be placed in: `../Build/Gw2FolderHook.dll`
