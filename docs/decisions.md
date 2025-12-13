# GWxLauncher – Decisions Log

> Note: Superseded by **2025-02 — UI / UX Interaction Model (Profiles, Bulk Launch, Scope)**.
> “Launch All” is replaced by eligibility-based **Bulk Launch** (checked profiles), not view/visibility-based launching.


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

### From‑scratch launcher (not a Gw2Launcher fork)

**Decision**  
GWxLauncher is implemented from scratch and does not reuse Gw2Launcher code.

**Rationale**  
- Learning‑focused project
- Avoid inheriting complexity, assumptions, or architectural constraints
- Full control over injection boundaries and UI behavior

**Consequences**  
- Some features appear later than in mature launchers
- Architecture stays small, readable, and purpose‑built

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

### Multiclient behavior is defined before mechanisms

**Decision**  
Multiclient support is specified in terms of **observable behavior**, not low‑level mechanisms.

**Rationale**  
- Mutexes, embedded browsers (CEF), and similar internals are implementation details
- Prematurely committing to techniques risks misinformation and rework

**Consequences**  
- Current behavior allows multiple launches implicitly
- Specific multiclient mechanics will be researched and documented *when required*

---

## 2025‑01 — Login automation sequencing

### Login automation is a core goal, but intentionally sequenced

**Decision**  
Per‑account login automation is part of the project’s core vision, but is **not implemented early**.

**Rationale**  
- Early implementation would have added security and architectural risk
- Requires stable profile behavior and well‑defined multiclient semantics

**Consequences**  
- Feature is planned, not optional
- Implementation is deferred until launch behavior is observable and debuggable

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

### Theme follows system by default, with a user override

**Decision**  
The launcher will support a theme mode setting: `System` (default), `Dark`, and `Light`.

**Rationale**  
- Matches user expectations on Windows
- Allows a stable manual override for edge cases and preference

**Consequences**  
- A single place must resolve effective theme and apply it consistently across all forms
- Forms should not hand-roll theme logic independently

---

## 2025-01 — Profile views and launch-all behavior

### Profiles are filtered by view, not duplicated into separate groups

**Decision**  
GW1/GW2/All “views” are UI filters over the same underlying profile list. No separate group membership lists.

**Rationale**  
- Avoids redundant state and desync
- Keeps profiles as the single source of truth

**Consequences**  
- Filtering/sorting is UI state only
- Persistence does not change when the view changes

---

### (Superseded) Launch-all via view filtering

**Decision**  
“Launch All” operates on the current view (GW1-only or GW2-only). It skips profiles missing valid executable paths and continues launching the rest.

**Rationale**  
- Prevents accidental mixed-game mass launches
- Skipping invalid profiles avoids user-blocking modal loops
- Continuing allows partial success (and supports future diagnostics)

**Consequences**  
- UI should clearly indicate which view is active
- Later: surface per-profile results (LaunchReport) instead of relying only on message boxes

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
- Double-clicking the profile row
- Context menu → Launch

**Rationale**  
Explicit, intentional, and predictable.

**Consequences**  
- No per-row launch buttons
- Selection alone never implies launch intent

---

### Bulk launch is opt-in and eligibility-based

**Decision**  
Bulk launch is performed via a single Launch action and launches only profiles explicitly enabled for Bulk Launch.

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
Bulk launch is considered “armed” only when:
- One or more profiles are checked, and
- “Show Checked Accounts Only” is enabled

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
- UI labels may say “Account”, “Launch Profile”, or similar
- Documentation clarifies this distinction explicitly

---

## Usage notes

- Add entries **only** when a decision affects architecture, scope, or long‑term direction
- Prefer short, factual explanations over debate
- If a decision is reversed later, add a *new* entry explaining why

