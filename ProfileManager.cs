using System.Collections.Generic;

namespace GWxLauncher
{
    public class ProfileManager
    {
        private readonly List<GameProfile> _profiles = new();

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
    }
}
