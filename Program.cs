using GWxLauncher.Config;
using GWxLauncher.Services;
using GWxLauncher.UI;

namespace GWxLauncher
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Helper mode: run elevated to clear the GW2 mutex, then exit.
            // This avoids trying to elevate the whole UI app.
            if (args.Length == 1 && string.Equals(args[0], "--gw2-kill-mutex", StringComparison.OrdinalIgnoreCase))
            {
                var ok = Gw2MutexKiller.TryKillGw2Mutex(
                    out int clearedPid,
                    out string detail,
                    allowElevatedFallback: false,
                    out bool usedElevated);

                // Exit code is how the non-elevated parent knows whether it worked.
                Environment.ExitCode = ok ? 0 : 1;
                return;
            }

            // Standard WinForms startup plumbing
            ApplicationConfiguration.Initialize();

            var cfg = LauncherConfig.Load();
            ThemeService.SetTheme(ParseTheme(cfg.Theme));

            Application.Run(new MainForm());

        }
        private static AppTheme ParseTheme(string? value)
        {
            if (string.Equals(value, "Light", StringComparison.OrdinalIgnoreCase))
                return AppTheme.Light;

            // Default + fallback
            return AppTheme.Dark;
        }
    }
}
