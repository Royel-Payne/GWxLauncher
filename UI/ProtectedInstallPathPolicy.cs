namespace GWxLauncher.Services
{
    internal static class ProtectedInstallPathPolicy
    {
        public static bool IsProtectedPath(string exePath)
        {
            if (string.IsNullOrWhiteSpace(exePath))
                return false;

            string full;
            try { full = Path.GetFullPath(exePath); }
            catch { return false; }

            return
                IsUnder(full, @"C:\Program Files") ||
                IsUnder(full, @"C:\Program Files (x86)");
        }

        private static bool IsUnder(string fullPath, string root)
        {
            if (string.IsNullOrWhiteSpace(root))
                return false;

            string rootFull;
            try { rootFull = Path.GetFullPath(root); }
            catch { return false; }

            if (!rootFull.EndsWith(Path.DirectorySeparatorChar))
                rootFull += Path.DirectorySeparatorChar;

            return fullPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase);
        }

        public static string BuildWarningMessage(string exePath)
        {
            return
                "This executable is located under Program Files, which is a protected Windows directory." + Environment.NewLine + Environment.NewLine +
                "GWxLauncher advanced features (multi-client, injection, automation) may fail or behave unpredictably " +
                "when the game is installed there." + Environment.NewLine + Environment.NewLine +
                "Recommended:" + Environment.NewLine +
                "Install or move the game to a user-writable folder (for example: C:\\Games\\Guild Wars\\)." + Environment.NewLine + Environment.NewLine +
                "Detected executable path:" + Environment.NewLine +
                exePath;
        }

        // Non-fatal diagnostic note for post-launch troubleshooting.
        public static void TryAppendLaunchReportNote(LaunchReport? report, string exePath)
        {
            if (report == null)
                return;

            if (!IsProtectedPath(exePath))
                return;

            try
            {
                var step = new LaunchStep
                {
                    Label = "ProtectedInstallPathDetected",
                    Outcome = StepOutcome.Success,
                    Detail = "Executable is under Program Files / Program Files (x86). Warning-only."
                };

                report.Steps.Add(step);
            }
            catch
            {
                // Best-effort only. We never want this policy note to break launching.
            }
        }
    }
}
