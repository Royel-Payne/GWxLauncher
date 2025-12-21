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
