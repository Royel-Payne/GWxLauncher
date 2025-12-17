using System.Collections.Generic;
using System;

namespace GWxLauncher.Domain
{
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
        public string ExecutablePath { get; set; } = "";
        public bool BulkLaunchEnabled { get; set; } = false;
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public bool Gw1ToolboxEnabled { get; set; } = false;
        public string Gw1ToolboxDllPath { get; set; } = "";
        public bool Gw1Py4GwEnabled { get; set; } = false;
        public string Gw1Py4GwDllPath { get; set; } = "";
        public bool Gw1GModEnabled { get; set; } = false;
        public string Gw1GModDllPath { get; set; } = "";
        public List<string> Gw1GModPluginPaths { get; set; } = new();
        public bool Gw1AutoLoginEnabled { get; set; } = false;
        public string Gw1Email { get; set; } = "";
        public string Gw1PasswordProtected { get; set; } = ""; // base64; DPAPI-encrypted
        public bool Gw1AutoSelectCharacterEnabled { get; set; } = false;
        public string Gw1CharacterName { get; set; } = "";      
        
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
        public bool Gw2RunAfterEnabled { get; set; } = false;
        public List<RunAfterProgram> Gw2RunAfterPrograms { get; set; } = new();
        public bool Gw2AutoLoginEnabled { get; set; } = false;
        public string Gw2Email { get; set; } = "";
        public string Gw2PasswordProtected { get; set; } = ""; // base64; DPAPI-encrypted
        public bool Gw2AutoPlayEnabled { get; set; } = false;
    }
    public class RunAfterProgram
    {
        public string Name { get; set; } = "";
        public string ExePath { get; set; } = "";
        public bool Enabled { get; set; } = true;

        public override string ToString() => $"{(Enabled ? "✓" : "✗")} {Name}";
    }
}
