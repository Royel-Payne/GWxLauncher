using System.Text;
using GWxLauncher.Domain;

namespace GWxLauncher.Services
{
    internal static class Gw2MumbleLinkService
    {
        /// <summary>
        /// Returns a deterministic MumbleLink shared-memory name for this profile:
        ///   MumbleLink_p{slot}_{suffix}
        ///
        /// Returns "" if the profile is not GW2 or if slot is not assigned (>0 required).
        /// </summary>
        public static string GetMumbleLinkName(GameProfile profile)
        {
            if (profile == null)
                return "";

            if (profile.GameType != GameType.GuildWars2)
                return "";

            if (profile.Gw2MumbleSlot <= 0)
                return "";

            // Use suffix if provided; otherwise fall back to Name for readability
            string suffixSource =
                string.IsNullOrWhiteSpace(profile.Gw2MumbleNameSuffix)
                    ? (profile.Name ?? "")
                    : profile.Gw2MumbleNameSuffix;

            string suffix = SanitizeSuffix(suffixSource);

            // If sanitize yields empty, fall back to slot-only
            if (string.IsNullOrWhiteSpace(suffix))
                return $"MumbleLink_p{profile.Gw2MumbleSlot}";

            return $"MumbleLink_p{profile.Gw2MumbleSlot}_{suffix}";
        }

        /// <summary>
        /// Sanitizes suffix for safe/consistent use in a name + command-line:
        /// - Allows letters, digits, underscore
        /// - Everything else becomes underscore
        /// - Collapses repeats
        /// - Trims leading/trailing underscores
        /// - Caps length for tray readability
        /// </summary>
        public static string SanitizeSuffix(string raw)
        {
            raw ??= "";
            raw = raw.Trim();

            if (raw.Length == 0)
                return "Profile";

            var sb = new StringBuilder(raw.Length);
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

            const int maxLen = 24;
            if (s.Length > maxLen)
                s = s.Substring(0, maxLen);

            return s;
        }
    }
}
