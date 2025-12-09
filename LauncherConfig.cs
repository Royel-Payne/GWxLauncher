using System.Text.Json;
using System.IO;   // <- optional if you don't already have implicit usings

namespace GWxLauncher
{
    internal class LauncherConfig
    {
        public string Gw1Path { get; set; } = "";
        public string Gw2Path { get; set; } = "";

        private const string ConfigFileName = "launcherConfig.json";

        public static LauncherConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigFileName))
                {
                    return new LauncherConfig();
                }

                string json = File.ReadAllText(ConfigFileName);
                return JsonSerializer.Deserialize<LauncherConfig>(json)
                       ?? new LauncherConfig();
            }
            catch
            {
                // If JSON is broken, return a blank config
                return new LauncherConfig();
            }
        }

        // 🔹 NEW: Save current settings back to launcherConfig.json
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
