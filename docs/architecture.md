# GWxLauncher – Architecture Overview

This document explains the structure, responsibilities, and internal flow of GWxLauncher as of 2025.
It reflects the **current codebase**, not early pre-implementation ideas.

---

## 1. High-Level Architecture

GWxLauncher is structured into four main layers:

- `/Domain` → Pure data models (profiles, DLL entries, enums)
- `/Services` → Logic for launching games, DLL injection, persistence
- `/Config` → App-wide configuration (window state, legacy paths)
- `/UI` → WinForms views with minimal logic

### Layering rules

- UI never contains business logic
- Services never depend on UI except for owner handles (e.g. window ownership)
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

## 4. UI Layer

The UI layer is intentionally thin and declarative.

### 4.1 MainForm
Responsibilities:
- Display the profile list (owner-drawn cards)
- Context menu actions
- Launch a selected profile
- Delegate GW1 launching to `Gw1InjectionService`
- Restore and persist window state
- Add / edit / delete profiles
- Apply dark-mode styling and titlebar configuration

`MainForm` never performs injection or low-level process logic.

---

### 4.2 ProfileSettingsForm
Responsibilities:
- Edit display name
- Edit executable path
- Configure GW1 DLLs (Toolbox, Py4GW, gMod)
- Validate required input
- Save changes back into the `GameProfile`

This form does not start processes or inspect DLL contents.

---

### 4.3 AddAccountDialog
Minimal UI for creating or renaming profiles.

---

## 5. Data Flow

### A. Startup

- `MainForm` → `LauncherConfig.Load()` → apply window state
- `MainForm` → `ProfileManager.Load()` → populate profiles
- `MainForm` → `RefreshProfileList()`

### B. Launching a profile

User action (double-click or context menu)
↓
`MainForm.LaunchProfile(profile)`
↓
Executable path resolved (profile override → global fallback)
↓
- If GW1 → `Gw1InjectionService.TryLaunchGw1(...)`
- If GW2 → `Process.Start(exePath)`

### C. Editing a profile

- `MainForm` → `ProfileSettingsForm(profile)`
- User edits values
- OK → `ProfileManager.Save()`
- UI refresh

---

## 6. Extensibility Philosophy

The architecture is designed to **bend without breaking**.

Extension-friendly areas include:
- Additional GW1 injection strategies
- GW2 companion process launching (e.g. Blish HUD)
- Centralized logging and diagnostics
- UI feature expansion without touching injection code

Key safety properties:
- Injection logic is fully isolated
- UI remains thin and replaceable
- JSON persistence uses stable, readable models

---

## 7. Future Architecture Directions (Illustrative)

These items describe *possible* directions, not commitments.

### Bulk launch intent (planned)

Bulk launch will be driven by an explicit per-profile eligibility flag (e.g., `BulkLaunchEnabled`)
and a UI “arming” filter (e.g., “Show Checked Accounts Only”).

Key rule: **selection and visibility never imply launch intent** — only explicit eligibility does.

This keeps batch actions safe while allowing UI-only filters that do not change underlying profile data.

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

