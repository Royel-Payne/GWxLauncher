# Quick test script for the PoC
# PowerShell version

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "  GW2 AppData Redirection - Quick Test" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

# Check if GW2 is installed in default location
$gw2Path = "C:\Program Files\Guild Wars 2\Gw2-64.exe"
if (-not (Test-Path $gw2Path)) {
    Write-Host "ERROR: GW2 not found at default location: $gw2Path" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please edit this script and set `$gw2Path to your installation" -ForegroundColor Yellow
    exit 1
}

# Check if build exists
if (-not (Test-Path "Build\Gw2AppDataRedirectPoC.exe")) {
    Write-Host "ERROR: PoC not built yet!" -ForegroundColor Red
    Write-Host "Please run Build.ps1 first" -ForegroundColor Yellow
    exit 2
}

# Create test profile
$testProfile = "C:\Temp\GW2Test\Profile1"
Write-Host "Creating test profile at: $testProfile" -ForegroundColor Cyan
New-Item -ItemType Directory -Path $testProfile -Force | Out-Null

Write-Host ""
Write-Host "Launching GW2 with AppData redirection..." -ForegroundColor Green
Write-Host ""

Set-Location Build
& .\Gw2AppDataRedirectPoC.exe $gw2Path $testProfile

Write-Host ""
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "Test complete!" -ForegroundColor Cyan
Write-Host ""
Write-Host "Check the results:" -ForegroundColor Yellow
Write-Host "  1. Log: C:\Temp\Gw2FolderHook.log"
Write-Host "  2. Profile data: $testProfile"
Write-Host "================================================================" -ForegroundColor Cyan
