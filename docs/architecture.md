# GWxLauncher – Architecture Overview

This document explains the structure, responsibilities, and internal flow of GWxLauncher as of 2025.
It reflects the **current codebase**, not early pre-implementation ideas.

---

## 1. High-Level Architecture

GWxLauncher is structured into four main layers:

- `/Domain` → Pure data models (profiles, DLL entries, enums)
- `/Services` → Logic for launching games, DLL injection, persistence
- `/UI` → WinForms views and their Interaction Controllers
- `/Config` → App-wide configuration (window state, legacy paths)

### Layering rules

- UI Views never contain business logic
- UI Controllers mediate between Views and Services
- Services never depend on UI logic (except for window handles where required)
- Domain never depends on anything except .NET built-ins

This separation keeps the launcher stable, testable, and easy to extend without regressions.

---

## 2. Domain Layer

### GameProfile
Represents a single user profile.

Includes:
- Display name
- Game type (`GuildWars1` or `GuildWars2`)
- Optional per-profile executable path
- GW1 mod settings:
  - Toolbox (path + enabled)
  - Py4GW
  - gMod
- Future mod list (`List<Gw1InjectedDll>`)
Note: UI terminology may refer to these as “accounts” or “launch profiles”.

### Gw1InjectedDll
Represents a single DLL entry:
- `Name`
- `Path`
- `Enabled`

### GameType
Enum identifying the game the profile targets.

The Domain layer contains **no behavior** — only state.

---

## 3. Service Layer

Services perform real work. They operate on domain models and system APIs, but do not contain UI logic.

### 3.1 ProfileManager
Responsible for:
- Loading `profiles.json`
- Saving `profiles.json`
- Maintaining the in-memory profile list

No UI or injection logic appears here.

---

### 3.2 LauncherConfig
Stores launcher-wide configuration in `launcherConfig.json`:
- Window position
- Window size
- Maximized state
- Legacy global GW1 / GW2 paths (fallback only)

Any app-level configuration belongs here.

---

### 3.3 Gw1InjectionService

This service encapsulates **all Guild Wars 1 launch and injection behavior**.
It is intentionally isolated due to its use of Win32 interop.
Refactored to use `NativeMethods` for cleaner P/Invoke separation.

Implemented injection strategies:

#### Toolbox injection
- Normal `Process.Start`
- Immediate remote-thread DLL injection

#### Py4GW injection
- Background `Task.Run`
- Waits for the game window to appear
- Injects after a short stabilization delay

#### gMod injection (early / advanced)
- Launches via `CreateProcessW` in suspended mode
- Injects DLL before the engine initializes
- Resumes the main thread

Internal responsibilities include:
- Win32 process API interop
- Remote memory allocation
- Writing Unicode DLL paths
- Creating remote threads
- Cleaning up native handles

This service is the **only place** where injection logic exists and is safe to expand independently.

---

### 3.4 Gw2LaunchOrchestrator

This service encapsulates **all Guild Wars 2 launch logic**, including:
- Mutex detection and clearing (multiclient support)
- Launch argument construction
- Auto-login coordination
- Launch reporting and diagnostics

It is isolated from UI to support both single-profile and bulk-launch scenarios.

### 3.5 Gw1InstanceTracker

Responsible for:
- Tracking running GW1 process IDs mapped to Profile IDs
- Preventing duplicate launches of the same profile
- Re-hydrating tracking state from running processes on startup

---

## 4. UI Layer

The UI layer is split between **Views** (WinForms) and **Controllers** (logic).

### 4.1 Views (Forms & Controls)

**MainForm**
- Hosts the profile grid and main toolbar
- Delegates all user actions to the `MainFormController` or specific sub-controllers
- Does not contain launch or profile management logic

**ProfileSettingsForm**
- Edit display name, paths, and mod settings
- Validates input

**AddAccountDialog**
- Minimal UI for creating profiles

### 4.2 Controllers

Controllers handle user interaction and coordinate services.

**ProfileLaunchController**
- Orchestrates the launch workflow
- Resolves executable paths and checks permissions
- Delegates to `Gw1InjectionService` or `Gw2LaunchOrchestrator`
- Updates the UI status bar (via `LaunchSessionPresenter`)

**ProfileGridController**
- Manages the grid of profile cards
- Handles selection, filtering, and click events

**Gw1ForegroundFollower** & others
- Handle specific UI-service bindings (e.g. updating window titles)

---

## 5. Data Flow

### A. Startup

- `MainForm` initializes controllers
- `MainFormRefresher` triggers initial load
- `ProfileManager.Load()` → populate profiles
- Grid is rendered

### B. Launching a profile

User action (double-click or context menu)
↓
`MainForm.LaunchProfile(profile)`
↓
Executable path resolved (profile override → global fallback)
↓
- If GW1 → `Gw1InjectionService.TryLaunchGw1(...)`
- If GW2 → `Gw2LaunchOrchestrator.Launch(...)`

### C. Editing a profile

- `MainForm` → `ProfileSettingsForm(profile)`
- User edits values
- OK → `ProfileManager.Save()`
- UI refresh

---

## 7. Future Architecture Directions (Illustrative)

These items describe *possible* directions, not commitments.

### Theme service
A centralized styling helper:

```
ThemeService.GetColor("Card.Background")
ThemeService.ApplyTo(Form)
```

### Settings window
Consolidates launcher configuration into a single UI surface.

### Unified GW1 mod manager
- Add/remove DLLs
- Auto-detect known mods
- Warn when injection order matters

### Diagnostics / logging
- Structured launch reports
- Optional log window for debugging

---

## 8. Non-Goals

GWxLauncher will not attempt to:
- Inject arbitrary DLLs generically
- Become a heavy, animated launcher
- Replace specialized mod installers
- Support mods that require deep memory manipulation beyond injection

---

## Final Summary

GWxLauncher uses a **cleanly layered, profile-centric architecture** built around:
- Explicit Win32 interop where required
- Strong separation of UI and logic
- JSON-based persistence
- Safe, isolated mod injection paths

The architecture is stable enough for long-term growth while remaining approachable for learning and experimentation.

