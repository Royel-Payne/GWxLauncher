# Build script for GW2 Folder Hook DLL (Production)
# PowerShell version - works from any PowerShell prompt

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "  Building Gw2FolderHook.dll (Native Component)" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the right directory
if (-not (Test-Path "Gw2FolderHook")) {
    Write-Host "ERROR: This script must be run from the Native directory" -ForegroundColor Red
    Write-Host "Current directory: $PWD" -ForegroundColor Yellow
    exit 1
}

# Create Build output directory
if (-not (Test-Path "Build")) {
    New-Item -ItemType Directory -Path "Build" | Out-Null
}

Write-Host "[1/2] Building C++ Hook DLL (x64)..." -ForegroundColor Green
Write-Host "----------------------------------------------------------------"

# Try to find MSBuild using multiple methods
$msbuild = $null

# Method 1: Check if MSBuild is already in PATH
Write-Host "Checking if MSBuild is in PATH..."
$msbuildInPath = Get-Command msbuild.exe -ErrorAction SilentlyContinue
if ($msbuildInPath) {
    $msbuild = $msbuildInPath.Source
    Write-Host "  Found MSBuild in PATH: $msbuild" -ForegroundColor Green
}

# Method 2: Use vswhere to find Visual Studio installation
if (-not $msbuild) {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        Write-Host "Using vswhere to locate Visual Studio..."
        $vsPath = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
        if ($vsPath) {
            $msbuild = $vsPath
            Write-Host "  Found MSBuild via vswhere: $msbuild" -ForegroundColor Green
        }
    }
}

# Method 3: Check common Visual Studio 2022 paths
if (-not $msbuild) {
    Write-Host "Checking common Visual Studio 2022 paths..."
    $commonPaths = @(
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    )
    
    foreach ($path in $commonPaths) {
        if (Test-Path $path) {
            $msbuild = $path
            Write-Host "  Found MSBuild at: $msbuild" -ForegroundColor Green
            break
        }
    }
}

# If MSBuild still not found, show helpful error
if (-not $msbuild) {
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Red
    Write-Host "  ERROR: MSBuild not found!" -ForegroundColor Red
    Write-Host "================================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "OPTION 1 (Recommended): Use Developer PowerShell" -ForegroundColor Yellow
    Write-Host "  1. Open 'Developer PowerShell for VS 2022' from Start Menu"
    Write-Host "  2. Navigate to: $PWD"
    Write-Host "  3. Run: .\Build.ps1"
    Write-Host ""
    Write-Host "OPTION 2: Launch Visual Studio Installer and verify 'Desktop development with C++' is installed" -ForegroundColor Yellow
    Write-Host ""
    exit 2
}

# Check if MinHook library exists
if (-not (Test-Path "Gw2FolderHook\MinHook\lib\libMinHook.x64.lib")) {
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Red
    Write-Host "  ERROR: MinHook library not found!" -ForegroundColor Red
    Write-Host "================================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "You need to download the MinHook prebuilt binaries:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Visit: https://github.com/TsudaKageyu/minhook/releases" -ForegroundColor Cyan
    Write-Host "2. Download the latest release ZIP"
    Write-Host "3. Extract and copy libMinHook.x64.lib to:"
    Write-Host "   Gw2FolderHook\MinHook\lib\libMinHook.x64.lib" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "See: Gw2FolderHook\MinHook\README.md for details" -ForegroundColor Yellow
    Write-Host ""
    exit 3
}

# Build the C++ DLL
Write-Host ""
Write-Host "Building C++ project..." -ForegroundColor Yellow
$buildArgs = @(
    "Gw2FolderHook\Gw2FolderHook.vcxproj",
    "/p:Configuration=Release",
    "/p:Platform=x64",
    "/t:Rebuild",
    "/v:minimal",
    "/nologo"
)

& $msbuild $buildArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: C++ DLL build failed!" -ForegroundColor Red
    exit 4
}

Write-Host ""
Write-Host "[2/2] Verifying output..." -ForegroundColor Green
Write-Host "----------------------------------------------------------------"

if (Test-Path "Build\Gw2FolderHook.dll") {
    Write-Host "[OK] Gw2FolderHook.dll" -ForegroundColor Green
} else {
    Write-Host "[FAIL] Gw2FolderHook.dll not found" -ForegroundColor Red
    exit 5
}

Write-Host ""
Write-Host "================================================================" -ForegroundColor Green
Write-Host "  BUILD SUCCESSFUL!" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Outputs are in: $PWD\Build" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Build GWxLauncher project - the DLL will be copied automatically"
Write-Host "  2. See README.md for integration details"
Write-Host ""

exit 0
