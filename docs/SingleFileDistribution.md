# Single-File Distribution

GWxLauncher now supports true single-file distribution using embedded resources.

## How It Works

1. **Native dependencies are embedded** as resources in the main executable
2. **On first run**, dependencies are extracted to `%AppData%\GWxLauncher\Bin\`
3. **Automatic updates**: If you ship a new version with updated DLLs, they're automatically re-extracted

## Building a Single-File Release

### Prerequisites

First, build the native dependencies:

```powershell
# Build the C++ hook DLL
.\Native\Build.ps1

# The injector will be built automatically by MSBuild
```

### Standard Build (Development)

```powershell
dotnet build -c Release
```

This creates a regular build with all files in the output directory.

### Single-File Build (Distribution)

```powershell
dotnet publish -c Release -r win-x86 `
  /p:PublishSingleFile=true `
  /p:SelfContained=true `
  /p:IncludeNativeLibrariesForSelfExtract=true
```

This creates **one executable** in `bin\Release\net8.0-windows\win-x86\publish\GWxLauncher.exe`.

## What Gets Embedded

- `Gw2FolderHook.dll` - Native C++ hook for AppData redirection
- `GWxInjector.exe` - x64 helper for DLL injection (required because main app is x86)

## Extracted File Location

```
%AppData%\GWxLauncher\Bin\
??? Gw2FolderHook.dll
??? GWxInjector.exe
```

Files are extracted automatically on first run and updated when you ship new versions.

## Version Updates

When you update the embedded DLLs:
1. User runs the new version
2. App detects files have changed (SHA256 hash comparison)
3. Automatically extracts updated versions
4. GW2 isolation uses the new files

No manual cleanup required!

## Troubleshooting

**Q: GW2 isolation says "Hook DLL not found"**

A: The native DLL wasn't built before publishing. Run:
```powershell
.\Native\Build.ps1
dotnet publish -c Release -r win-x86 /p:PublishSingleFile=true
```

**Q: Can I delete files from `%AppData%\GWxLauncher\Bin\`?**

A: Yes! They'll be re-extracted on next launch.

**Q: Why not bundle everything into the EXE?**

A: .NET's `PublishSingleFile` can't embed separate executables like `GWxInjector.exe`. The extraction approach gives us true single-file distribution while supporting external executables.

## Size Comparison

- **Before** (multi-file): 3 files, ~2.5 MB total
- **After** (single-file): 1 file, ~2.5 MB (same size, better UX)
