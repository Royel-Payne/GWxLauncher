using System.Text.Json;
using System.Text.Json.Serialization;
using GWxLauncher.Config;
using GWxLauncher.Domain;

namespace GWxLauncher.Services
{
    public class ProfileManager
    {
        private readonly List<GameProfile> _profiles = new();

        private const string ProfilesFileName = "profiles.json";

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public IReadOnlyList<GameProfile> Profiles => _profiles;

        public void AddProfile(GameProfile profile)
        {
            if (profile != null)
                _profiles.Add(profile);
        }

        public GameProfile CopyProfile(GameProfile source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // Generate a unique display name: "Name (Copy)", "(Copy 2)", etc.
            string baseName = $"{source.Name} (Copy)";
            string name = baseName;
            int i = 2;

            while (_profiles.Any(p =>
                string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase)))
            {
                name = $"{baseName} {i++}";
            }

            var copy = new GameProfile
            {
                // Identity
                Id = Guid.NewGuid().ToString("N"),
                Name = name,
                GameType = source.GameType,
                ExecutablePath = source.ExecutablePath,
                BulkLaunchEnabled = source.BulkLaunchEnabled,

                // ---- GW1 flags (paths intentionally NOT copied) ----
                Gw1ToolboxEnabled = source.Gw1ToolboxEnabled,
                Gw1ToolboxDllPath = LauncherConfig.Load().LastToolboxPath ?? "",

                Gw1Py4GwEnabled = source.Gw1Py4GwEnabled,
                Gw1Py4GwDllPath = LauncherConfig.Load().LastPy4GWPath ?? "",

                Gw1GModEnabled = source.Gw1GModEnabled,
                Gw1GModDllPath = LauncherConfig.Load().LastGModPath ?? "",
                Gw1GModPluginPaths = new List<string>(),

                Gw1InjectedDlls = new List<Gw1InjectedDll>(),

                // ---- GW1 Auto-login (copied) ----
                Gw1AutoLoginEnabled = source.Gw1AutoLoginEnabled,
                Gw1Email = source.Gw1Email,
                Gw1PasswordProtected = source.Gw1PasswordProtected,
                Gw1AutoSelectCharacterEnabled = source.Gw1AutoSelectCharacterEnabled,
                Gw1CharacterName = source.Gw1CharacterName,

                // ---- GW2 Auto-login / play (copied) ----
                Gw2AutoLoginEnabled = source.Gw2AutoLoginEnabled,
                Gw2Email = source.Gw2Email,
                Gw2PasswordProtected = source.Gw2PasswordProtected,
                Gw2AutoPlayEnabled = source.Gw2AutoPlayEnabled,

                // ---- GW2 ----
                Gw2RunAfterEnabled = source.Gw2RunAfterEnabled,
                Gw2RunAfterPrograms = source.Gw2RunAfterPrograms
                    .Select(p => new RunAfterProgram
                    {
                        Name = p.Name,
                        ExePath = p.ExePath,
                        Enabled = p.Enabled,
                        PassMumbleLinkName = p.PassMumbleLinkName
                    })
                    .ToList(),

                Gw2MumbleSlot = 0,               // will be re-assigned deterministically
                Gw2MumbleNameSuffix = ""          // regenerated from name
            };

            AddProfile(copy);

            // Re-run GW2 identity assignment logic to avoid collisions
            EnsureGw2MumbleIdentity(copy);

            Save();

            return copy;
        }

        public void RemoveProfile(GameProfile profile)
        {
            if (profile != null)
                _profiles.Remove(profile);
        }

        public void Load()
        {
            try
            {
                string path = GetProfilesPath();
                MigrateLegacyProfilesFile(path);

                if (!File.Exists(path))
                    return;

                string json = File.ReadAllText(path);
                var loaded = JsonSerializer.Deserialize<List<GameProfile>>(json, _jsonOptions);
                if (loaded != null)
                {
                    _profiles.Clear();
                    _profiles.AddRange(loaded);

                    // Ensure all profiles have a stable Id (older profiles.json may not have this field)
                    bool changed = false;
                    foreach (var p in _profiles)
                    {
                        if (string.IsNullOrWhiteSpace(p.Id))
                        {
                            p.Id = Guid.NewGuid().ToString("N");
                            changed = true;
                        }
                    }

                    // Ensure GW2 profiles have deterministic Mumble identity (slot + default suffix)
                    changed |= EnsureGw2MumbleIdentityForAllProfiles();

                    // Persist any migrations so identity remains stable across restarts
                    if (changed)
                        Save();
                }
            }
            catch
            {
                // For now we fail silently and just start with an empty list.
            }
        }

        public void Save()
        {
            try
            {
                string path = GetProfilesPath();
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                string json = JsonSerializer.Serialize(_profiles, _jsonOptions);
                File.WriteAllText(path, json);
            }
            catch
            {
                // Later we can surface this via status label or logging.
            }
        }
        private void EnsureGw2MumbleIdentity(GameProfile profile)
        {
            if (profile.GameType != GameType.GuildWars2)
                return;

            // Slot: assign if missing/invalid or colliding
            if (profile.Gw2MumbleSlot <= 0 || _profiles.Any(p =>
                    p != profile &&
                    p.GameType == GameType.GuildWars2 &&
                    p.Gw2MumbleSlot == profile.Gw2MumbleSlot))
            {
                profile.Gw2MumbleSlot = FindNextAvailableGw2MumbleSlot();
            }

            // Suffix: default to sanitized profile name if empty
            if (string.IsNullOrWhiteSpace(profile.Gw2MumbleNameSuffix))
            {
                profile.Gw2MumbleNameSuffix = SanitizeMumbleSuffix(profile.Name);
            }
        }

        private bool EnsureGw2MumbleIdentityForAllProfiles()
        {
            bool changed = false;

            // Track used slots; preserve first-come slots, fix missing/duplicates deterministically.
            var used = new HashSet<int>();

            foreach (var p in _profiles.Where(p => p.GameType == GameType.GuildWars2))
            {
                // Fix missing/invalid slot
                if (p.Gw2MumbleSlot <= 0)
                {
                    p.Gw2MumbleSlot = FindNextAvailableGw2MumbleSlot(used);
                    changed = true;
                }
                else if (used.Contains(p.Gw2MumbleSlot))
                {
                    // Duplicate: reassign deterministically
                    p.Gw2MumbleSlot = FindNextAvailableGw2MumbleSlot(used);
                    changed = true;
                }

                used.Add(p.Gw2MumbleSlot);

                // Default suffix if missing
                if (string.IsNullOrWhiteSpace(p.Gw2MumbleNameSuffix))
                {
                    p.Gw2MumbleNameSuffix = SanitizeMumbleSuffix(p.Name);
                    changed = true;
                }
            }

            return changed;
        }

        private int FindNextAvailableGw2MumbleSlot()
        {
            var used = new HashSet<int>(
                _profiles
                    .Where(p => p.GameType == GameType.GuildWars2 && p.Gw2MumbleSlot > 0)
                    .Select(p => p.Gw2MumbleSlot));

            return FindNextAvailableGw2MumbleSlot(used);
        }

        private static int FindNextAvailableGw2MumbleSlot(HashSet<int> used)
        {
            int slot = 1;
            while (used.Contains(slot))
                slot++;

            return slot;
        }

        private static string SanitizeMumbleSuffix(string? raw)
        {
            raw ??= "";
            raw = raw.Trim();

            if (raw.Length == 0)
                return "Profile";

            // Keep [A-Za-z0-9_] only; convert whitespace and punctuation to underscores.
            var sb = new System.Text.StringBuilder(raw.Length);
            bool lastUnderscore = false;

            foreach (char c in raw)
            {
                bool ok =
                    (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9') ||
                    c == '_';

                if (ok)
                {
                    sb.Append(c);
                    lastUnderscore = false;
                }
                else
                {
                    if (!lastUnderscore)
                    {
                        sb.Append('_');
                        lastUnderscore = true;
                    }
                }
            }

            var s = sb.ToString().Trim('_');
            if (s.Length == 0)
                s = "Profile";

            // Optional: keep it readable in tray titles etc.
            const int maxLen = 24;
            if (s.Length > maxLen)
                s = s.Substring(0, maxLen);

            return s;
        }

        private static string GetProfilesPath()
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(root, "GWxLauncher", ProfilesFileName);
        }

        private static void MigrateLegacyProfilesFile(string appDataPath)
        {
            // Legacy file location (working directory)
            string legacyPath = Path.Combine(AppContext.BaseDirectory, ProfilesFileName);

            // Only migrate if legacy exists and appdata doesn't
            if (!File.Exists(appDataPath) && File.Exists(legacyPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(appDataPath)!);
                File.Copy(legacyPath, appDataPath, overwrite: false);
            }
        }
    }
}
