using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using GWxLauncher.Domain;
using static GWxLauncher.Services.NativeMethods;

namespace GWxLauncher.Services
{
    internal static class WindowManagementService
    {
        private const string GW1_WINDOW_CLASS = "ArenaNet_Dx_Window_Class";
        private const int ENFORCEMENT_DURATION_MS = 7000;
        private const int ENFORCEMENT_INTERVAL_MS = 100;

        public static void ApplyWindowSettings(Process process, GameProfile profile)
        {
            if (process == null || profile == null) return;
            if (!profile.WindowedModeEnabled) return;

            IntPtr hwnd = FindGameWindow(process, TimeSpan.FromSeconds(10));

            if (hwnd == IntPtr.Zero) return;

            ApplyPlacement(hwnd, profile);

            if (profile.WindowLockChanges)
            {
                ApplyLocking(hwnd);
            }

            if (profile.WindowBlockInputs)
            {
                ApplyInputBlocking(hwnd);
            }
        }

        public static void ManageWindowLifecycle(Process process, GameProfile profile, Action<GameProfile>? saveProfileCallback)
        {
            if (process == null || profile == null) return;
            if (!profile.WindowedModeEnabled) return;

            Task.Run(async () =>
            {
                try
                {
                    IntPtr hwnd = IntPtr.Zero;
                    var startTime = DateTime.UtcNow;

                    // Baseline for enforcement
                    var targetRect = new RECT
                    {
                        Left = profile.WindowX,
                        Top = profile.WindowY,
                        Right = profile.WindowX + profile.WindowWidth,
                        Bottom = profile.WindowY + profile.WindowHeight
                    };
                    bool targetMaximized = profile.WindowMaximized;

                    // State for "Remember Changes"
                    RECT lastSeenRect = targetRect;
                    bool lastSeenMaximized = targetMaximized;
                    DateTime lastChangeTime = DateTime.MinValue;
                    bool pendingSave = false;

                    while (!process.HasExited)
                    {
                        if (hwnd == IntPtr.Zero || !NativeMethods.IsWindow(hwnd) || !NativeMethods.IsWindowVisible(hwnd))
                        {
                            hwnd = FindGameWindow(process, TimeSpan.Zero);
                        }

                        if (hwnd != IntPtr.Zero)
                        {
                            var placement = new WINDOWPLACEMENT();
                            placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));

                            if (GetWindowPlacement(hwnd, ref placement))
                            {
                                bool isMaximized = placement.showCmd == SW_MAXIMIZE;
                                RECT currentRect = placement.rcNormalPosition;

                                double elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

                                // Phase 1: Enforcement (Fix "Bounce")
                                if (elapsedMs < ENFORCEMENT_DURATION_MS)
                                {
                                    // If window drifted from profile setting, force it back
                                    // Allow small tolerance? No, exact is better for preventing creep.
                                    bool mismatch =
                                        currentRect.Left != targetRect.Left ||
                                        currentRect.Top != targetRect.Top ||
                                        currentRect.Right != targetRect.Right ||
                                        currentRect.Bottom != targetRect.Bottom ||
                                        isMaximized != targetMaximized;

                                    if (mismatch)
                                    {
                                        ApplyPlacement(hwnd, profile);
                                    }

                                    // Continually re-apply lock/input blocks during startup as game might recreate window
                                    if (profile.WindowLockChanges) ApplyLocking(hwnd);
                                    if (profile.WindowBlockInputs) ApplyInputBlocking(hwnd);
                                }
                                // Phase 2: Watcher (Save User Changes)
                                else if (profile.WindowRememberChanges && !profile.WindowLockChanges && saveProfileCallback != null)
                                {
                                    bool changed =
                                        currentRect.Left != lastSeenRect.Left ||
                                        currentRect.Top != lastSeenRect.Top ||
                                        currentRect.Right != lastSeenRect.Right ||
                                        currentRect.Bottom != lastSeenRect.Bottom ||
                                        isMaximized != lastSeenMaximized;

                                    if (changed)
                                    {
                                        lastSeenRect = currentRect;
                                        lastSeenMaximized = isMaximized;
                                        lastChangeTime = DateTime.UtcNow;
                                        pendingSave = true;
                                    }

                                    if (pendingSave && (DateTime.UtcNow - lastChangeTime).TotalMilliseconds > 1000)
                                    {
                                        // Commit changes to profile
                                        profile.WindowX = lastSeenRect.Left;
                                        profile.WindowY = lastSeenRect.Top;
                                        profile.WindowWidth = lastSeenRect.Right - lastSeenRect.Left;
                                        profile.WindowHeight = lastSeenRect.Bottom - lastSeenRect.Top;
                                        profile.WindowMaximized = lastSeenMaximized;

                                        // Update the enforcement target so we don't snap back later if we re-enable enforcement
                                        targetRect = lastSeenRect;
                                        targetMaximized = lastSeenMaximized;

                                        saveProfileCallback(profile);
                                        pendingSave = false;
                                    }
                                }
                            }
                        }

                        // Faster poll during enforcement
                        int delay = (DateTime.UtcNow - startTime).TotalMilliseconds < ENFORCEMENT_DURATION_MS
                            ? ENFORCEMENT_INTERVAL_MS
                            : 500;

                        await Task.Delay(delay);
                    }
                }
                catch
                {
                    // Swallow background errors
                }
            });
        }

        private static IntPtr FindGameWindow(Process process, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                // 1. Try EnumWindows for class match (most accurate for game window)
                IntPtr found = IntPtr.Zero;
                NativeMethods.EnumWindows((hwnd, lParam) =>
                {
                    uint pid;
                    NativeMethods.GetWindowThreadProcessId(hwnd, out pid);
                    if (pid == process.Id && NativeMethods.IsWindowVisible(hwnd))
                    {
                        StringBuilder sb = new StringBuilder(256);
                        NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
                        if (sb.ToString() == GW1_WINDOW_CLASS)
                        {
                            found = hwnd;
                            return false;
                        }
                    }
                    return true;
                }, IntPtr.Zero);

                if (found != IntPtr.Zero) return found;

                // 2. Fallback: MainWindowHandle
                process.Refresh();
                IntPtr mainHandle = process.MainWindowHandle;
                if (mainHandle != IntPtr.Zero && NativeMethods.IsWindowVisible(mainHandle))
                {
                    // Check if it looks right (not 0x0 size)
                    if (NativeMethods.GetClientRect(mainHandle, out RECT clientRect) &&
                       (clientRect.Right - clientRect.Left) > 100 && (clientRect.Bottom - clientRect.Top) > 100)
                    {
                        return mainHandle;
                    }
                }

                if (sw.Elapsed > timeout) break;
                Thread.Sleep(200);
            }
            return IntPtr.Zero;
        }

        private static void ApplyPlacement(IntPtr hwnd, GameProfile profile)
        {
            if (profile.WindowWidth <= 0 || profile.WindowHeight <= 0) return;

            var placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));

            if (!GetWindowPlacement(hwnd, ref placement))
                return;

            placement.rcNormalPosition = new RECT
            {
                Left = profile.WindowX,
                Top = profile.WindowY,
                Right = profile.WindowX + profile.WindowWidth,
                Bottom = profile.WindowY + profile.WindowHeight
            };

            if (profile.WindowMaximized)
                placement.showCmd = SW_MAXIMIZE;
            else
                placement.showCmd = SW_SHOWNORMAL;

            SetWindowPlacement(hwnd, ref placement);
        }

        private static void ApplyLocking(IntPtr hwnd)
        {
            IntPtr stylePtr = GetWindowLongPtr(hwnd, GWL_STYLE);
            long style = stylePtr.ToInt64();

            style &= ~WS_THICKFRAME;
            style &= ~WS_MAXIMIZEBOX;

            SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style));

            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        private static void ApplyInputBlocking(IntPtr hwnd)
        {
            IntPtr hMenu = GetSystemMenu(hwnd, false);
            if (hMenu != IntPtr.Zero)
            {
                EnableMenuItem(hMenu, SC_CLOSE, MF_GRAYED | MF_BYCOMMAND);
            }

            IntPtr stylePtr = GetWindowLongPtr(hwnd, GWL_STYLE);
            long style = stylePtr.ToInt64();

            style &= ~WS_MINIMIZEBOX;

            SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style));

            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }
    }
}
