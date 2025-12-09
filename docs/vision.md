GWxLauncher Vision

GWxLauncher is intended to become a unified, profile-driven launcher for:
- Guild Wars 1
- Guild Wars 2

Long-term feature goals:
- Profile-based launch system
- Per-profile enabled/disabled flags
- Multiclient launching
- GW1 mod injection:
  - GWToolbox++
  - gMod/uMod
  - Plugin chains
- GW2 mod launching:
  - Blish-HUD
- Clean separation between:
  - UI
  - Profiles
  - Launch logic
  - Injection logic
- Per-account login automation:
  - Store and auto-fill usernames/emails
  - Optionally store and auto-fill passwords (with encryption)
  - Optional auto-login into a specific character (GW1 and GW2)
  - Option to stop at character select instead of full auto-login


Non-goals:
- Replicating every feature of Gw2Launcher
- Supporting non-Windows platforms
- Premature optimization

Design priorities:
1. Clear state visibility via logging and debugger
2. Small, understandable steps
3. Rules-based behavior (state changes cause controlled side effects)
4. Safe failure modes with meaningful error messages
