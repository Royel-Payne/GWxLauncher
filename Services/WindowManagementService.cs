using System.Diagnostics;
using System.Runtime.InteropServices;
using GWxLauncher.Domain;
using static GWxLauncher.Services.NativeMethods;

namespace GWxLauncher.Services
{
    internal static class WindowManagementService
    {
        public static void ApplyWindowSettings(Process process, GameProfile profile)
        {
            if (process == null || profile == null) return;
            if (!profile.WindowedModeEnabled) return;

            // Wait for window handle (timeout handled inside WaitForMainWindow or caller should ensure readiness)
            // Assuming this is called when we know window *should* be there, but we might need to wait a bit.
            IntPtr hwnd = WindowTitleService.WaitForMainWindow(process, TimeSpan.FromSeconds(10));
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
            
            // If locking is enabled, remember changes should be disabled logically, but check here too.
            if (profile.WindowLockChanges) return;

            Task.Run(async () =>
            {
                try
                {
                    IntPtr hwnd = WindowTitleService.WaitForMainWindow(process, TimeSpan.FromSeconds(10));
                    if (hwnd == IntPtr.Zero) return;

                    RECT lastRect = new RECT { Left = profile.WindowX, Top = profile.WindowY, Right = profile.WindowX + profile.WindowWidth, Bottom = profile.WindowY + profile.WindowHeight };
                    bool lastMaximized = profile.WindowMaximized;
                    
                    DateTime lastChangeTime = DateTime.MinValue;
                    bool pendingSave = false;

                    while (!process.HasExited)
                    {
                        if (NativeMethods.IsWindow(hwnd))
                        {
                            var placement = new WINDOWPLACEMENT();
                            placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT)); // FIX: Must initialize length
                            
                            if (GetWindowPlacement(hwnd, ref placement))
                            {
                                // Check for changes
                                bool maximized = placement.showCmd == SW_MAXIMIZE; // Approx check
                                
                                // We care about rcNormalPosition
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

                                // Debounce save
                                if (pendingSave && (DateTime.UtcNow - lastChangeTime).TotalMilliseconds > 1000)
                                {
                                    // Update profile object
                                    profile.WindowX = lastRect.Left;
                                    profile.WindowY = lastRect.Top;
                                    profile.WindowWidth = lastRect.Right - lastRect.Left;
                                    profile.WindowHeight = lastRect.Bottom - lastRect.Top;
                                    profile.WindowMaximized = lastMaximized;

                                    // Persist
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
            // Remove sizing border and maximize box
            IntPtr stylePtr = GetWindowLongPtr(hwnd, GWL_STYLE);
            long style = stylePtr.ToInt64();

            style &= ~WS_THICKFRAME;
            style &= ~WS_MAXIMIZEBOX;
            
            SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style));
            
            // Force frame update
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, 
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        private static void ApplyInputBlocking(IntPtr hwnd)
        {
            // Disable Close (X) button
            IntPtr hMenu = GetSystemMenu(hwnd, false);
            if (hMenu != IntPtr.Zero)
            {
                EnableMenuItem(hMenu, SC_CLOSE, MF_GRAYED | MF_BYCOMMAND);
            }

            // Remove minimize box (and maximize too usually makes sense if blocking inputs)
            IntPtr stylePtr = GetWindowLongPtr(hwnd, GWL_STYLE);
            long style = stylePtr.ToInt64();

            style &= ~WS_MINIMIZEBOX;
            
            SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style));

            // Force frame update
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, 
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }
    }
}
