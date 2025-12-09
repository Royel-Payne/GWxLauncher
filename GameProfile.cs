namespace GWxLauncher
{
    public class GameProfile
    {
        public string Name { get; set; } = "";
        public GameType GameType { get; set; }

        // Later we’ll add per-account stuff:
        // - login info (email/etc.)
        // - Toolbox/Blish toggles
        // - favorite character
        // - etc.
    }
}
