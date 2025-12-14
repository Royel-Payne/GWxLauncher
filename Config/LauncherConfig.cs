using System.IO;
using System.Text.Json;

namespace GWxLauncher.Config
{
    internal class LauncherConfig
    {
        public string Gw1Path { get; set; } = "";
        public string Gw2Path { get; set; } = "";

        // 🔹 New: window placement
        public int WindowX { get; set; } = -1;
        public int WindowY { get; set; } = -1;
        public int WindowWidth { get; set; } = -1;
        public int WindowHeight { get; set; } = -1;
        public bool WindowMaximized { get; set; } = false;

        // 🔹 Profile Settings window placement
        public int ProfileSettingsX { get; set; } = -1;
        public int ProfileSettingsY { get; set; } = -1;
        public int ProfileSettingsWidth { get; set; } = -1;
        public int ProfileSettingsHeight { get; set; } = -1;

        private const string ConfigFileName = "launcherConfig.json";

        public static LauncherConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigFileName))
                    return new LauncherConfig();

                string json = File.ReadAllText(ConfigFileName);
                var config = JsonSerializer.Deserialize<LauncherConfig>(json);
                return config ?? new LauncherConfig();
            }
            catch
            {
                // If anything goes wrong, just use defaults
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
            File.WriteAllText(ConfigFileName, json);
        }
    }
}
