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

        public static void ApplyWindowSettings(Process process, GameProfile profile)
        {
            if (process == null || profile == null) return;
            if (!profile.WindowedModeEnabled) return;

            // Robustly find the real window, not just the handle at start
            IntPtr hwnd = FindGameWindow(process, TimeSpan.FromSeconds(10));
            
            if (hwnd == IntPtr.Zero) return;

            // 1. Apply placement
            ApplyPlacement(hwnd, profile);

            // 2. Lock changes (prevent resize/move)
            if (profile.WindowLockChanges)
            {
                ApplyLocking(hwnd);
            }

            // 3. Block inputs (minimize/close)
            if (profile.WindowBlockInputs)
            {
                ApplyInputBlocking(hwnd);
            }
        }

        public static void StartWatching(Process process, GameProfile profile, Action<GameProfile> saveProfileCallback)
        {
            if (process == null || profile == null || saveProfileCallback == null) return;
            if (!profile.WindowedModeEnabled || !profile.WindowRememberChanges) return;
            
            if (profile.WindowLockChanges) return;

            Task.Run(async () =>
            {
                try
                {
                    // Initial state capture
                    RECT lastRect = new RECT { Left = profile.WindowX, Top = profile.WindowY, Right = profile.WindowX + profile.WindowWidth, Bottom = profile.WindowY + profile.WindowHeight };
                    bool lastMaximized = profile.WindowMaximized;
                    
                    DateTime lastChangeTime = DateTime.MinValue;
                    bool pendingSave = false;

                    // Keep track of which window we are watching
                    IntPtr currentHwnd = IntPtr.Zero;

                    while (!process.HasExited)
                    {
                        // 1. Re-acquire window if invalid or lost
                        if (currentHwnd == IntPtr.Zero || !NativeMethods.IsWindow(currentHwnd) || !NativeMethods.IsWindowVisible(currentHwnd))
                        {
                            currentHwnd = FindGameWindow(process, TimeSpan.Zero); // Fast check
                        }

                        // 2. Poll if we have a valid window
                        if (currentHwnd != IntPtr.Zero)
                        {
                            var placement = new WINDOWPLACEMENT();
                            placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                            
                            if (GetWindowPlacement(currentHwnd, ref placement))
                            {
                                bool maximized = placement.showCmd == SW_MAXIMIZE; 
                                RECT currentRect = placement.rcNormalPosition;
                                
                                bool changed = 
                                    currentRect.Left != lastRect.Left || 
                                    currentRect.Top != lastRect.Top || 
                                    currentRect.Right != lastRect.Right || 
                                    currentRect.Bottom != lastRect.Bottom ||
                                    maximized != lastMaximized;

                                if (changed)
                                {
                                    lastRect = currentRect;
                                    lastMaximized = maximized;
                                    lastChangeTime = DateTime.UtcNow;
                                    pendingSave = true;
                                }

                                if (pendingSave && (DateTime.UtcNow - lastChangeTime).TotalMilliseconds > 1000)
                                {
                                    profile.WindowX = lastRect.Left;
                                    profile.WindowY = lastRect.Top;
                                    profile.WindowWidth = lastRect.Right - lastRect.Left;
                                    profile.WindowHeight = lastRect.Bottom - lastRect.Top;
                                    profile.WindowMaximized = lastMaximized;

                                    saveProfileCallback(profile);
                                    pendingSave = false;
                                }
                            }
                        }
                        
                        await Task.Delay(500);
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
