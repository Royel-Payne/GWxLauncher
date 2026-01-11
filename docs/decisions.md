# GWxLauncher – Decisions Log

This document records **intentional design decisions** made during development of GWxLauncher.

Its purpose is to:
- Capture *why* certain choices were made
- Prevent future drift or re‑litigation of settled questions
- Provide context for deferred or sequenced features

This is **not** a full change log and **not** a roadmap.
Only decisions that affect architecture, scope, or long‑term direction belong here.

---

## 2025‑01 — Project structure & scope

### Profiles are the primary abstraction

**Decision**  
Profiles (not games, accounts, or global configs) are the core unit of configuration and launching.

**Rationale**  
- Enables multi‑game support (GW1 + GW2) without branching UI logic
- Simplifies multiclient behavior (each profile represents an independent launch intent)
- Makes future features (login automation, companion apps) easier to scope per profile

**Consequences**  
- UI, persistence, and launch logic all revolve around `GameProfile`
- Global paths exist only as fallbacks, not as the primary configuration mechanism

---

### GW2 launcher responsibility boundary

**Decision**  
GWxLauncher's responsibility for Guild Wars 2 ends once the DX game window is created and rendering.

Entering the world, character selection, and gameplay are explicitly user-controlled and out of scope.

**Rationale**  
- The launcher's role is to automate **launch and login**, not gameplay
- DX window creation is the first reliable, observable signal that the game client is running
- Waiting beyond this point introduces ambiguity, instability, and user-specific behavior

**Consequences**  
- Bulk launch serialization gates on **DX window creation + rendering** (stable signal)
- Launch reports stop at DX readiness
- No automation is attempted beyond the launcher → game transition

---

## 2025‑01 — GW1 injection architecture

### Injection logic is isolated in a dedicated service

**Decision**  
All Guild Wars 1 injection logic lives exclusively in `Gw1InjectionService`.

**Rationale**  
- Win32 interop and remote‑process manipulation are high‑risk
- Keeps unsafe code out of UI and domain layers
- Makes injection strategies easier to reason about and extend

**Consequences**  
- UI delegates launch decisions instead of performing injection directly
- Future injection changes do not ripple through the app

---

### Multiple GW1 injection strategies are supported explicitly

**Decision**  
Toolbox, Py4GW, and gMod are implemented as **distinct injection paths**, not a single generic injector.

**Rationale**  
- Different mods require different timing and launch behavior
- Early injection (gMod) cannot be treated the same as deferred injection (Py4GW)

**Consequences**  
- Slightly more code
- Much clearer behavior and fewer edge‑case bugs

---

## 2025‑01 — Multiclient handling

### Multiclient behavior is explicit and per-game

**Decision**  
Multiclient support is implemented with explicit mechanisms:
- **GW1**: Memory patching via suspended CreateProcessW
- **GW2**: Mutex killing + `-shareArchive` argument

**Rationale**  
- Each game requires different multiclient strategies
- Making the mechanisms explicit improves debuggability and user understanding

**Consequences**  
- Multiclient flags are per-game in settings
- Launch reports show multiclient step outcomes
- Future games would need game-specific multiclient strategies

---

## 2025‑01 — Login automation (implemented)

### Login automation is a core goal (now implemented)

**Decision**  
Per‑profile login automation is implemented with:
- **GW1**: Command-line arguments (`-email`, `-password`, `-character`)
- **GW2**: UI automation via FlaUI (finds launcher window, fills credentials, clicks Login/Play)

**Rationale**  
- Login automation is a common use case for multi-account launchers
- Credentials are stored per-profile using DPAPI encryption
- Implementation was sequenced after stable launch behavior was achieved

**Consequences**  
- Credentials are stored in `profiles.json` (encrypted base64)
- Auto-login is opt-in per profile
- GW2 auto-play is optional (stops at character select)

---

## 2025‑01 — Non‑goals

### No background services or stealth behavior

**Decision**  
GWxLauncher will not run persistent background services and will not attempt stealth or anti‑cheat evasion.

**Rationale**  
- Keeps the launcher transparent and user‑controlled
- Reduces risk and complexity

**Consequences**  
- Some automation features may require explicit user action
- Design favors clarity over invisibility

---

## 2025-01 — Theming

### Theme follows dark by default, light/system detection deferred

**Decision**  
The launcher uses a canonical dark theme by default. A full theme system (system detection, user switching, multiple themes) is optional and deferred.

**Rationale**  
- The current default dark theme meets usability and aesthetic goals
- Introducing a full theming system would add complexity without clear user benefit at this stage

**Consequences**  
- The current theme is treated as the canonical default  
- ThemeService may evolve incrementally but no dedicated theme UI is planned  
- Theme switching may be revisited in a later polish phase

---

## 2025-01 — Profile views and launch behavior

### Profiles are filtered by view, not duplicated into separate groups

**Decision**  
GW1/GW2/All "views" are UI filters over the same underlying profile list. No separate group membership lists.

**Rationale**  
- Avoids redundant state and desync
- Keeps profiles as the single source of truth

**Consequences**  
- Filtering/sorting is UI state only
- Persistence does not change when the view changes

---

## 2025-02 — UI / UX Interaction Model (Profiles, Bulk Launch, Scope)

### Profiles are the unit of launch intent

**Decision**  
Profiles are the primary unit of launch intent. All launch behavior is defined in terms of which profiles are eligible to launch and how they launch.

**Rationale**  
Keeps behavior explicit, predictable, and scalable as multiclient, auto-login, and mod injection expand.

**Consequences**  
- Single-profile launch is always explicit and scoped to one profile
- Bulk launch is treated as a deliberate action with explicit eligibility

---

### Single-profile launch is explicit

**Decision**  
A single profile may be launched via:
- Double-clicking the profile card
- Context menu → Launch

**Rationale**  
Explicit, intentional, and predictable.

**Consequences**  
- No per-row launch buttons
- Selection alone never implies launch intent

---

### Bulk launch is opt-in and eligibility-based

**Decision**  
Bulk launch is performed via a single Launch action and launches only profiles explicitly enabled for Bulk Launch (checked).

**Rationale**  
Bulk actions are risky; explicit eligibility prevents accidental launches.

**Consequences**  
- Only checked/eligible profiles can bulk launch
- Visibility and selection do not determine eligibility

---

### UI concepts are strictly separated

**Decision**  
The launcher strictly separates:
- **Selection** (focused for inspection/edit) → no launch effect  
- **Visibility** (currently shown in list) → no launch effect  
- **Checked** (explicit bulk eligibility) → launch effect  

**Rationale**  
Prevents accidental launches due to scrolling, filtering, or transient UI focus.

**Consequences**  
- Launch intent is always explicit
- UI state changes never silently alter launch behavior

---

### Bulk launch arming requires focused confirmation

**Decision**  
Bulk launch is considered "armed" only when:
- One or more profiles are checked, and
- "Show Checked Accounts Only" is enabled

**Rationale**  
Two-step safety model: mark eligibility, then enter focused launch view.

**Consequences**  
- The Launch button clearly reflects intent
- Users cannot forget what will launch before clicking Launch

---

## 2025-02 — Terminology: Profile vs Account

### Decision
The internal domain model is named **Profile** (`GameProfile`), while the UI may refer to profiles as **Accounts** or **Launch Profiles** for clarity.

### Rationale
- `GameProfile` represents a launch intent, not necessarily a login account
- UI language should be approachable and familiar to users
- Avoids a large refactor while keeping semantics clear

### Consequences
- Code, persistence, and architecture use `Profile`
- UI labels may say "Account", "Launch Profile", or similar
- Documentation clarifies this distinction explicitly

---

## 2025-02 — gMod per-profile plugin selection (implemented)

### Per-profile gMod plugin selection via hardlinked DLL folder

**Decision**  
GWxLauncher supports **per-profile gMod plugin configuration** by injecting gMod from a **per-profile folder** containing:
- a **hardlink** to the canonical `gMod.dll`
- a **per-profile `modlist.txt`** generated by the launcher

The injected DLL path determines which `modlist.txt` gMod uses.

**Rationale**  
Testing confirmed that:
- gMod loads `modlist.txt` from the **directory containing the injected `gMod.dll`**
- gMod behaves correctly when the injected DLL is a **hardlink**
- Removing `modlist.txt` from the GW install folder does not affect behavior when the DLL is injected from another folder

This enables granular, per-profile plugin selection **without**:
- copying DLLs
- moving plugin files
- requiring admin privileges
- depending on GW multiclient folder separation

**Implementation model**  
For each profile, GWxLauncher creates:

```
%AppData%\GWxLauncher\accounts\<ProfileId>\
  ├── gMod.dll (hardlink → canonical gMod.dll)
  └── modlist.txt (generated per profile)
```

Canonical gMod.dll path is per-profile (each profile can point to a different canonical source).

- `modlist.txt` contains absolute paths to plugin files
- plugin files remain in their original locations
- no plugin or DLL files are duplicated or relocated
- supported plugin files use extension `.tpf`

**UI behavior**  
In the GW1 profile settings panel:
- Users may add plugins via file browser
- Selected plugins are displayed as a list
- Removing a plugin updates the list
- Any change regenerates `modlist.txt` deterministically
- No per-plugin toggles: list is the enabled set; remove = disable.

**Launch behavior**  
When launching GW1 with gMod enabled:
- gMod is injected from the per-profile folder
- gMod loads plugins listed in that profile's `modlist.txt`
- No additional report entries (silent, only affects behavior)

**Canonical gMod.dll handling**  
GWxLauncher treats the user-selected gMod.dll path as the canonical source.
If the canonical path changes:
- per-profile hardlinks are recreated or repaired automatically

**Constraints**  
- No admin elevation required (hardlinks on same volume)
- No background services
- No implicit behavior; all plugin selection is explicit
- Fully compatible with future GW1 multiclient strategies

**Consequences**  
- Enables true per-profile mod control
- Keeps disk usage minimal
- Keeps launch behavior explicit and predictable
- Adds a small amount of filesystem management responsibility to the launcher

---

## 2025-02 — Window Management (implemented)

### Window positioning enforcement + lifecycle management

**Decision**  
Window management uses a two-phase approach:
1. **Enforcement (0-7 seconds)**: Continuously re-apply profile settings to fix "bounce" during game startup
2. **Watching (7+ seconds)**: Monitor for user changes and optionally auto-save if "Remember Changes" is enabled

**Rationale**  
- GW1 window positioning is fragile during startup (splash screens, UI initialization)
- Without enforcement, windows "bounce" to unintended positions
- Enforcement must be time-limited to avoid fighting user adjustments after startup
- "Remember Changes" allows users to adjust windows and have the launcher persist their preferences

**Consequences**  
- Window positioning is reliable during game startup
- Users can optionally adjust windows after launch and have changes auto-saved
- Window lock and input blocking are applied during enforcement phase
- "Remember Changes" and "Window Lock" are mutually exclusive in behavior (lock prevents changes)

---

## Usage notes

- Add entries **only** when a decision affects architecture, scope, or long‑term direction
- Prefer short, factual explanations over debate
- If a decision is reversed later, add a *new* entry explaining why

