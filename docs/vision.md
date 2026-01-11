# GWxLauncher – Vision

GWxLauncher is a **profile-driven Windows launcher** that can launch both:
- Guild Wars 1
- Guild Wars 2

from a single, consistent application.

"Unified" in this context means **one launcher and one profile model**, not identical feature sets between games.
In the UI, profiles may be referred to as "accounts" or "launch profiles" for clarity. Internally, they are represented by the GameProfile model.

---

## Primary goals

- **Unified multi-game launcher**
  - Launch GW1 or GW2 from a single application
  - Profiles are the primary unit of configuration and launching

- **Profile-based launch system**
  - Per-profile executable paths (with global fallbacks)
  - Clear, visible per-profile state

- **Theme support (dark theme)**
  - Default dark theme applied consistently across all forms
  - System light/dark detection deferred (dark theme is canonical)

- **Bulk launch (explicit, opt-in)**
  - Profiles can be explicitly marked as eligible for Bulk Launch
  - Bulk Launch uses a deliberate "armed" mode (Show Checked Accounts Only)
  - Selection and filtering never imply launch intent

- **Multiclient support**
  - Allow multiple concurrent game instances
  - Mutex handling (GW2) and memory patching (GW1)
  - Explicit per-game multiclient toggles in settings

- **GW1 mod injection (implemented)**
  - GWToolbox++
  - gMod with per-profile plugin selection via hardlinked DLL folders
  - Py4GW (deferred/background injection)
  - Injection logic is isolated from UI and profile management

- **GW2 companion process launching (implemented)**
  - Support launching external helper applications (e.g. Blish HUD)
  - Mumble link integration for multi-instance companion apps
  - Run-after programs configured per profile

- **GW2 auto-login / auto-play (implemented)**
  - DPAPI-encrypted credentials stored per profile
  - Optional auto-play (clicks "Play" after login)
  - Launch gating waits for DX window creation

- **GW1 window management (implemented)**
  - Per-profile window positioning (x/y/width/height)
  - Window state enforcement during startup (7-second anti-bounce)
  - Optional "Remember Changes" to auto-save user adjustments
  - Optional window locking (prevent resize/move)
  - Optional input blocking (remove minimize/close buttons)

- **GW2 window management (implemented)**
  - Per-profile window positioning
  - Custom window titles per profile
  - Windowed mode argument injection

- **Clean separation of concerns**
  - UI Controllers
  - Services (launch orchestration, injection, automation, tracking)
  - Profiles / persistence
  - Domain model

---

## Design priorities

1. **Design clarity and debuggability over feature completeness**
2. Small, understandable steps
3. Rules-based behavior (state changes cause controlled side effects)
4. Safe failure modes with meaningful error messages
5. JSON files are the authoritative source of truth

This project is intentionally built for learning and maintainability, not rapid feature parity with existing launchers.

---

## Implemented core features

- **✅ Bulk launch (explicit, opt-in)**
  - Bulk launch operates only on explicitly eligible profiles (checked)
  - Filters and selection do not imply launch intent
  - Bulk launch is "armed" via a deliberate focused view (Show Checked Accounts Only)

- **✅ Per-profile Login Automation**
  - Store usernames/emails per profile
  - Encrypted password storage (DPAPI)
  - Auto-login flows (GW1 command-line arguments, GW2 UI automation)
  - Optional auto-play for GW2 (stops at character select)

- **✅ GW1 Mod Injection**
  - GWToolbox++ (early injection)
  - gMod (per-profile plugin selection via hardlinked DLL folders)
  - Py4GW (deferred background injection after window ready)
  - Multiclient memory patching

- **✅ GW2 Companion Apps**
  - Run-after programs configured per profile
  - Mumble link slot assignment
  - Automatic launching after GW2 DX window is ready

- **✅ Window Management**
  - Per-profile positioning for GW1 and GW2
  - Window state enforcement (anti-bounce during startup)
  - Optional auto-save of user adjustments
  - Optional window locking and input blocking (GW1)
  - Custom window titles

- **✅ Launch Reporting**
  - Per-step launch diagnostics
  - Success/Failed/Pending/Skipped outcomes
  - LaunchReport viewer in context menu

- **✅ Instance Tracking**
  - Tracks running GW1/GW2 processes
  - Maps processes to profiles
  - Rehydrates on launcher startup
  - Visual "running" indicator in profile cards

- **✅ View System**
  - Multiple views (All/GW1/GW2/custom)
  - Per-view eligibility (checked profiles)
  - Per-view "Show Checked Only" state
  - Rename/create views

All login automation features are:
- Opt-in
- Clearly scoped
- Designed with explicit security and UX considerations

---

## Non-goals

- Replicating every feature of Gw2Launcher or similar tools
- Supporting non-Windows platforms
- Background services or always-running processes
- Stealth, anti-cheat evasion, or adversarial behavior
- Premature optimization
- Full theme system (system light/dark detection deferred)
