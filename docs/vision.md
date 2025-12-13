# GWxLauncher – Vision

GWxLauncher is a **profile-driven Windows launcher** that can launch both:
- Guild Wars 1
- Guild Wars 2

from a single, consistent application.

“Unified” in this context means **one launcher and one profile model**, not identical feature sets between games.
In the UI, profiles may be referred to as “accounts” or “launch profiles” for clarity. Internally, they are represented by the GameProfile model.

---

## Primary goals

- **Unified multi-game launcher**
  - Launch GW1 or GW2 from a single application
  - Profiles are the primary unit of configuration and launching

- **Profile-based launch system**
  - Per-profile executable paths (with global fallbacks)
  - Clear, visible per-profile state

- **Theme support (system + override)**
  - Follow Windows light/dark mode by default
  - Allow a user override (System / Dark / Light)
  - Apply consistently across all forms

- **Bulk launch (explicit, opt-in)**
  - Profiles can be explicitly marked as eligible for Bulk Launch
  - Bulk Launch uses a deliberate “armed” mode (e.g., Show Checked Only)
  - Selection and filtering never imply launch intent

- **Multiclient support**
  - Allow multiple concurrent game instances
  - Initially implicit (no forced single-instance behavior)
  - Later phases may add explicit multiclient controls

- **GW1 mod injection (implemented)**
  - GWToolbox++
  - gMod / uMod-style early injection
  - Deferred/background injection (e.g. Py4GW)
  - Injection logic is isolated from UI and profile management

- **GW2 companion process launching**
  - Support launching external helper applications (e.g. Blish HUD)
  - Simple QoL automation (start-if-not-running, optional)
  - No DLL injection or client modification for GW2

- **Clean separation of concerns**
  - UI
  - Profiles / persistence
  - Launch orchestration
  - Injection logic (GW1 only)

---

## Design priorities

1. **Design clarity and debuggability over feature completeness**
2. Small, understandable steps
3. Rules-based behavior (state changes cause controlled side effects)
4. Safe failure modes with meaningful error messages
5. Project files are the authoritative source of truth

This project is intentionally built for learning and maintainability, not rapid feature parity with existing launchers.

---

## Planned core features (sequenced)

- **Bulk launch (future, explicit, opt-in)**
  - Bulk launch operates only on explicitly eligible profiles (checked)
  - Filters and selection do not imply launch intent
  - Bulk launch is “armed” via a deliberate focused view (e.g., Show Checked Only)

The following features are part of GWxLauncher’s core vision, but are
**intentionally implemented later** to avoid premature complexity and security risk:
- Per-account login automation
- Store usernames/emails per profile
- Optional encrypted password storage
- Optional auto-login flows
- Option to stop at character select instead of full auto-login

These features are deferred until:
- Profile and launch behavior is stable
- Multiclient behavior is well-defined
- Failure modes are observable and debuggable

All login automation features must be:
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
