using System.Collections.Generic;
using System;

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

        // Per-profile executable path (optional override)
        public string ExecutablePath { get; set; } = "";

        // Bulk launch eligibility (explicit, per-profile)
        public bool BulkLaunchEnabled { get; set; } = false;
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        // GW1 mods – Toolbox (already implemented)
        public bool Gw1ToolboxEnabled { get; set; } = false;
        public string Gw1ToolboxDllPath { get; set; } = "";

        // GW1 mods – Py4GW (Phase 3.6 will make this do something)
        public bool Gw1Py4GwEnabled { get; set; } = false;
        public string Gw1Py4GwDllPath { get; set; } = "";

        // GW1 mods – gMod (Phase 3.7 will implement early injection)
        public bool Gw1GModEnabled { get; set; } = false;
        public string Gw1GModDllPath { get; set; } = "";
        public List<string> Gw1GModPluginPaths { get; set; } = new();

        // General-purpose list, still available for experiments / future
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
        // GW2 helpers – companion programs (e.g. Blish HUD)
        public bool Gw2RunAfterEnabled { get; set; } = false;
        public List<RunAfterProgram> Gw2RunAfterPrograms { get; set; } = new();
    }
    public class RunAfterProgram
    {
        public string Name { get; set; } = "";
        public string ExePath { get; set; } = "";
        public bool Enabled { get; set; } = true;

        public override string ToString() => $"{(Enabled ? "✓" : "✗")} {Name}";
    }
}
