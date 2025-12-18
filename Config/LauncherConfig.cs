using System.IO;
using System.Text.Json;
using System;

namespace GWxLauncher.Config
{
    internal class LauncherConfig
    {
        public string Gw1Path { get; set; } = "";
        public string Gw2Path { get; set; } = "";

        public bool Gw1MulticlientEnabled { get; set; } = true;
        public bool Gw2MulticlientEnabled { get; set; } = true;

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
