using System.Collections.Generic;

namespace GWxLauncher.Domain
{
    // Represents one GW1 DLL this profile can inject (e.g. Toolbox, gMod, etc.)
    public class Gw1InjectedDll
    {
        public string Name { get; set; } = "";   // "Toolbox", "gMod", etc.
        public string Path { get; set; } = "";   // Full path to the DLL
        public bool Enabled { get; set; } = true;

        public override string ToString()
            => $"{(Enabled ? "✓" : "✗")} {Name} ({Path})";
    }

    public class GameProfile
    {
        public string Name { get; set; } = "";
        public GameType GameType { get; set; }

        // Each profile's own executable path (may be blank, we fall back to config)
        public string ExecutablePath { get; set; } = "";

        // GW1-only: optional Toolbox injection
        public bool Gw1ToolboxEnabled { get; set; } = false;
        public string Gw1ToolboxDllPath { get; set; } = "";

        // 🔹 New: general-purpose list of DLLs this profile can inject
        public List<Gw1InjectedDll> Gw1InjectedDlls { get; set; } = new();

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
}
