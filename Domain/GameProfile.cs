namespace GWxLauncher.Domain
{
    // ------------------------------------------------------------
    // GW1 injected DLL descriptor
    // ------------------------------------------------------------

    public class Gw1InjectedDll
    {
        public string Name { get; set; } = "";   // "Toolbox", "gMod", etc.
        public string Path { get; set; } = "";   // Full path to the DLL
        public bool Enabled { get; set; } = true;

        public override string ToString()
            => $"{(Enabled ? "✓" : "✗")} {Name} ({Path})";
    }

    // ------------------------------------------------------------
    // Game profile (GW1 / GW2)
    // ------------------------------------------------------------

    public class GameProfile
    {
        // -----------------------------
        // Identity / common
        // -----------------------------

        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "";
        public GameType GameType { get; set; }
        public string ExecutablePath { get; set; } = "";

        public bool BulkLaunchEnabled { get; set; } = false;

        public string LaunchArguments { get; set; } = "";

        // -----------------------------
        // GW1 – Mods / Injection
        // -----------------------------

        public bool Gw1ToolboxEnabled { get; set; } = false;
        public string Gw1ToolboxDllPath { get; set; } = "";

        public bool Gw1Py4GwEnabled { get; set; } = false;
        public string Gw1Py4GwDllPath { get; set; } = "";

        public bool Gw1GModEnabled { get; set; } = false;
        public string Gw1GModDllPath { get; set; } = "";
        public List<string> Gw1GModPluginPaths { get; set; } = new();

        // Optional / legacy list (used by some launch paths)
        public List<Gw1InjectedDll> Gw1InjectedDlls { get; set; } = new();

        // -----------------------------
        // GW1 – Auto-login
        // -----------------------------

        public bool Gw1AutoLoginEnabled { get; set; } = false;
        public string Gw1Email { get; set; } = "";
        public string Gw1PasswordProtected { get; set; } = ""; // base64; DPAPI-encrypted

        public bool Gw1AutoSelectCharacterEnabled { get; set; } = false;
        public string Gw1CharacterName { get; set; } = "";

        // Optional custom label used for GW1 window title renaming
        // If null or empty, Profile.Name is used. Null = use game's default titlebar.
        public string? Gw1WindowTitleLabel { get; set; }

        // -----------------------------
        // GW1 – Window Positioning
        // -----------------------------

        public bool WindowedModeEnabled { get; set; } = false;
        public int WindowX { get; set; } = 0;
        public int WindowY { get; set; } = 0;
        public int WindowWidth { get; set; } = 800;
        public int WindowHeight { get; set; } = 600;
        public bool WindowMaximized { get; set; } = false;

        public bool WindowRememberChanges { get; set; } = false;
        public bool WindowLockChanges { get; set; } = false; // Prevent resizing/moving
        public bool WindowBlockInputs { get; set; } = false; // Block minimize/close


        // -----------------------------
        // GW2 – Run-after programs
        // -----------------------------

        public bool Gw2RunAfterEnabled { get; set; } = false;
        public List<RunAfterProgram> Gw2RunAfterPrograms { get; set; } = new();
        public int Gw2MumbleSlot { get; set; } = 0;
        public string Gw2MumbleNameSuffix { get; set; } = "";

        // -----------------------------
        // GW2 – Auto-login / play
        // -----------------------------

        public bool Gw2AutoLoginEnabled { get; set; } = false;
        public string Gw2Email { get; set; } = "";
        public string Gw2PasswordProtected { get; set; } = ""; // base64; DPAPI-encrypted
        public bool Gw2AutoPlayEnabled { get; set; } = false;
        
        // When enabled, skips filling email/password and just presses Enter/clicks LOG IN
        // Use this when GW2's built-in "Remember Password" has saved your credentials
        public bool Gw2AutoSubmitLoginOnly { get; set; } = false;

        // Optional custom label used for GW2 window title renaming
        // If null or empty, Profile.Name is used. Null = use game's default titlebar.
        public string? Gw2WindowTitleLabel { get; set; }

        // -----------------------------
        // GW2 – Per-Profile Isolation
        // -----------------------------

        // When Gw2IsolationEnabled (global), each profile needs:
        // 1. IsolationGameFolderPath: dedicated folder containing Gw2-64.exe and Gw2.dat
        // 2. IsolationProfileRoot: root folder for redirected AppData (Roaming & Local)

        /// <summary>
        /// Path to this profile's dedicated GW2 game folder (must contain Gw2-64.exe and Gw2.dat).
        /// Required when GW2 isolation is enabled. Must be unique per profile.
        /// </summary>
        public string IsolationGameFolderPath { get; set; } = "";

        /// <summary>
        /// Root folder for this profile's isolated AppData.
        /// Default: %AppData%\Roaming\GWxLauncher\Profiles\{ProfileId}
        /// Will contain: Roaming\ and Local\ subdirectories.
        /// </summary>
        public string IsolationProfileRoot { get; set; } = "";

        // Helper to get default profile root path
        public string GetDefaultIsolationProfileRoot()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GWxLauncher",
                "Profiles",
                Id);
        }

        // -----------------------------
        // Display helpers
        // -----------------------------

        public override string ToString()
        {
            var prefix = GameType switch
            {
                GameType.GuildWars1 => "[GW1]",
                GameType.GuildWars2 => "[GW2]",
                _ => "[?]"
            };

            return $"{prefix} {Name}";
        }
    }

    // ------------------------------------------------------------
    // GW2 run-after program descriptor
    // ------------------------------------------------------------

    public class RunAfterProgram
    {
        public string Name { get; set; } = "";
        public string ExePath { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public bool PassMumbleLinkName { get; set; } = false;
        public override string ToString()
            => $"{(Enabled ? "✓" : "✗")} {Name}";
    }
}
