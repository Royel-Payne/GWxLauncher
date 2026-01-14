using GWxLauncher.Config;
using GWxLauncher.Services;
using GWxLauncher.UI;
using System.Diagnostics;
using Microsoft.Win32;

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

            // Check for .NET 8 Desktop Runtime (framework-dependent build)
            if (!IsNet8DesktopRuntimeInstalled())
            {
                ShowRuntimeRequiredDialog();
                return;
            }

            // Standard WinForms startup plumbing
            ApplicationConfiguration.Initialize();

            // Extract embedded native dependencies for GW2 isolation (single-file distribution)
            EmbeddedResourceExtractor.EnsureNativeDependencies();

            var cfg = LauncherConfig.Load();
            ThemeService.SetTheme(ParseTheme(cfg.Theme));

            Application.Run(new MainForm());

        }
        private static AppTheme ParseTheme(string? value)
        {
            var v = (value ?? "").Trim();

            if (string.Equals(v, "Dark", StringComparison.OrdinalIgnoreCase))
                return AppTheme.Dark;

            return AppTheme.Light;
        }

        /// <summary>
        /// Check if .NET 8.0 Desktop Runtime (x86) is installed by checking registry.
        /// Framework-dependent builds require the runtime to be installed separately.
        /// </summary>
        private static bool IsNet8DesktopRuntimeInstalled()
        {
            try
            {
                // Check for .NET 8.0 Desktop Runtime in registry (x86 or x64)
                // Registry path: HKLM\SOFTWARE\dotnet\Setup\InstalledVersions\x86\sharedhost or x64
                // We look for WindowsDesktop shared framework version 8.x
                
                var basePaths = new[]
                {
                    @"SOFTWARE\dotnet\Setup\InstalledVersions\x86\sharedfx\Microsoft.WindowsDesktop.App",
                    @"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App"
                };

                foreach (var path in basePaths)
                {
                    using var key = Registry.LocalMachine.OpenSubKey(path);
                    if (key != null)
                    {
                        // Check for any 8.x version
                        var versions = key.GetValueNames();
                        if (versions.Any(v => v.StartsWith("8.")))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                // If registry check fails, assume runtime is present (avoid blocking users)
                return true;
            }
        }

        /// <summary>
        /// Show a friendly dialog prompting user to install .NET 8 Desktop Runtime.
        /// </summary>
        private static void ShowRuntimeRequiredDialog()
        {
            var result = MessageBox.Show(
                "GWxLauncher requires .NET 8 Desktop Runtime to run.\n\n" +
                "This is a free, one-time download (~50 MB) from Microsoft.\n\n" +
                "Click OK to download and install it now, or Cancel to exit.",
                "Runtime Required - GWxLauncher",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                try
                {
                    // Open the official Microsoft download page for .NET 8 Desktop Runtime (x86)
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x86.exe",
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show(
                        "Could not open download link. Please visit:\n\n" +
                        "https://dotnet.microsoft.com/download/dotnet/8.0\n\n" +
                        "and download '.NET Desktop Runtime 8.0 (x86)'.",
                        "Download Link",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }
    }
}
