@echo off
REM Build script for GW2 AppData Redirection PoC
REM This builds both the C++ DLL and C# injector

echo ================================================================
echo   Building GW2 AppData Redirection Proof of Concept
echo ================================================================
echo.

REM Check if we're in the right directory
if not exist "Gw2FolderHook" (
    echo ERROR: This script must be run from the Gw2AppDataRedirectPoC directory
    echo Current directory: %CD%
    exit /b 1
)

REM Create Build output directory
if not exist "Build" mkdir Build

echo [1/3] Building C++ Hook DLL (x64)...
echo ----------------------------------------------------------------

REM Try to find MSBuild using multiple methods
set "MSBUILD="

REM Method 1: Check if MSBuild is already in PATH (Developer Command Prompt)
where msbuild.exe >nul 2>&1
if %errorlevel% equ 0 (
    set "MSBUILD=msbuild.exe"
    echo Found MSBuild in PATH
    goto :msbuild_found
)

REM Method 2: Use vswhere to find Visual Studio installation
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist "%VSWHERE%" (
    echo Using vswhere to locate Visual Studio...
    for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
        set "MSBUILD=%%i"
        echo Found MSBuild via vswhere: %%i
        goto :msbuild_found
    )
)

REM Method 3: Check common Visual Studio 2022 installation paths
echo Checking common Visual Studio 2022 paths...
if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    goto :msbuild_found
)
if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
    goto :msbuild_found
)
if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    goto :msbuild_found
)

REM Method 4: Check VS 2019 as fallback
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    goto :msbuild_found
)

REM If we get here, MSBuild wasn't found
echo.
echo ================================================================
echo   ERROR: MSBuild not found!
echo ================================================================
echo.
echo Tried multiple methods to locate MSBuild without success.
echo.
echo OPTION 1 (Recommended): Use Developer Command Prompt
echo   1. Open "Developer Command Prompt for VS 2022" from Start Menu
echo   2. Navigate to: %CD%
echo   3. Run: Build.bat
echo.
echo OPTION 2: Add MSBuild to your PATH, then re-run this script
echo.
echo OPTION 3: Build manually:
echo   C++: msbuild Gw2FolderHook\Gw2FolderHook.vcxproj /p:Configuration=Release /p:Platform=x64
echo   C#:  dotnet build Gw2AppDataRedirectPoC\Gw2AppDataRedirectPoC.csproj -c Release
echo.
exit /b 2

:msbuild_found

REM Check if MinHook library exists
if not exist "Gw2FolderHook\MinHook\lib\libMinHook.x64.lib" (
    echo.
    echo ================================================================
    echo   ERROR: MinHook library not found!
    echo ================================================================
    echo.
    echo You need to download the MinHook prebuilt binaries:
    echo.
    echo 1. Visit: https://github.com/TsudaKageyu/minhook/releases
    echo 2. Download the latest release ZIP
    echo 3. Extract and copy libMinHook.x64.lib to:
    echo    Gw2FolderHook\MinHook\lib\libMinHook.x64.lib
    echo.
    echo See: Gw2FolderHook\MinHook\README.md for details
    echo.
    exit /b 3
)

REM Build the C++ DLL
"%MSBUILD%" Gw2FolderHook\Gw2FolderHook.vcxproj /p:Configuration=Release /p:Platform=x64 /t:Rebuild /v:minimal
if errorlevel 1 (
    echo.
    echo ERROR: C++ DLL build failed!
    exit /b 4
)

echo.
echo [2/3] Building C# Injector (x64)...
echo ----------------------------------------------------------------

REM Build the C# project
dotnet build Gw2AppDataRedirectPoC\Gw2AppDataRedirectPoC.csproj -c Release --nologo
if errorlevel 1 (
    echo.
    echo ERROR: C# injector build failed!
    exit /b 5
)

echo.
echo [3/3] Verifying outputs...
echo ----------------------------------------------------------------

if exist "Build\Gw2FolderHook.dll" (
    echo [OK] Gw2FolderHook.dll
) else (
    echo [FAIL] Gw2FolderHook.dll not found
    exit /b 6
)

if exist "Build\Gw2AppDataRedirectPoC.exe" (
    echo [OK] Gw2AppDataRedirectPoC.exe
) else (
    echo [FAIL] Gw2AppDataRedirectPoC.exe not found
    exit /b 7
)

echo.
echo ================================================================
echo   BUILD SUCCESSFUL!
echo ================================================================
echo.
echo Outputs are in: %CD%\Build
echo.
echo Next steps:
echo   1. Read the README.md for testing instructions
echo   2. Run: Build\Gw2AppDataRedirectPoC.exe "C:\Program Files\Guild Wars 2\Gw2-64.exe" "C:\Temp\GW2Test\Profile1"
echo.

exit /b 0
