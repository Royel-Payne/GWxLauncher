using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        public void RemoveProfile(GameProfile profile)
        {
            if (profile != null)
                _profiles.Remove(profile);
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(ProfilesFileName))
                    return;

                string json = File.ReadAllText(ProfilesFileName);
                var loaded = JsonSerializer.Deserialize<List<GameProfile>>(json, _jsonOptions);
                if (loaded != null)
                {
                    _profiles.Clear();
                    _profiles.AddRange(loaded);
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
                string json = JsonSerializer.Serialize(_profiles, _jsonOptions);
                File.WriteAllText(ProfilesFileName, json);
            }
            catch
            {
                // Later we can surface this via status label or logging.
            }
        }
    }
}
