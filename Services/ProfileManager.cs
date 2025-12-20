using GWxLauncher.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
