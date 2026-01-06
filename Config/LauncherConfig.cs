using System.Text.Json;

namespace GWxLauncher.Config
{
    internal class LauncherConfig
    {
        public string Gw1Path { get; set; } = "";
        public string Gw2Path { get; set; } = "";

        // -----------------------------
        // GW1 tool paths (last known good)
        // -----------------------------
        // These are NOT "defaults" in the UI — just remembered last working paths.
        public string LastToolboxPath { get; set; } = "";
        public string LastGModPath { get; set; } = "";
        public string LastPy4GWPath { get; set; } = "";

        // Allowed values: "Dark" or "Light"
        public string Theme { get; set; } = "Light";

        public bool Gw1MulticlientEnabled { get; set; } = true;
        public bool Gw2MulticlientEnabled { get; set; } = true;
        // -----------------------------
        // GW1 window title renaming
        // -----------------------------
        public bool Gw1WindowTitleEnabled { get; set; } = false;
        // Template supports: {ProfileName}
        public string Gw1WindowTitleTemplate { get; set; } = "GW1 · {ProfileName}";
        // -----------------------------
        // Global mod kill-switches
        // -----------------------------

        // These gate injection at launch time.
        // They do NOT modify per-profile settings.
        public bool GlobalToolboxEnabled { get; set; } = true;
        public bool GlobalPy4GwEnabled { get; set; } = true;
        public bool GlobalGModEnabled { get; set; } = true;

        // Bulk throttling (advanced-only)
        // NOTE: Do not clamp here. Clamp is applied in BulkLaunchThrottlingPolicy (0–60 seconds).
        public int Gw1BulkLaunchDelaySeconds { get; set; } = 15;
        public int Gw2BulkLaunchDelaySeconds { get; set; } = 15;

        // New: window placement
        public int WindowX { get; set; } = -1;
        public int WindowY { get; set; } = -1;
        public int WindowWidth { get; set; } = -1;
        public int WindowHeight { get; set; } = -1;
        public bool WindowMaximized { get; set; } = false;

        // Profile Settings window placement
        public int ProfileSettingsX { get; set; } = -1;
        public int ProfileSettingsY { get; set; } = -1;
        public int ProfileSettingsWidth { get; set; } = -1;
        public int ProfileSettingsHeight { get; set; } = -1;

        // Global Settings window placement
        public int GlobalSettingsX { get; set; } = -1;
        public int GlobalSettingsY { get; set; } = -1;
        public int GlobalSettingsWidth { get; set; } = -1;
        public int GlobalSettingsHeight { get; set; } = -1;


        private static readonly string ConfigFilePath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GWxLauncher",
                "launcherConfig.json");


        public static LauncherConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                    return new LauncherConfig();

                string json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<LauncherConfig>(json);
                return config ?? new LauncherConfig();
            }
            catch
            {
                return new LauncherConfig();
            }
        }


        public void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(this, options);

            var dir = Path.GetDirectoryName(ConfigFilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            File.WriteAllText(ConfigFilePath, json);
        }
    }
}
