# GWxLauncher

A from-scratch, lightweight **Guild Wars 1 & Guild Wars 2 unified launcher** for Windows.

GWxLauncher focuses on:
- clean profile-based launching
- safe bulk launch (eligibility-based)
- optional GW1 mod injection
- GW2 auto-login / auto-play automation
- clear per-attempt LaunchReport diagnostics

## Screenshots

A quick look at the launcher UI and configuration workflow:

![Main window](docs/screenshots/main.png)
![Main-resized window](docs/screenshots/main-resized.png)

## Features

### Profiles & bulk launch
- Profile-based configuration (`GameProfile`)
- Bulk launch is **explicitly opt-in and eligibility-based** (checked profiles)
- Bulk launch is “enabled” only when:
  - one or more profiles are checked, and
  - **Show Checked Accounts Only** is enabled

### Guild Wars 1 (GW1)
- Optional mod injection support:
  - **GWToolbox++**
  - **Py4GW**
  - **gMod** (with per-profile plugin list)
- Mods can be enabled per-profile or controlled globally
- Multiclient support (user-controlled toggle)
- Launch reporting per step (success / skipped / warning / failed)

### Guild Wars 2 (GW2)
- Multiclient support (mutex handling + `-shareArchive`)
- Optional **auto-login** (DPAPI protected credentials)
- Optional **auto-play**
- Bulk launch sequencing gates on:
  - Launcher UI rendered
  - Launcher ready (Play enabled)
  - DX window created and rendering (best-effort)

### Window Management (New in v1.4)
- **Per-profile positioning:** Set exact window coordinates and size for each account.
- **Auto-save:** "Remember Changes" option updates your profile automatically when you move or resize the game window.
- **Window Lock:** Prevent accidental resizing or moving of the game window.
- **Input Blocking:** Optional removal of minimize/close buttons for kiosk-like setups.
- **Startup Enforcement:** Anti-bounce logic ensures windows land exactly where configured during game release/splash screen transitions.

## Download / install

1. Download the latest release from the GitHub **Releases** page.
2. Extract the zip somewhere (e.g. `C:\Tools\GWxLauncher\`).
3. Run `GWxLauncher.exe`.

Settings and per-profile data are stored under `%AppData%\GWxLauncher\`.

## Build from source

Prereqs:
- Visual Studio (or .NET SDK) with Windows Desktop support

Build:
```powershell
dotnet build -c Release
