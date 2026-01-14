@echo off
REM Quick test script for the PoC
REM This runs a single test with a profile in C:\Temp

echo ================================================================
echo   GW2 AppData Redirection - Quick Test
echo ================================================================
echo.

REM Check if GW2 is installed in default location
set "GW2_PATH=C:\Program Files\Guild Wars 2\Gw2-64.exe"
if not exist "%GW2_PATH%" (
    echo ERROR: GW2 not found at default location: %GW2_PATH%
    echo.
    echo Please edit this script and set GW2_PATH to your installation
    exit /b 1
)

REM Check if build exists
if not exist "Build\Gw2AppDataRedirectPoC.exe" (
    echo ERROR: PoC not built yet!
    echo Please run Build.bat first
    exit /b 2
)

REM Create test profile
set "TEST_PROFILE=C:\Temp\GW2Test\Profile1"
echo Creating test profile at: %TEST_PROFILE%
mkdir "%TEST_PROFILE%" 2>nul

echo.
echo Launching GW2 with AppData redirection...
echo.

cd Build
Gw2AppDataRedirectPoC.exe "%GW2_PATH%" "%TEST_PROFILE%"

echo.
echo ================================================================
echo Test complete!
echo.
echo Check the results:
echo   1. Log: C:\Temp\Gw2FolderHook.log
echo   2. Profile data: %TEST_PROFILE%
echo ================================================================
