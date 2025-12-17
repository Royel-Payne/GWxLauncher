using GWxLauncher.Domain;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace GWxLauncher.Services
{
    internal sealed class Gw2AutoLoginService
    {
        // --- Timing knobs (based on your testing) ---
        private const int WindowWaitTimeoutMs = 30000;
        private const int WindowPollMs = 200;

        private const int StabilizeCheckMs = 150;
        private const int StabilizeRequiredStableMs = 900;

        private const int PostForegroundSettleMs = 400;
        private const int PostStableExtraSettleMs = 1400;

        private const int AfterClickMs = 180;
        private const int AfterClearMs = 120;
        private const int CharDelayMs = 6;

        // --- Click target ratios inside the GW2 client area (0..1 of client W/H) ---
        // These are for the login fields in the launcher window.
        // Your logs show these were close; do not “tab navigate” here.
        private const double EmailClickX = 0.180;
        private const double EmailClickY = 0.425;

        private const double PassClickX = 0.180;
        private const double PassClickY = 0.520;

        // --- Public entry point ---
        public bool TryAutomateLogin(Process? gw2Process, GameProfile profile, LaunchReport report, out string error)
        {
            error = "";

            var stepLogin = new LaunchStep { Label = "Auto-Login" };
            report.Steps.Add(stepLogin);

            var stepPlay = new LaunchStep { Label = "Auto-Play" };
            report.Steps.Add(stepPlay);

            if (!profile.Gw2AutoLoginEnabled)
            {
                stepLogin.Outcome = StepOutcome.Skipped;
                stepLogin.Detail = "Disabled";
                stepPlay.Outcome = StepOutcome.Skipped;
                stepPlay.Detail = "Disabled";
                return true;
            }

            if (gw2Process == null)
            {
                stepLogin.Outcome = StepOutcome.Failed;
                stepLogin.Detail = "GW2 process was null.";
                stepPlay.Outcome = StepOutcome.Skipped;
                stepPlay.Detail = "Skipped (login failed).";
                error = "GW2 process was null.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(profile.Gw2Email) || string.IsNullOrWhiteSpace(profile.Gw2PasswordProtected))
            {
                stepLogin.Outcome = StepOutcome.Failed;
                stepLogin.Detail = "Email or password not configured.";
                stepPlay.Outcome = StepOutcome.Skipped;
                stepPlay.Detail = "Skipped (login failed).";
                error = "GW2 email/password not configured for this profile.";
                return false;
            }

            string password;
            try
            {
                password = DpapiProtector.UnprotectFromBase64(profile.Gw2PasswordProtected);
            }
            catch (Exception ex)
            {
                stepLogin.Outcome = StepOutcome.Failed;
                stepLogin.Detail = "Stored password could not be decrypted.";
                stepPlay.Outcome = StepOutcome.Skipped;
                stepPlay.Detail = "Skipped (login failed).";
                error = $"DPAPI decrypt failed: {ex.Message}";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                stepLogin.Outcome = StepOutcome.Failed;
                stepLogin.Detail = "Decrypted password was empty.";
                stepPlay.Outcome = StepOutcome.Skipped;
                stepPlay.Detail = "Skipped (login failed).";
                error = "GW2 decrypted password was empty.";
                return false;
            }

            if (!TryWaitForMainWindow(gw2Process.Id, out IntPtr gw2Hwnd, out int waitedMs))
            {
                stepLogin.Outcome = StepOutcome.Pending;
                stepLogin.Detail = $"GW2 window not detected within {waitedMs}ms (best-effort).";
                stepPlay.Outcome = StepOutcome.Skipped;
                stepPlay.Detail = "Skipped (no window).";
                return true;
            }

            string diagBase = $"hwnd=0x{gw2Hwnd.ToInt64():X} class={GetClassNameSafe(gw2Hwnd)} pid={gw2Process.Id}";

            bool blocked = false;
            try
            {
                // Ensure we are not fighting modifier keys (Ctrl/Alt/Shift) at the OS level.
                WaitForModifierKeysUp(timeoutMs: 2000);

                // Force foreground and let it settle
                if (!ForceAndHoldForeground(gw2Hwnd, holdStableMs: 250, timeoutMs: 5000))
                    throw new Exception("Failed to acquire GW2 foreground.");

                Thread.Sleep(PostForegroundSettleMs);

                // Wait until the client rect is stable (this is the missing piece you observed)
                if (!WaitForClientRectStable(gw2Hwnd, requiredStableMs: StabilizeRequiredStableMs, timeoutMs: 12000, out var clientTL, out var clientWH))
                    throw new Exception("GW2 client area did not stabilize in time (UI not rendered).");

                Thread.Sleep(PostStableExtraSettleMs);

                // BlockInput during the critical send phase
                blocked = BlockInput(true);

                // Re-assert foreground after BlockInput (it can cause tiny focus transitions)
                if (!ForceAndHoldForeground(gw2Hwnd, holdStableMs: 250, timeoutMs: 3000))
                    throw new Exception("Failed to keep GW2 foreground before typing.");

                // Compute click points in SCREEN coords from client TL + client WH
                int emailX = clientTL.X + (int)(clientWH.X * EmailClickX);
                int emailY = clientTL.Y + (int)(clientWH.Y * EmailClickY);

                int passX = clientTL.X + (int)(clientWH.X * PassClickX);
                int passY = clientTL.Y + (int)(clientWH.Y * PassClickY);

                // --- Email field ---
                MouseClickScreen(emailX, emailY);
                Thread.Sleep(AfterClickMs);

                if (!ForceAndHoldForeground(gw2Hwnd, holdStableMs: 200, timeoutMs: 2500))
                    throw new Exception("Foreground changed after email click.");

                SendCtrlA_Clear();
                Thread.Sleep(AfterClearMs);

                TypeUnicodeText(profile.Gw2Email, perCharDelayMs: CharDelayMs);

                // --- Password field ---
                Thread.Sleep(120);

                MouseClickScreen(passX, passY);
                Thread.Sleep(AfterClickMs);

                if (!ForceAndHoldForeground(gw2Hwnd, holdStableMs: 200, timeoutMs: 2500))
                    throw new Exception("Foreground changed after password click.");

                SendCtrlA_Clear();
                Thread.Sleep(AfterClearMs);

                TypeUnicodeText(password, perCharDelayMs: CharDelayMs);

                stepLogin.Outcome = StepOutcome.Success;
                stepLogin.Detail =
                    $"Typed email+password via SendInput(UNICODE) (client coords, stable wait, BlockInput). " +
                    $"clientTL=({clientTL.X},{clientTL.Y}) clientWH=({clientWH.X},{clientWH.Y}) " +
                    $"EmailClick=({emailX},{emailY}) PassClick=({passX},{passY}) | {diagBase}";
            }
            catch (Exception ex)
            {
                stepLogin.Outcome = StepOutcome.Failed;
                stepLogin.Detail = $"Auto-login failed: {ex.Message}";
                stepPlay.Outcome = StepOutcome.Skipped;
                stepPlay.Detail = "Skipped (login failed).";
                error = ex.Message;
                return false;
            }
            finally
            {
                if (blocked)
                    BlockInput(false);
            }

            if (!profile.Gw2AutoPlayEnabled)
            {
                stepPlay.Outcome = StepOutcome.Skipped;
                stepPlay.Detail = "Disabled";
                return true;
            }

            try
            {
                // Give CEF a moment to apply the final text
                Thread.Sleep(900);

                KeyPress(VK_RETURN);

                stepPlay.Outcome = StepOutcome.Success;
                stepPlay.Detail = "Sent Enter (SendInput).";
                return true;
            }
            catch (Exception ex)
            {
                stepPlay.Outcome = StepOutcome.Failed;
                stepPlay.Detail = "Failed to send Enter.";
                error = ex.Message;
                return false;
            }
        }

        // -----------------------------
        // Window finding / stability
        // -----------------------------

        private static bool TryWaitForMainWindow(int pid, out IntPtr hwnd, out int waitedMs)
        {
            waitedMs = 0;
            hwnd = IntPtr.Zero;

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < WindowWaitTimeoutMs)
            {
                hwnd = FindTopLevelWindowByPidAndClass(pid, "ArenaNet");
                if (hwnd != IntPtr.Zero && IsWindow(hwnd))
                {
                    waitedMs = (int)sw.ElapsedMilliseconds;
                    return true;
                }

                Thread.Sleep(WindowPollMs);
            }

            waitedMs = (int)sw.ElapsedMilliseconds;
            hwnd = IntPtr.Zero;
            return false;
        }

        private static bool WaitForClientRectStable(IntPtr hwnd, int requiredStableMs, int timeoutMs, out POINT clientTL, out POINT clientWH)
        {
            clientTL = new POINT();
            clientWH = new POINT();

            long stableFor = 0;
            var sw = Stopwatch.StartNew();

            POINT lastTL = new POINT { X = int.MinValue, Y = int.MinValue };
            POINT lastWH = new POINT { X = int.MinValue, Y = int.MinValue };

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (!GetClientRect(hwnd, out RECT cr))
                {
                    Thread.Sleep(StabilizeCheckMs);
                    continue;
                }

                var wh = new POINT { X = cr.Right - cr.Left, Y = cr.Bottom - cr.Top };

                var tl = new POINT { X = 0, Y = 0 };
                if (!ClientToScreen(hwnd, ref tl))
                {
                    Thread.Sleep(StabilizeCheckMs);
                    continue;
                }

                // Ignore obviously bogus values (happens during early render)
                if (wh.X < 200 || wh.Y < 200)
                {
                    Thread.Sleep(StabilizeCheckMs);
                    continue;
                }

                bool same = (tl.X == lastTL.X && tl.Y == lastTL.Y && wh.X == lastWH.X && wh.Y == lastWH.Y);

                if (same)
                {
                    stableFor += StabilizeCheckMs;
                    if (stableFor >= requiredStableMs)
                    {
                        clientTL = tl;
                        clientWH = wh;
                        return true;
                    }
                }
                else
                {
                    stableFor = 0;
                    lastTL = tl;
                    lastWH = wh;
                }

                Thread.Sleep(StabilizeCheckMs);
            }

            return false;
        }

        private static IntPtr FindTopLevelWindowByPidAndClass(int pid, string className)
        {
            IntPtr found = IntPtr.Zero;

            EnumWindows((h, _) =>
            {
                GetWindowThreadProcessId(h, out uint wpid);
                if ((int)wpid != pid)
                    return true;

                if (!IsWindowVisible(h))
                    return true;

                var cn = GetClassNameSafe(h);
                if (!string.Equals(cn, className, StringComparison.Ordinal))
                    return true;

                found = h;
                return false;
            }, IntPtr.Zero);

            return found;
        }

        // -----------------------------
        // Foreground / focus handling
        // -----------------------------

        private static bool ForceAndHoldForeground(IntPtr hwnd, int holdStableMs, int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                ForceForeground(hwnd);

                if (IsForegroundStable(hwnd, holdStableMs))
                    return true;

                Thread.Sleep(80);
            }

            return false;
        }

        private static void ForceForeground(IntPtr hwnd)
        {
            if (IsIconic(hwnd))
                ShowWindow(hwnd, SW_RESTORE);

            // Try standard path first
            SetForegroundWindow(hwnd);
            BringWindowToTop(hwnd);

            // Harder path: attach thread input to foreground thread
            IntPtr fg = GetForegroundWindow();
            uint fgTid = GetWindowThreadProcessId(fg, out _);
            uint myTid = GetCurrentThreadId();
            uint targetTid = GetWindowThreadProcessId(hwnd, out _);

            // Attach our thread to FG and to target briefly
            AttachThreadInput(myTid, fgTid, true);
            AttachThreadInput(myTid, targetTid, true);
            try
            {
                SetForegroundWindow(hwnd);
                SetFocus(hwnd);
            }
            finally
            {
                AttachThreadInput(myTid, targetTid, false);
                AttachThreadInput(myTid, fgTid, false);
            }
        }

        private static bool IsForegroundStable(IntPtr hwnd, int stableMs)
        {
            int elapsed = 0;
            while (elapsed < stableMs)
            {
                if (GetForegroundWindow() != hwnd)
                    return false;

                Thread.Sleep(50);
                elapsed += 50;
            }
            return true;
        }

        // -----------------------------
        // Input helpers (SendInput)
        // -----------------------------

        private static void SendCtrlA_Clear()
        {
            // Ctrl down
            SendKey(VK_CONTROL, keyUp: false);
            Thread.Sleep(10);

            // A down/up
            SendKey((ushort)'A', keyUp: false);
            Thread.Sleep(10);
            SendKey((ushort)'A', keyUp: true);

            Thread.Sleep(10);

            // Ctrl up
            SendKey(VK_CONTROL, keyUp: true);
        }

        private static void TypeUnicodeText(string text, int perCharDelayMs)
        {
            foreach (char c in text)
            {
                SendUnicodeChar(c);
                if (perCharDelayMs > 0)
                    Thread.Sleep(perCharDelayMs);
            }
        }

        private static void KeyPress(ushort vk)
        {
            SendKey(vk, keyUp: false);
            Thread.Sleep(15);
            SendKey(vk, keyUp: true);
        }

        private static void SendKey(ushort vk, bool keyUp)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = 0,
                        dwFlags = keyUp ? KEYEVENTF_KEYUP : 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            _ = SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendUnicodeChar(char c)
        {
            // KEYEVENTF_UNICODE uses wScan, wVk must be 0
            INPUT[] inputs = new INPUT[2];

            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = KEYEVENTF_UNICODE,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            inputs[1] = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            _ = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void WaitForModifierKeysUp(int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (!IsKeyDown(VK_SHIFT) && !IsKeyDown(VK_CONTROL) && !IsKeyDown(VK_MENU))
                    return;

                Thread.Sleep(50);
            }
        }

        private static bool IsKeyDown(int vk)
        {
            return (GetAsyncKeyState(vk) & 0x8000) != 0;
        }

        // -----------------------------
        // Mouse helpers
        // -----------------------------

        private static void MouseClickScreen(int x, int y)
        {
            SetCursorPos(x, y);

            INPUT[] inputs = new INPUT[2];
            inputs[0] = new INPUT { type = INPUT_MOUSE, U = new InputUnion { mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTDOWN } } };
            inputs[1] = new INPUT { type = INPUT_MOUSE, U = new InputUnion { mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTUP } } };

            _ = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        // -----------------------------
        // Misc helpers
        // -----------------------------

        private static string GetClassNameSafe(IntPtr hwnd)
        {
            var sb = new StringBuilder(256);
            _ = GetClassName(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        // -----------------------------
        // Win32
        // -----------------------------

        private const ushort VK_RETURN = 0x0D;
        private const ushort VK_CONTROL = 0x11;

        private const int VK_SHIFT = 0x10;
        private const int VK_MENU = 0x12; // Alt

        private const int SW_RESTORE = 9;

        private const int INPUT_MOUSE = 0;
        private const int INPUT_KEYBOARD = 1;

        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool BlockInput(bool fBlock);
    }
}
