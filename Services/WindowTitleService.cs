using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static GWxLauncher.Services.NativeMethods;

namespace GWxLauncher.Services
{
    internal static class WindowTitleService
    {
        public static bool TrySetMainWindowTitle(Process process, string title, TimeSpan timeout)
        {
            if (process == null)
                return false;

            IntPtr hwnd = WaitForMainWindow(process, timeout);
            if (hwnd == IntPtr.Zero)
                return false;

            if (!SetWindowText(hwnd, title ?? string.Empty))
                return false;

            // GW1 can transition from splash -> main window; do a short stability check.
            // If the main window handle changes shortly after, re-apply once.
            try
            {
                System.Threading.Thread.Sleep(750);
                process.Refresh();
                if (process.MainWindowHandle != IntPtr.Zero && process.MainWindowHandle != hwnd)
                {
                    SetWindowText(process.MainWindowHandle, title ?? string.Empty);
                }
            }
            catch { /* best-effort */ }

            return true;
        }

        public static string? TryGetMainWindowTitle(Process process)
        {
            if (process == null)
                return null;

            IntPtr hwnd = IntPtr.Zero;
            try
            {
                process.Refresh();
                hwnd = process.MainWindowHandle;
            }
            catch { }

            if (hwnd == IntPtr.Zero)
                return null;

            return GetWindowTitle(hwnd);
        }

        public static IntPtr WaitForMainWindow(Process process, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();

            IntPtr first = IntPtr.Zero;
            IntPtr last = IntPtr.Zero;

            while (sw.Elapsed < timeout)
            {
                try
                {
                    if (process.HasExited)
                        return IntPtr.Zero;

                    process.Refresh();
                    var hwnd = process.MainWindowHandle;

                    if (hwnd != IntPtr.Zero)
                    {
                        // first non-zero (often splash)
                        if (first == IntPtr.Zero)
                        {
                            first = hwnd;
                            last = hwnd;
                        }
                        else if (hwnd != last)
                        {
                            // handle changed => likely the real game window
                            return hwnd;
                        }
                    }
                }
                catch
                {
                    // ignore
                }

                Thread.Sleep(100);
            }

            // If we never saw a post-splash change, use what we got.
            return first;
        }

        private static string? GetWindowTitle(IntPtr hwnd)
        {
            try
            {
                int length = GetWindowTextLength(hwnd);
                if (length <= 0)
                    return string.Empty;

                var sb = new StringBuilder(length + 1);
                GetWindowText(hwnd, sb, sb.Capacity);
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
