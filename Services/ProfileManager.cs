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

                    // Persist the newly assigned Ids so view eligibility can match across restarts
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
