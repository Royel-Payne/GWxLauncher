using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GWxLauncher.Config;
using GWxLauncher.Domain;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Imports GW1 accounts.json into profiles.json + views.json.
    /// One-way import: always creates NEW profiles with new Ids.
    /// </summary>
    internal sealed class AccountsJsonImportService
    {
        internal sealed class ImportResult
        {
            public int ImportedCount { get; set; }
            public bool MissingToolboxPath { get; set; }
            public bool MissingGModPath { get; set; }
            public bool MissingPy4GwPath { get; set; }

            // Keep collections mutable, but do NOT reassign the properties.
            public List<string> ImportedProfileIds { get; } = new();
            public Dictionary<string, ToolWants> ToolWantsByProfileId { get; } = new();
        }

        internal sealed class ToolWants
        {
            public bool WantsToolbox { get; init; }
            public bool WantsGMod { get; init; }
            public bool WantsPy4Gw { get; init; }
        }

        private sealed class AccountDto
        {
            public string? character_name { get; set; }
            public string? email { get; set; }
            public string? password { get; set; }
            public string? gw_client_name { get; set; }
            public string? gw_path { get; set; }
            public string? extra_args { get; set; }

            public bool inject_py4gw { get; set; }
            public bool inject_blackbox { get; set; }
            public bool inject_gmod { get; set; }

            public List<string>? gmod_mods { get; set; }
        }

        public ImportResult ImportFromFile(string accountsJsonPath, LauncherConfig cfg)
        {
            if (string.IsNullOrWhiteSpace(accountsJsonPath))
                throw new ArgumentException("accountsJsonPath is required", nameof(accountsJsonPath));

            if (!File.Exists(accountsJsonPath))
                throw new FileNotFoundException("accounts.json not found", accountsJsonPath);

            var json = File.ReadAllText(accountsJsonPath);
            var root = JsonSerializer.Deserialize<Dictionary<string, List<AccountDto>>>(json);

            if (root == null)
                throw new InvalidOperationException("accounts.json deserialized null");

            var pm = new ProfileManager();
            pm.Load();

            var views = new ViewStateStore();
            views.Load();

            // Ensure views exist and showCheckedOnly = false for imported views.
            var oldActive = views.ActiveViewName;
            foreach (var viewName in root.Keys.Where(k => !string.IsNullOrWhiteSpace(k)))
            {
                views.SetActiveView(viewName.Trim());
                views.SetShowCheckedOnly(viewName.Trim(), false);
            }
            views.SetActiveView(oldActive);

            // Track name collisions
            var existingNames = new HashSet<string>(
                pm.Profiles.Select(p => p.Name ?? ""),
                StringComparer.CurrentCultureIgnoreCase);

            // Determine which global DLL paths are valid
            string toolboxPath = (cfg.LastToolboxPath ?? "").Trim();
            string gmodPath = (cfg.LastGModPath ?? "").Trim();
            string py4gwPath = (cfg.LastPy4GWPath ?? "").Trim();

            bool toolboxOk = !string.IsNullOrWhiteSpace(toolboxPath) && File.Exists(toolboxPath);
            bool gmodOk = !string.IsNullOrWhiteSpace(gmodPath) && File.Exists(gmodPath);
            bool py4gwOk = !string.IsNullOrWhiteSpace(py4gwPath) && File.Exists(py4gwPath);

            bool missingToolbox = false;
            bool missingGmod = false;
            bool missingPy4gw = false;

            var result = new ImportResult();

            foreach (var kv in root)
            {
                var viewName = (kv.Key ?? "").Trim();
                if (string.IsNullOrWhiteSpace(viewName))
                    continue;

                var accounts = kv.Value ?? new List<AccountDto>();

                foreach (var a in accounts)
                {
                    // Skip obviously broken records
                    var exe = NormalizePath(a.gw_path);
                    if (string.IsNullOrWhiteSpace(exe))
                        continue;

                    // Name selection: gw_client_name → character_name → email prefix → "GW1 Account"
                    string baseName =
                        FirstNonEmpty(a.gw_client_name, a.character_name, EmailPrefix(a.email), "GW1 Account");

                    string finalName = MakeUniqueImportedName(baseName, existingNames);
                    existingNames.Add(finalName);

                    // Tool wants (NOTE: PROPOSED mapping inject_blackbox -> toolbox)
                    var wantsToolbox = a.inject_blackbox;
                    var wantsGmod = a.inject_gmod;
                    var wantsPy4gw = a.inject_py4gw;

                    if (wantsToolbox && !toolboxOk) missingToolbox = true;
                    if (wantsGmod && !gmodOk) missingGmod = true;
                    if (wantsPy4gw && !py4gwOk) missingPy4gw = true;

                    // Build profile
                    var p = new GameProfile
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Name = finalName,
                        GameType = GameType.GuildWars1,
                        ExecutablePath = exe,
                        BulkLaunchEnabled = false,

                        // Auto-fill tool paths if known-good exists
                        Gw1ToolboxDllPath = toolboxOk ? toolboxPath : "",
                        Gw1GModDllPath = gmodOk ? gmodPath : "",
                        Gw1Py4GwDllPath = py4gwOk ? py4gwPath : "",

                        // Enable only if source wants it AND path exists (hard rule)
                        Gw1ToolboxEnabled = wantsToolbox && toolboxOk,
                        Gw1GModEnabled = wantsGmod && gmodOk,
                        Gw1Py4GwEnabled = wantsPy4gw && py4gwOk,

                        // gMod plugin paths: import only existing files
                        Gw1GModPluginPaths = (a.gmod_mods ?? new List<string>())
                            .Select(NormalizePath)
                            .Where(s => !string.IsNullOrWhiteSpace(s) && File.Exists(s))
                            .ToList()
                    };

                    // Credentials: only persist password if it isn't obviously a placeholder.
                    // (Your sample file uses "redacted".):contentReference[oaicite:13]{index=13}
                    var email = (a.email ?? "").Trim();
                    var pw = (a.password ?? "").Trim();

                    if (!string.IsNullOrWhiteSpace(email))
                        p.Gw1Email = email;

                    if (!string.IsNullOrWhiteSpace(email) &&
                        !string.IsNullOrWhiteSpace(pw) &&
                        !string.Equals(pw, "redacted", StringComparison.OrdinalIgnoreCase))
                    {
                        p.Gw1PasswordProtected = DpapiProtector.ProtectToBase64(pw);
                        p.Gw1AutoLoginEnabled = true;
                    }
                    else
                    {
                        p.Gw1AutoLoginEnabled = false;
                        p.Gw1PasswordProtected = "";
                    }

                    // Character: import name; auto-select only if present.
                    var charName = (a.character_name ?? "").Trim();
                    p.Gw1CharacterName = charName;
                    p.Gw1AutoSelectCharacterEnabled = !string.IsNullOrWhiteSpace(charName) && p.Gw1AutoLoginEnabled;

                    pm.AddProfile(p);
                    result.ImportedCount++;
                    result.ImportedProfileIds.Add(p.Id);

                    result.ToolWantsByProfileId[p.Id] = new ToolWants
                    {
                        WantsToolbox = wantsToolbox,
                        WantsGMod = wantsGmod,
                        WantsPy4Gw = wantsPy4gw
                    };
                }
            }

            // Imported profiles start unchecked in ALL views.
            foreach (var view in views.ViewNames)
            {
                foreach (var id in result.ImportedProfileIds)
                {
                    views.SetEligible(view, id, false);
                }
            }

            pm.Save();
            views.Save();

            result.MissingToolboxPath = missingToolbox;
            result.MissingGModPath = missingGmod;
            result.MissingPy4GwPath = missingPy4gw;

            return result;
        }

        public void ApplyNewlySelectedDllPathsToImportedProfiles(
            IEnumerable<string> importedProfileIds,
            Dictionary<string, ToolWants> toolWantsByProfileId,
            LauncherConfig cfg)
        {
            var ids = new HashSet<string>(importedProfileIds ?? Enumerable.Empty<string>());
            if (ids.Count == 0)
                return;

            var pm = new ProfileManager();
            pm.Load();

            string toolboxPath = (cfg.LastToolboxPath ?? "").Trim();
            string gmodPath = (cfg.LastGModPath ?? "").Trim();
            string py4gwPath = (cfg.LastPy4GWPath ?? "").Trim();

            bool toolboxOk = !string.IsNullOrWhiteSpace(toolboxPath) && File.Exists(toolboxPath);
            bool gmodOk = !string.IsNullOrWhiteSpace(gmodPath) && File.Exists(gmodPath);
            bool py4gwOk = !string.IsNullOrWhiteSpace(py4gwPath) && File.Exists(py4gwPath);

            foreach (var p in pm.Profiles.Where(p => ids.Contains(p.Id)))
            {
                if (toolWantsByProfileId.TryGetValue(p.Id, out var wants))
                {
                    if (toolboxOk && string.IsNullOrWhiteSpace(p.Gw1ToolboxDllPath))
                        p.Gw1ToolboxDllPath = toolboxPath;
                    if (gmodOk && string.IsNullOrWhiteSpace(p.Gw1GModDllPath))
                        p.Gw1GModDllPath = gmodPath;
                    if (py4gwOk && string.IsNullOrWhiteSpace(p.Gw1Py4GwDllPath))
                        p.Gw1Py4GwDllPath = py4gwPath;

                    // Re-enable where source wanted it and the path now exists
                    p.Gw1ToolboxEnabled = wants.WantsToolbox && toolboxOk;
                    p.Gw1GModEnabled = wants.WantsGMod && gmodOk;
                    p.Gw1Py4GwEnabled = wants.WantsPy4Gw && py4gwOk;
                }
            }

            pm.Save();
        }

        private static string NormalizePath(string? raw)
        {
            raw ??= "";
            raw = raw.Trim();
            if (raw.Length == 0)
                return "";

            // accounts.json uses forward slashes; normalize for Windows.
            return raw.Replace('/', '\\');
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            foreach (var v in values)
            {
                var s = (v ?? "").Trim();
                if (s.Length > 0)
                    return s;
            }
            return "";
        }

        private static string EmailPrefix(string? email)
        {
            var e = (email ?? "").Trim();
            if (e.Length == 0)
                return "";
            int at = e.IndexOf('@');
            return at > 0 ? e.Substring(0, at) : e;
        }

        private static string MakeUniqueImportedName(string baseName, HashSet<string> used)
        {
            // Rules:
            // - if conflict: "Name (Imported)", then "(Imported 2)" etc.
            if (!used.Contains(baseName))
                return baseName;

            string n1 = $"{baseName} (Imported)";
            if (!used.Contains(n1))
                return n1;

            int i = 2;
            while (true)
            {
                string n = $"{baseName} (Imported {i})";
                if (!used.Contains(n))
                    return n;
                i++;
            }
        }
    }
}
