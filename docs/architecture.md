GWxLauncher – High-Level Architecture

Core Layers:

1. UI Layer
   - Windows Forms UI (for now)
   - Dark mode
   - Displays profiles
   - Launch buttons
   - Toggle states


2. Data Models
   - GameProfile
     - Name
     - GameType (GW1 or GW2)
     - ExecutablePath
     - Enabled
     - ToolboxEnabled (GW1 only)
     - Notes
     - AccountEmailOrLogin
     - AccountPassword (stored securely, not plain text) – future
     - PreferredCharacterName (optional)
     - AutoLoginEnabled (bool)
     - AutoSelectCharacter (bool)

3. Profile Manager
   - Holds all profiles in memory
   - Responsible for:
     - Enabling/disabling profiles
     - Toggling Toolbox flags
     - Enforcing rules between flags

4. Launch Service
   - Responsible for:
     - Process.Start for GW1 and GW2
     - Applying launch arguments (email/login, etc.) where supported
     - Passing control to an Account/Login Automation layer (future)
     - Multiclient handling later
     - Launch arguments


5. Injection Service (GW1 only – future)
   - Starts GW1 process in suspended state
   - Injects:
     - GWToolbox++
     - gMod/uMod
     - Any plugins (file management only, plugins managed by gMod but need to be in directories that gMod knows to look in)
   - Resumes process

6. GW2 Mod Launcher (future)
   - Starts Blish-HUD
   - Starts GW2 in correct order

Design Pattern:
- UI never talks directly to injection logic
- UI talks only to ProfileManager and LaunchService
- Injection is a post-launch operation
- All state changes are logged
