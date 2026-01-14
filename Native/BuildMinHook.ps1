# Script to download and build MinHook from source
# This ensures it's compiled with the same compiler as your project

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "  Building MinHook from Source" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# Create temp directory for cloning
$tempDir = Join-Path $env:TEMP "MinHook-Build"
$minhookSource = Join-Path $tempDir "minhook"
$outputLib = "Gw2FolderHook\MinHook\lib\libMinHook.x64.lib"

Write-Host "Step 1: Downloading MinHook source code..." -ForegroundColor Yellow

# Clean up old temp directory if it exists
if (Test-Path $tempDir) {
    Write-Host "  Cleaning up old temporary files..."
    Remove-Item -Recurse -Force $tempDir
}

# Clone MinHook repository
Write-Host "  Cloning MinHook repository..."
git clone --depth 1 https://github.com/TsudaKageyu/minhook.git $minhookSource

if (-not $?) {
    Write-Host ""
    Write-Host "ERROR: Failed to clone MinHook repository" -ForegroundColor Red
    Write-Host "Make sure git is installed and accessible" -ForegroundColor Yellow
    exit 1
}

Write-Host "  ? Downloaded successfully" -ForegroundColor Green
Write-Host ""

# Find MSBuild
Write-Host "Step 2: Locating MSBuild..." -ForegroundColor Yellow

$msbuild = $null

# Check if MSBuild is in PATH
$msbuildInPath = Get-Command msbuild.exe -ErrorAction SilentlyContinue
if ($msbuildInPath) {
    $msbuild = $msbuildInPath.Source
    Write-Host "  ? Found MSBuild in PATH" -ForegroundColor Green
} else {
    # Use vswhere
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
        if ($vsPath) {
            $msbuild = $vsPath
            Write-Host "  ? Found MSBuild via vswhere" -ForegroundColor Green
        }
    }
}

if (-not $msbuild) {
    Write-Host ""
    Write-Host "ERROR: MSBuild not found" -ForegroundColor Red
    Write-Host "Please run this from Developer PowerShell for VS 2022" -ForegroundColor Yellow
    exit 2
}

Write-Host ""

# Build MinHook
Write-Host "Step 3: Building MinHook (Release x64)..." -ForegroundColor Yellow

# MinHook uses CMake for modern builds
Write-Host "  Using CMake to build MinHook..." -ForegroundColor Yellow

# Check if cmake is available
$cmake = Get-Command cmake -ErrorAction SilentlyContinue
if (-not $cmake) {
    Write-Host ""
    Write-Host "ERROR: CMake not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "CMake is required to build MinHook." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "OPTION 1: Install CMake via Visual Studio Installer" -ForegroundColor Cyan
    Write-Host "  - Open Visual Studio Installer"
    Write-Host "  - Modify your VS installation"
    Write-Host "  - Under 'Individual Components', search for 'CMake'"
    Write-Host "  - Check 'C++ CMake tools for Windows'"
    Write-Host "  - Install"
    Write-Host ""
    Write-Host "OPTION 2: Download standalone CMake" -ForegroundColor Cyan
    Write-Host "  https://cmake.org/download/"
    Write-Host ""
    Write-Host "After installing CMake, re-run this script." -ForegroundColor Yellow
    exit 3
}

Push-Location $minhookSource

try {
    # Create build directory
    if (-not (Test-Path "build")) {
        New-Item -ItemType Directory -Path "build" | Out-Null
    }
    
    Set-Location build
    
    # Configure with CMake (use Visual Studio generator explicitly)
    Write-Host "  Configuring with CMake..."
    cmake .. -G "Visual Studio 17 2022" -A x64
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "ERROR: CMake configuration failed" -ForegroundColor Red
        Pop-Location
        exit 4
    }
    
    # Build
    Write-Host "  Building..."
    cmake --build . --config Release --target MinHook
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "ERROR: CMake build failed" -ForegroundColor Red
        Pop-Location
        exit 4
    }
    
    Write-Host "  ? Built successfully" -ForegroundColor Green
}
finally {
    Pop-Location
}

Write-Host ""

# Copy the library file
Write-Host "Step 4: Copying library file..." -ForegroundColor Yellow

# Try multiple possible locations for the built library
$possibleLibPaths = @(
    "lib\libMinHook.x64.lib",
    "bin\libMinHook.x64.lib",
    "build\lib\Release\libMinHook.x64.lib",
    "build\Release\libMinHook.x64.lib",
    "build\bin\Release\libMinHook.lib",
    "lib\Release\libMinHook.x64.lib"
)

$builtLib = $null
foreach ($path in $possibleLibPaths) {
    $fullPath = Join-Path $minhookSource $path
    if (Test-Path $fullPath) {
        $builtLib = $fullPath
        Write-Host "  Found library at: $path" -ForegroundColor Green
        break
    }
}

# If still not found, search for it
if (-not $builtLib) {
    Write-Host "  Searching for library file..." -ForegroundColor Yellow
    $libFiles = Get-ChildItem -Path $minhookSource -Filter "*MinHook*.lib" -Recurse | Where-Object { 
        $_.FullName -like "*Release*" -or $_.FullName -like "*x64*" 
    } | Select-Object -First 1
    
    if ($libFiles) {
        $builtLib = $libFiles.FullName
        Write-Host "  Found library: $($libFiles.Name)" -ForegroundColor Green
    }
}

if (-not $builtLib) {
    Write-Host ""
    Write-Host "ERROR: Built library not found" -ForegroundColor Red
    Write-Host ""
    Write-Host "All .lib files in MinHook directory:" -ForegroundColor Yellow
    Get-ChildItem -Recurse $minhookSource -Filter "*.lib" | ForEach-Object { 
        $relativePath = $_.FullName.Replace($minhookSource, ".")
        Write-Host "  $relativePath" 
    }
    Write-Host ""
    Write-Host "Please build MinHook manually:" -ForegroundColor Yellow
    Write-Host "  1. Open: $minhookSource" -ForegroundColor Cyan
    Write-Host "  2. Find and open the .sln file in Visual Studio"
    Write-Host "  3. Build as Release x64"
    Write-Host "  4. Copy the resulting .lib file to:" -ForegroundColor Cyan
    Write-Host "     $outputLib"
    exit 5
}

# Create output directory
$outputDir = Split-Path $outputLib -Parent
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# Copy the library
Copy-Item $builtLib $outputLib -Force
Write-Host "  ? Copied to: $outputLib" -ForegroundColor Green
Write-Host ""

# Verify the file
if (Test-Path $outputLib) {
    $fileInfo = Get-Item $outputLib
    Write-Host "Step 5: Verification" -ForegroundColor Yellow
    Write-Host "  ? File exists: $outputLib" -ForegroundColor Green
    Write-Host "  ? Size: $($fileInfo.Length) bytes" -ForegroundColor Green
    Write-Host "  ? Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Green
}

Write-Host ""
Write-Host "================================================================" -ForegroundColor Green
Write-Host "  SUCCESS! MinHook built and ready to use" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Now run: .\Build.ps1" -ForegroundColor Cyan
Write-Host ""

# Cleanup
Write-Host "Cleaning up temporary files..." -ForegroundColor Yellow
Remove-Item -Recurse -Force $tempDir
Write-Host "  ? Done" -ForegroundColor Green

exit 0
