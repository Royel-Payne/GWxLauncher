using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using GWxLauncher.Domain;
using static GWxLauncher.Services.NativeMethods;

namespace GWxLauncher.Services
{
    internal static class Gw2WindowManagementService
    {
        private const int DefaultFindTimeoutMs = 15000;
        private const int EnforcementDurationMs = 7000;
        private const int EnforcementIntervalMs = 100;

        // Known 3D (post-PLAY) GW2 window classes
        private static readonly string[] DxWindowClasses =
        {
            "ArenaNet_Dx_Window_Class",
            "ArenaNet_Gr_Window_Class",
        };

        public static void ApplyWindowSettings(Process process, GameProfile profile, LaunchReport? report = null)
        {
            if (process == null || profile == null)
                return;

            if (!profile.WindowedModeEnabled)
                return;

            if (profile.WindowWidth <= 0 || profile.WindowHeight <= 0)
                return;

            // Run in the background so GW2LaunchOrchestrator stays non-blocking.
            _ = Task.Run(() =>
            {
                try
                {
                    var step = report != null ? new LaunchStep { Label = "Window sizing" } : null;
                    if (report != null && step != null)
                        report.Steps.Add(step);

                    // IMPORTANT: don’t resize the launcher/login window (class "ArenaNet").
                    // Only apply after the 3D window exists (post-PLAY).
                    IntPtr hwnd = FindGw2DxWindow(process, timeoutMs: DefaultFindTimeoutMs, out string foundClass, out int waitedMs);
                    if (hwnd == IntPtr.Zero)
                    {
                        if (step != null)
                        {
                            step.Outcome = StepOutcome.Pending;
                            step.Detail = $"GW2 DX window not found within {waitedMs}ms (best-effort).";
                        }
                        return;
                    }

                    EnforcePlacement(process, hwnd, profile, foundClass, step);
                }
                catch
                {
                    // best-effort only
                }
            });
        }

        private static void EnforcePlacement(Process process, IntPtr hwnd, GameProfile profile, string initialClass, LaunchStep? step)
        {
            var targetRect = new RECT
            {
                Left = profile.WindowX,
                Top = profile.WindowY,
                Right = profile.WindowX + profile.WindowWidth,
                Bottom = profile.WindowY + profile.WindowHeight
            };

            bool targetMaximized = profile.WindowMaximized;

            var sw = Stopwatch.StartNew();
            int applied = 0;
            int mismatches = 0;
            string lastClass = initialClass;

            while (!process.HasExited && sw.ElapsedMilliseconds < EnforcementDurationMs)
            {
                // GW2 can recreate windows during startup; re-find if needed.
                if (hwnd == IntPtr.Zero || !IsWindow(hwnd) || !IsWindowVisible(hwnd))
                {
                    hwnd = FindGw2DxWindow(process, timeoutMs: 0, out lastClass, out _);
                }

                if (hwnd != IntPtr.Zero)
                {
                    var placement = new WINDOWPLACEMENT { length = Marshal.SizeOf(typeof(WINDOWPLACEMENT)) };

                    if (GetWindowPlacement(hwnd, ref placement))
                    {
                        bool isMaximized = placement.showCmd == SW_MAXIMIZE;
                        RECT currentRect = placement.rcNormalPosition;

                        bool mismatch =
                            currentRect.Left != targetRect.Left ||
                            currentRect.Top != targetRect.Top ||
                            currentRect.Right != targetRect.Right ||
                            currentRect.Bottom != targetRect.Bottom ||
                            isMaximized != targetMaximized;

                        if (mismatch)
                        {
                            mismatches++;
                            ApplyPlacement(hwnd, profile);
                            applied++;
                        }
                    }
                }

                Thread.Sleep(EnforcementIntervalMs);
            }

            if (step != null)
            {
                step.Outcome = StepOutcome.Success;
                step.Detail =
                    $"Enforced {profile.WindowWidth}x{profile.WindowHeight} at ({profile.WindowX},{profile.WindowY})" +
                    $" for {EnforcementDurationMs}ms (dxClass={lastClass}, applied={applied}, mismatches={mismatches}).";
            }
        }

        private static IntPtr FindGw2DxWindow(Process process, int timeoutMs, out string foundClass, out int waitedMs)
        {
            foundClass = "";
            waitedMs = 0;

            var sw = Stopwatch.StartNew();
            while (!process.HasExited)
            {
                IntPtr found = IntPtr.Zero;
                string cls = "";

                EnumWindows((h, _) =>
                {
                    if (!IsWindowVisible(h))
                        return true;

                    GetWindowThreadProcessId(h, out uint pid);
                    if (pid != (uint)process.Id)
                        return true;

                    string c = GetClassNameSafe(h);
                    if (DxWindowClasses.Contains(c, StringComparer.Ordinal))
                    {
                        found = h;
                        cls = c;
                        return false;
                    }

                    return true;
                }, IntPtr.Zero);

                if (found != IntPtr.Zero)
                {
                    foundClass = cls;
                    waitedMs = (int)sw.ElapsedMilliseconds;
                    return found;
                }

                if (timeoutMs <= 0 || sw.ElapsedMilliseconds >= timeoutMs)
                    break;

                Thread.Sleep(200);
            }

            waitedMs = (int)sw.ElapsedMilliseconds;
            return IntPtr.Zero;
        }

        private static void ApplyPlacement(IntPtr hwnd, GameProfile profile)
        {
            if (profile.WindowWidth <= 0 || profile.WindowHeight <= 0)
                return;

            var placement = new WINDOWPLACEMENT { length = Marshal.SizeOf(typeof(WINDOWPLACEMENT)) };
            if (!GetWindowPlacement(hwnd, ref placement))
                return;

            placement.rcNormalPosition = new RECT
            {
                Left = profile.WindowX,
                Top = profile.WindowY,
                Right = profile.WindowX + profile.WindowWidth,
                Bottom = profile.WindowY + profile.WindowHeight
            };

            placement.showCmd = profile.WindowMaximized ? SW_MAXIMIZE : SW_SHOWNORMAL;

            SetWindowPlacement(hwnd, ref placement);
        }

        private static string GetClassNameSafe(IntPtr hwnd)
        {
            var sb = new StringBuilder(256);
            _ = GetClassName(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}
