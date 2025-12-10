namespace GWxLauncher
{
    public class GameProfile
    {
        public string Name { get; set; } = "";
        public GameType GameType { get; set; }

        // Each profile's own executable path (may be blank, we fall back to config)
        public string ExecutablePath { get; set; } = "";

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
