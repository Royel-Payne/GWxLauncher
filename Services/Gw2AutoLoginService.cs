using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using GWxLauncher.Domain;
using static GWxLauncher.Services.NativeMethods;

namespace GWxLauncher.Services
{
    internal sealed class Gw2AutoLoginService
    {
        // --- Timing knobs (based on your testing) ---
        private const int WindowWaitTimeoutMs = 30000;
        private const int WindowPollMs = 200;

        private const int StabilizeCheckMs = 150;
        private const int StabilizeRequiredStableMs = 1200;

        private const int PostForegroundSettleMs = 900;
        private const int PostStableExtraSettleMs = 4100;

        private const int AfterClickMs = 180;
        private const int AfterClearMs = 120;
        private const int CharDelayMs = 12;

        // --- Click target ratios inside the GW2 client area (0..1 of client W/H) ---
        // These are for the login fields in the launcher window.
        // Your logs show these were close; do not “tab navigate” here.
        private const double EmailClickX = 0.270;
        private const double EmailClickY = 0.430;

        private const double PassClickX = 0.270;
        private const double PassClickY = 0.500;

        // --- Pre-login UI probe ratios (for "Launcher UI Rendered" gate) ---
        // We avoid sampling the textbox fill (white); instead we probe reliable non-white anchors.
        private const double LoginButtonProbeX = 0.074;
        private const double LoginButtonProbeY = 0.585;

        private const double ProgressBarProbeX = 0.653;
        private const double ProgressBarProbeY = 0.711;

        private const double ArtProbeX = 0.830;
        private const double ArtProbeY = 0.300;

        private const double ReadyButtonProbeX = 0.760;
        private const double ReadyButtonProbeY = 0.705;

        private const double AnetLogoProbeX = 0.552;
        private const double AnetLogoProbeY = 0.290;


        // --- PLAY button ratios inside the GW2 client area ---
        // Derived from your provided screenshots / testing.
        private const double PlayClickX = 0.725;
        private const double PlayClickY = 0.715;

        private const int PlayWaitTimeoutMs = 30000;
        private const int PlayPollMs = 200;
        private const int PlayRequiredStableMs = 600;

        // Hand-cursor gate (best-effort)
        private const int HandGateTimeoutMs = 700;  // per probe point
        private const int HandGatePollMs = 50;
        private const int HandHuntTotalTimeoutMs = 3500; // total time to hunt around target


        // --- Public entry point ---
        public bool TryAutomateLogin(Process? gw2Process, GameProfile profile, LaunchReport report, bool bulkMode, out string error)
        {
            error = "";

            var stepUiReady = new LaunchStep { Label = "Launcher UI Rendered" };
            var stepLogin = new LaunchStep { Label = "Auto-Login" };
            var stepLauncherReady = new LaunchStep { Label = "Launcher Ready (Play Enabled)" };
            var stepPlay = new LaunchStep { Label = "Auto-Play" };
            var stepDxWindow = new LaunchStep { Label = "DX Window Created" };

            report.Steps.Add(stepUiReady);
            report.Steps.Add(stepLogin);
            report.Steps.Add(stepLauncherReady);
            report.Steps.Add(stepPlay);
            report.Steps.Add(stepDxWindow);

            if (!profile.Gw2AutoLoginEnabled)
            {
                stepLogin.Outcome = StepOutcome.Skipped;
                stepLogin.Detail = "Disabled";

                stepUiReady.Outcome = StepOutcome.Skipped;
                stepUiReady.Detail = "Disabled";

                stepLauncherReady.Outcome = StepOutcome.Skipped;
                stepLauncherReady.Detail = "Disabled";

                stepDxWindow.Outcome = StepOutcome.Skipped;
                stepDxWindow.Detail = "Disabled";

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
                stepLauncherReady.Outcome = StepOutcome.Skipped;
                stepLauncherReady.Detail = "Skipped (login failed).";
                stepUiReady.Outcome = StepOutcome.Skipped;
                stepUiReady.Detail = "Skipped (login failed).";
                stepDxWindow.Outcome = StepOutcome.Skipped;
                stepDxWindow.Detail = "Skipped (login failed).";
                return false;
            }

            if (string.IsNullOrWhiteSpace(profile.Gw2Email) || string.IsNullOrWhiteSpace(profile.Gw2PasswordProtected))
            {
                stepLogin.Outcome = StepOutcome.Failed;
                stepLogin.Detail = "Email or password not configured.";
                stepPlay.Outcome = StepOutcome.Skipped;
                stepPlay.Detail = "Skipped (login failed).";
                error = "GW2 email/password not configured for this profile.";
                stepLauncherReady.Outcome = StepOutcome.Skipped;
                stepLauncherReady.Detail = "Skipped (login failed).";
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
                stepLauncherReady.Outcome = StepOutcome.Skipped;
                stepLauncherReady.Detail = "Skipped (login failed).";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                stepLogin.Outcome = StepOutcome.Failed;
                stepLogin.Detail = "Decrypted password was empty.";
                stepPlay.Outcome = StepOutcome.Skipped;
                stepPlay.Detail = "Skipped (login failed).";
                error = "GW2 decrypted password was empty.";
                stepLauncherReady.Outcome = StepOutcome.Skipped;
                stepLauncherReady.Detail = "Skipped (login failed).";
                return false;
            }

            if (!TryWaitForMainWindow(gw2Process.Id, out IntPtr gw2Hwnd, out int waitedMs))
            {
                stepLogin.Outcome = StepOutcome.Pending;
                stepLogin.Detail = $"GW2 window not detected within {waitedMs}ms (best-effort).";
                stepPlay.Outcome = StepOutcome.Skipped;
                stepPlay.Detail = "Skipped (no window).";
                stepUiReady.Outcome = StepOutcome.Skipped;
                stepUiReady.Detail = "Skipped (no window).";
                stepDxWindow.Outcome = StepOutcome.Skipped;
                stepDxWindow.Detail = "Skipped (no window).";
                return true;
            }

            bool blocked = false;
            bool wasDisabled = false;
            
            try
            {
                // Pre-automation checks
                if (!WaitForModifierKeysUp(timeoutMs: 5000))
                {
                    error = "Modifier keys held down - aborting automation";
                    stepLogin.Outcome = StepOutcome.Failed;
                    stepLogin.Detail = error;
                    return false;
                }

                if (!ForceAndHoldForeground(gw2Hwnd, holdStableMs: 250, timeoutMs: 5000))
                    throw new Exception("Failed to acquire GW2 foreground.");

                Thread.Sleep(PostForegroundSettleMs);

                if (!WaitForClientRectStable(gw2Hwnd, requiredStableMs: StabilizeRequiredStableMs, timeoutMs: 12000, out var clientTL, out var clientWH))
                    throw new Exception("GW2 client area did not stabilize in time (UI not rendered).");

                Thread.Sleep(PostStableExtraSettleMs);

                // Get window rect (full window including borders/title bar) for coordinate calculations
                // Note: Gw2Launcher uses GetWindowRect, NOT GetClientRect!
                if (!GetWindowRect(gw2Hwnd, out RECT windowRect))
                    throw new Exception("Failed to get window rect");

                int windowW = windowRect.Right - windowRect.Left;
                int windowH = windowRect.Bottom - windowRect.Top;

                // Find an empty area dynamically using WM_NCHITTEST (like Gw2Launcher does)
                // Start at 80% right, 50% down and search left until we find client area
                int emptyX = windowW * 4 / 5;  // 80%
                int emptyY = windowH / 2;      // 50%
                uint emptyCoord = 0;

                // Use WM_NCHITTEST to find a valid empty spot
                for (int searchX = emptyX; searchX > 50; searchX -= 50)
                {
                    // Test in screen coordinates
                    uint testCoord = (uint)(((emptyY + windowRect.Top) << 16) | (searchX + windowRect.Left));
                    IntPtr hitTest = SendMessage(gw2Hwnd, 0x0084, IntPtr.Zero, new IntPtr(testCoord)); // WM_NCHITTEST
                    
                    if (hitTest == new IntPtr(1)) // HTCLIENT = 1 (hit test = client area)
                    {
                        // Found valid client area - store window-relative coords
                        emptyCoord = (uint)((emptyY << 16) | searchX);
                        stepLogin.Detail += $" (empty coord: {searchX},{emptyY})";
                        break;
                    }
                }

                if (emptyCoord == 0)
                    throw new Exception("Could not find empty area to click");

// --- Debugging visualizer (uncomment to use) ---
// Draws a red crosshair at the computed empty area
/*
                {
                    SetForegroundWindow(gw2Hwnd);
                    Thread.Sleep(10);
                    DrawCrosshair(windowRect.Left + (int)(emptyCoord & 0xFFFF),
                                  windowRect.Top + ((int)(emptyCoord >> 16) & 0xFFFF));
                }
*/

                // Gate: ensure the launcher page has actually rendered before we start
                if (!WaitForLauncherUiRendered(clientTL, clientWH, 0, 0, 0, 0,
                        timeoutMs: 7500, requiredStableMs: 600, out string uiDiag))
                {
                    stepUiReady.Outcome = StepOutcome.Pending;
                    stepUiReady.Detail = $"Launcher UI not yet rendered (best-effort). {uiDiag}";
                }
                else
                {
                    stepUiReady.Outcome = StepOutcome.Success;
                    stepUiReady.Detail = $"Launcher UI rendered. {uiDiag}";
                }

                // DON'T disable window - it blocks keyboard messages via SendMessage!
                // Gw2Launcher only disables to prevent MOUSE clicks, but they have special keyboard handling
                // We'll rely on speed instead
                // wasDisabled = DisableWindow(gw2Hwnd);

                // Boost child process priority to reduce input delays
                try
                {
                    var childProcesses = Process.GetProcessesByName("CoherentUI_Host")
                        .Where(p => {
                            try { return GetParentProcessId(p.Id) == gw2Process.Id; }
                            catch { return false; }
                        }).ToList();
                    
                    foreach (var child in childProcesses)
                    {
                        try { child.PriorityClass = ProcessPriorityClass.High; }
                        catch { }
                    }
                }
                catch { }

                if (!ForceAndHoldForeground(gw2Hwnd, holdStableMs: 250, timeoutMs: 3000))
                    throw new Exception("Failed to keep GW2 foreground before typing.");

                // Wait for modifiers before starting input
                if (!WaitForModifierKeysUp(timeoutMs: 5000))
                    throw new Exception("Modifier keys held - cannot proceed with input");

                // NEW APPROACH: Tab navigation instead of clicking
                // 1. Click empty area to clear focus
                SendMessage(gw2Hwnd, 0x0201, new IntPtr(1), new IntPtr(emptyCoord)); // WM_LBUTTONDOWN
                SendMessage(gw2Hwnd, 0x0202, IntPtr.Zero, new IntPtr(emptyCoord)); // WM_LBUTTONUP
              
                // Sync wait
                SendMessage(gw2Hwnd, 0, IntPtr.Zero, IntPtr.Zero);
                Thread.Sleep(50);

                // 2. Tab to email field (14 tabs for CEF launcher, based on Gw2Launcher)
                // CEF (Chromium) requires SendInput, not SendMessage!
                // CRITICAL: Ensure window has focus before SendInput!
                if (GetForegroundWindow() != gw2Hwnd)
                {
                    stepLogin.Detail += " (warn: window lost focus before Tab navigation)";
                    if (!ForceAndHoldForeground(gw2Hwnd, holdStableMs: 250, timeoutMs: 2000))
                        throw new Exception("Lost focus before Tab navigation");
                }

                for (int i = 0; i < 14; i++)
                {
                    // Use SendInput for hardware-level simulation
                    SendKey(VK_TAB, keyUp: false);
                    Thread.Sleep(10);
                    SendKey(VK_TAB, keyUp: true);
                    Thread.Sleep(10);
                }
                
                Thread.Sleep(100);

                _ = EnsureForegroundOrWarn(gw2Hwnd, holdStableMs: 200, timeoutMs: 4000, stepLogin, "after tab to email");

                // Wait for modifiers before text entry
                if (!WaitForModifierKeysUp(timeoutMs: 5000))
                    throw new Exception("Modifier keys held during email entry");

                // 3. Enter email (try clipboard first, fallback to Unicode SendInput)
                if (!TryTypeViaClipboard(gw2Process.Id, gw2Hwnd, profile.Gw2Email, stepLogin))
                {
                    // Fallback: Unicode input via SendInput (hardware simulation for CEF)
                    TypeUnicodeText(profile.Gw2Email, CharDelayMs);
                }
                
                Thread.Sleep(100);

                // Wait for modifiers before tab
                if (!WaitForModifierKeysUp(timeoutMs: 5000))
                    throw new Exception("Modifier keys held before password tab");

                // 4. Tab to password field (use SendInput for CEF)
                SendKey(VK_TAB, keyUp: false);
                Thread.Sleep(10);
                SendKey(VK_TAB, keyUp: true);
                
                Thread.Sleep(100);

                _ = EnsureForegroundOrWarn(gw2Hwnd, holdStableMs: 200, timeoutMs: 4000, stepLogin, "after tab to password");

                // Wait for modifiers before password entry
                if (!WaitForModifierKeysUp(timeoutMs: 5000))
                    throw new Exception("Modifier keys held during password entry");

                // 5. Enter password (try clipboard first, fallback to Unicode SendInput)
                if (!TryTypeViaClipboard(gw2Process.Id, gw2Hwnd, password, stepLogin))
                {
                    // Fallback: Unicode input via SendInput (hardware simulation for CEF)
                    TypeUnicodeText(password, CharDelayMs);
                }
                
                Thread.Sleep(250);

                // Wait for modifiers before Enter
                if (!WaitForModifierKeysUp(timeoutMs: 5000))
                    throw new Exception("Modifier keys held before submit");

                // 6. Submit login (Enter key via SendInput for CEF)
                SendKey(VK_RETURN, keyUp: false);
                Thread.Sleep(15);
                SendKey(VK_RETURN, keyUp: true);

                stepLogin.Outcome = StepOutcome.Success;
                stepLogin.Detail = "Typed email+password via Tab navigation (clipboard primary, WM_CHAR fallback, WS_DISABLED, sync waits).";
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
                // Window was not disabled, so nothing to re-enable
                // if (wasDisabled)
                //     EnableWindow(gw2Hwnd);
                    
                // Reset child process priority
                try
                {
                    var childProcesses = Process.GetProcessesByName("CoherentUI_Host")
                        .Where(p => {
                            try { return GetParentProcessId(p.Id) == gw2Process.Id; }
                            catch { return false; }
                        }).ToList();
                    
                    foreach (var child in childProcesses)
                    {
                        try { child.PriorityClass = ProcessPriorityClass.Normal; }
                        catch { }
                    }
                }
                catch { }
            }
            // In Launch All (bulk mode), do not start the next GW2 instance until the launcher has transitioned
            // out of the immediate post-login churn (white window / auth / render).
            if (!bulkMode)
            {
                stepLauncherReady.Outcome = StepOutcome.Skipped;
                stepLauncherReady.Detail = "Not in bulk mode.";
            }
            else
            {
                try
                {
                    // Re-stabilize client rect (screen coords) post-login transition
                    if (!WaitForClientRectStable(gw2Hwnd, requiredStableMs: StabilizeRequiredStableMs, timeoutMs: 20000, out var clientTL2, out var clientWH2))
                    {
                        stepLauncherReady.Outcome = StepOutcome.Pending;
                        stepLauncherReady.Detail = "Client area did not stabilize post-login (best-effort).";
                    }
                    else
                    {
                        int playX = clientTL2.X + (int)(clientWH2.X * PlayClickX);
                        int playY = clientTL2.Y + (int)(clientWH2.Y * PlayClickY);

                        if (!WaitForPlayScreen(playX, playY, PlayWaitTimeoutMs, out string gateDiag))
                        {
                            stepLauncherReady.Outcome = StepOutcome.Pending;
                            stepLauncherReady.Detail = $"Post-login UI gate not detected: {gateDiag}";
                        }
                        else
                        {
                            stepLauncherReady.Outcome = StepOutcome.Success;
                            stepLauncherReady.Detail = $"Post-login UI stabilized. {gateDiag}";
                        }
                    }
                }
                catch (Exception ex)
                {
                    stepLauncherReady.Outcome = StepOutcome.Pending;
                    stepLauncherReady.Detail = $"Post-login stable gate error (best-effort): {ex.Message}";
                }
            }

            if (!profile.Gw2AutoPlayEnabled)
            {
                stepPlay.Outcome = StepOutcome.Skipped;
                stepPlay.Detail = "Disabled";

                stepDxWindow.Outcome = StepOutcome.Skipped;
                stepDxWindow.Detail = "Not attempted (Auto-Play disabled).";

                return true;
            }

            bool playClicked = false;
            string lastGateDiag = "";
            string lastHandDiag = "";

            try
            {
                // Re-stabilize client rect (screen coords) post-login transition
                if (!WaitForClientRectStable(gw2Hwnd, requiredStableMs: StabilizeRequiredStableMs, timeoutMs: 20000, out var clientTL2, out var clientWH2))
                {
                    stepPlay.Outcome = StepOutcome.Pending;
                    stepPlay.Detail = "PLAY wait skipped: client area did not stabilize post-login.";

                    if (bulkMode)
                    {
                        stepDxWindow.Outcome = StepOutcome.Skipped;
                        stepDxWindow.Detail = "Not attempted (PLAY not clicked).";
                    }
                    else
                    {
                        stepDxWindow.Outcome = StepOutcome.Skipped;
                        stepDxWindow.Detail = "Not in bulk mode.";
                    }

                    return true; // best-effort
                }

                // Get window rect for Play button (window-relative coords)
                if (!GetClientRect(gw2Hwnd, out RECT playWindowRect))
                    throw new Exception("Failed to get window rect for Play button");

                int playWindowW = playWindowRect.Right - playWindowRect.Left;
                int playWindowH = playWindowRect.Bottom - playWindowRect.Top;

                // Calculate Play button position (window-relative)
                int playXRel = (int)(playWindowW * PlayClickX);
                int playYRel = (int)(playWindowH * PlayClickY);

                // For pixel sampling, we still need screen coordinates
                int playXScreen = clientTL2.X + (int)(clientWH2.X * PlayClickX);
                int playYScreen = clientTL2.Y + (int)(clientWH2.Y * PlayClickY);

                if (!WaitForPlayScreen(playXScreen, playYScreen, PlayWaitTimeoutMs, out string gateDiag))
                {
                    stepPlay.Outcome = StepOutcome.Pending;
                    stepPlay.Detail = $"PLAY not detected: {gateDiag}";

                    if (bulkMode)
                    {
                        stepDxWindow.Outcome = StepOutcome.Skipped;
                        stepDxWindow.Detail = "Not attempted (PLAY not clicked).";
                    }
                    else
                    {
                        stepDxWindow.Outcome = StepOutcome.Skipped;
                        stepDxWindow.Detail = "Not in bulk mode.";
                    }

                    return true; // best-effort
                }

                lastGateDiag = gateDiag;

                if (!ForceAndHoldForeground(gw2Hwnd, holdStableMs: 250, timeoutMs: 5000))
                    throw new Exception("Failed to keep GW2 foreground before PLAY click.");

                // --- Hand-cursor hunt around the PLAY target (best-effort) ---
                // Note: Hand cursor uses screen coords, but PostMessage uses window-relative
                if (TryFindHandCursorHotspot(playXScreen, playYScreen, HandHuntTotalTimeoutMs, out int hxScreen, out int hyScreen, out string handDiag))
                {
                    lastHandDiag = handDiag;
                    
                    // Convert screen coords back to window-relative for PostMessage
                    int hxRel = hxScreen - clientTL2.X;
                    int hyRel = hyScreen - clientTL2.Y;
                    uint playCoord = (uint)((hyRel << 16) | hxRel);
                    
                    PostMessage(gw2Hwnd, NativeMethods.WM_LBUTTONDOWN, new IntPtr(1), new IntPtr(playCoord));
                    PostMessage(gw2Hwnd, NativeMethods.WM_LBUTTONUP, IntPtr.Zero, new IntPtr(playCoord));
                    playClicked = true;

                    stepPlay.Outcome = StepOutcome.Success;
                    stepPlay.Detail = $"Clicked PLAY (hand cursor) at window-rel ({hxRel},{hyRel}). {gateDiag} {handDiag}";
                }
                else
                {
                    lastHandDiag = handDiag;

                    // Fallback: use computed window-relative coordinates
                    uint playCoord = (uint)((playYRel << 16) | playXRel);
                    PostMessage(gw2Hwnd, NativeMethods.WM_LBUTTONDOWN, new IntPtr(1), new IntPtr(playCoord));
                    PostMessage(gw2Hwnd, NativeMethods.WM_LBUTTONUP, IntPtr.Zero, new IntPtr(playCoord));
                    playClicked = true;

                    stepPlay.Outcome = StepOutcome.Success;
                    stepPlay.Detail = $"Clicked PLAY at window-rel ({playXRel},{playYRel}). {gateDiag} {handDiag}";
                }
            }
            catch (Exception ex)
            {
                stepPlay.Outcome = StepOutcome.Failed;
                stepPlay.Detail = $"Failed to click PLAY: {ex.Message}";
                error = ex.Message;
                return false;
            }

            // NEW: In bulk mode, do not release the coordinator gate until the 3D window exists and is drawing (best-effort).
            if (!bulkMode)
            {
                stepDxWindow.Outcome = StepOutcome.Skipped;
                stepDxWindow.Detail = "Not in bulk mode.";
                return true;
            }

            if (!playClicked)
            {
                stepDxWindow.Outcome = StepOutcome.Skipped;
                stepDxWindow.Detail = "Not attempted (PLAY not clicked).";
                return true;
            }

            if (WaitForDxWindowCreated(gw2Process.Id, timeoutMs: 60000, out var dxHwnd, out var dxClass, out var dxWaitedMs))
            {
                stepDxWindow.Outcome = StepOutcome.Success;
                stepDxWindow.Detail = $"DX window created: {dxClass} after {dxWaitedMs}ms.";

                if (WaitForDxNonWhite(dxHwnd, timeoutMs: 60000, out string dxDiag))
                    stepDxWindow.Detail += $" DX ready. {dxDiag}";
                else
                {
                    stepDxWindow.Outcome = StepOutcome.Pending;
                    stepDxWindow.Detail += " DX window stayed white (best-effort).";
                }
            }
            else
            {
                stepDxWindow.Outcome = StepOutcome.Pending;
                stepDxWindow.Detail = "DX window not observed (best-effort).";
            }

            return true;
        }

        // -----------------------------
        // Hand-cursor gating (best-effort)
        // -----------------------------

        private static bool TryFindHandCursorHotspot(int baseX, int baseY, int totalTimeoutMs, out int foundX, out int foundY, out string diag)
        {
            foundX = baseX;
            foundY = baseY;
            diag = "";

            // Search pattern around the target point.
            // The idea: if our ratio lands near-but-not-on the interactive region, we "walk" into it.
            var offsets = new (int dx, int dy)[]
            {
                (0, 0),
                (-30, 0), (30, 0),
                (0, -18), (0, 18),
                (-50, 0), (50, 0),
                (-30, -18), (30, -18),
                (-30, 18), (30, 18),
                (-70, 0), (70, 0),
                (0, -28), (0, 28),
                (-50, -18), (50, -18),
                (-50, 18), (50, 18),
            };

            var sw = Stopwatch.StartNew();
            int tries = 0;

            while (sw.ElapsedMilliseconds < totalTimeoutMs)
            {
                foreach (var (dx, dy) in offsets)
                {
                    if (sw.ElapsedMilliseconds >= totalTimeoutMs)
                        break;

                    tries++;

                    int x = baseX + dx;
                    int y = baseY + dy;

                    // Move cursor there first.
                    SetCursorPos(x, y);
                    Thread.Sleep(30);

                    if (WaitForSystemHandCursor(HandGateTimeoutMs, HandGatePollMs))
                    {
                        foundX = x;
                        foundY = y;
                        diag = $"(hand found after {sw.ElapsedMilliseconds}ms, tries={tries}, at dx={dx},dy={dy})";
                        return true;
                    }
                }

                // If we didn't find it in one sweep, wait a moment and try again (UI may still be settling)
                Thread.Sleep(80);
            }

            diag = $"(hand not detected after {sw.ElapsedMilliseconds}ms, tries={tries})";
            return false;
        }

        private static bool WaitForSystemHandCursor(int timeoutMs, int pollMs)
        {
            IntPtr hand = LoadCursor(IntPtr.Zero, IDC_HAND);
            if (hand == IntPtr.Zero)
                return false;

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                var ci = new CURSORINFO { cbSize = Marshal.SizeOf<CURSORINFO>() };
                if (GetCursorInfo(out ci) && (ci.flags & CURSOR_SHOWING) != 0)
                {
                    if (ci.hCursor == hand)
                        return true;
                }

                Thread.Sleep(pollMs);
            }

            return false;
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
        private static bool WaitForDxNonWhite(IntPtr dxHwnd, int timeoutMs, out string diag)
        {
            diag = "";

            // Find center-ish point of the DX window client area in screen coords
            if (!GetClientRect(dxHwnd, out RECT cr))
            {
                diag = "GetClientRect failed.";
                return false;
            }

            var pt = new POINT { X = (cr.Right - cr.Left) / 2, Y = (cr.Bottom - cr.Top) / 2 };

            // Sample a tiny cross around center
            var samples = new (int dx, int dy)[] { (0, 0), (-12, 0), (12, 0), (0, -3), (0, 3) };

            long stableFor = 0;
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                int nonWhite = 0;

                foreach (var (dx, dy) in samples)
                {
                    if (!TryGetScreenPixel(pt.X + dx, pt.Y + dy, out var r, out var g, out var b))
                        continue;

                    if (!IsNearWhite(r, g, b))
                        nonWhite++;
                }

                if (nonWhite >= 3)
                {
                    stableFor += 200;
                    if (stableFor >= 800)
                    {
                        diag = $"Ready after {sw.ElapsedMilliseconds}ms (nonWhite={nonWhite}/5)";
                        return true;
                    }
                }
                else
                {
                    stableFor = 0;
                }

                Thread.Sleep(200);
            }

            diag = $"Timed out after {sw.ElapsedMilliseconds}ms waiting for DX non-white.";
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
                return false; // stop enumeration
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
                Thread.Sleep(60);
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

            SetForegroundWindow(hwnd);
            BringWindowToTop(hwnd);

            IntPtr fg = GetForegroundWindow();
            uint fgTid = GetWindowThreadProcessId(fg, out _);
            uint myTid = GetCurrentThreadId();
            uint targetTid = GetWindowThreadProcessId(hwnd, out _);

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
            SendKey(VK_CONTROL, keyUp: false);
            Thread.Sleep(10);

            SendKey((ushort)'A', keyUp: false);
            Thread.Sleep(10);
            SendKey((ushort)'A', keyUp: true);

            Thread.Sleep(10);

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
                        wScan = (ushort)MapVirtualKey(vk, 0), // MAPVK_VK_TO_VSC = 0 (get scan code)
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

        private static bool WaitForModifierKeysUp(int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (!IsKeyDown(VK_SHIFT) && !IsKeyDown(VK_CONTROL) && !IsKeyDown(VK_MENU))
                    return true;

                Thread.Sleep(50);
            }
            
            return false; // Timed out
        }

        private static bool IsKeyDown(int vk)
        {
            return (GetAsyncKeyState(vk) & 0x8000) != 0;
        }
        
        private static int GetParentProcessId(int pid)
        {
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.PROCESS_QUERY_INFORMATION, false, pid);
            if (hProcess == IntPtr.Zero)
                return -1;

            try
            {
                PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
                int returnLength;
                int status = NtQueryInformationProcess(hProcess, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
                
                if (status != 0)
                    return -1;

                return pbi.Reserved3.ToInt32();
            }
            finally
            {
                CloseHandle(hProcess);
            }
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
        
        private static bool DisableWindow(IntPtr hwnd)
        {
            try
            {
                IntPtr stylePtr = GetWindowLongPtr(hwnd, GWL_STYLE);
                long style = stylePtr.ToInt64();
                style |= WS_DISABLED;
                SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style));
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private static bool EnableWindow(IntPtr hwnd)
        {
            try
            {
                IntPtr stylePtr = GetWindowLongPtr(hwnd, GWL_STYLE);
                long style = stylePtr.ToInt64();
                style &= ~WS_DISABLED;
                SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style));
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private static bool TryTypeViaClipboard(int targetPid, IntPtr hwnd, string text, LaunchStep step)
        {
            try
            {
                // Save current clipboard
                string? savedClip = null;
                try
                {
                    if (Clipboard.ContainsText())
                        savedClip = Clipboard.GetText();
                }
                catch { /* ignore */ }

                // Set our text
                Clipboard.SetText(text);
                Thread.Sleep(50);

                // Send Ctrl+V via SendInput (hardware simulation for CEF)
                SendKey(VK_CONTROL, keyUp: false);
                Thread.Sleep(10);
                SendKey((ushort)'V', keyUp: false);
                Thread.Sleep(10);
                SendKey((ushort)'V', keyUp: true);
                Thread.Sleep(10);
                SendKey(VK_CONTROL, keyUp: true);
                
                Thread.Sleep(100); // Allow GW2 to read clipboard

                // Restore clipboard
                try
                {
                    if (savedClip != null)
                        Clipboard.SetText(savedClip);
                    else
                        Clipboard.Clear();
                }
                catch { /* ignore */ }

                return true;
            }
            catch (Exception ex)
            {
                step.Detail += $" (clipboard method failed: {ex.Message}, using WM_CHAR fallback)";
                return false;
            }
        }
        
        private static bool EnsureForegroundOrWarn(IntPtr hwnd, int holdStableMs, int timeoutMs, LaunchStep step, string context)
        {
            if (ForceAndHoldForeground(hwnd, holdStableMs, timeoutMs))
                return true;

            // Do not hard-fail: in practice GW2 can steal focus briefly during render/churn.
            step.Detail += $" (warn: foreground not stable after {context})";
            return false;
        }

        private static bool WaitForDxWindowCreated(int pid, int timeoutMs, out IntPtr dxHwnd, out string dxClass, out int waitedMs)
        {
            dxHwnd = IntPtr.Zero;
            dxClass = "";
            waitedMs = 0;

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                var found = FindTopLevelWindowByPidAndClassPrefix(pid, out dxHwnd, out dxClass);
                if (found)
                {
                    waitedMs = (int)sw.ElapsedMilliseconds;
                    return true;
                }

                Thread.Sleep(200);
            }

            waitedMs = (int)sw.ElapsedMilliseconds;
            return false;
        }
        private static bool FindTopLevelWindowByPidAndClassPrefix(int pid, out IntPtr hwnd, out string cls)
        {
            IntPtr foundHwnd = IntPtr.Zero;
            string foundCls = "";

            EnumWindows((h, l) =>
            {
                if (!IsWindowVisible(h))
                    return true;

                GetWindowThreadProcessId(h, out uint winPid);
                if (winPid != (uint)pid)
                    return true;

                var sb = new StringBuilder(256);
                GetClassName(h, sb, sb.Capacity);
                var c = sb.ToString();

                // GW2 3D window classes (per insight doc)
                if (c == "ArenaNet_Dx_Window_Class" || c == "ArenaNet_Gr_Window_Class")
                {
                    foundHwnd = h;
                    foundCls = c;
                    return false; // stop enumeration
                }

                return true;
            }, IntPtr.Zero);

            hwnd = foundHwnd;
            cls = foundCls;
            return hwnd != IntPtr.Zero;
        }

        private static string GetClassNameSafe(IntPtr hwnd)
        {
            var sb = new StringBuilder(256);
            _ = GetClassName(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        private static bool TryGetScreenPixel(int x, int y, out byte r, out byte g, out byte b)
        {
            r = g = b = 0;

            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero)
                return false;

            try
            {
                uint c = GetPixel(hdc, x, y);
                r = (byte)(c & 0xFF);
                g = (byte)((c >> 8) & 0xFF);
                b = (byte)((c >> 16) & 0xFF);
                return true;
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        private static bool IsNearWhite(byte r, byte g, byte b)
        {
            return r >= 240 && g >= 240 && b >= 240;
        }


        private static bool WaitForLauncherUiRendered(POINT clientTL, POINT clientWH,
            int emailX, int emailY, int passX, int passY,
            int timeoutMs, int requiredStableMs, out string diag)
        {
            diag = "";

            // Probe points in screen coords. We try to hit non-white "anchors" that only appear after the page is rendered.
            // (1) Borders/labels near the input fields (offsets relative to the click points, not inside the white fill)
            // (2) Log In button region (dark)
            // (3) Progress bar region (orange)
            // (4) Artwork area (non-white)
            var probes = new (int x, int y, string label)[]
            {
                // Known non-white anchors only
                (clientTL.X + (int)(clientWH.X * LoginButtonProbeX),
                 clientTL.Y + (int)(clientWH.Y * LoginButtonProbeY),
                 "login-btn"),

                (clientTL.X + (int)(clientWH.X * ProgressBarProbeX),
                 clientTL.Y + (int)(clientWH.Y * ProgressBarProbeY),
                 "progress"),

                (clientTL.X + (int)(clientWH.X * ArtProbeX),
                 clientTL.Y + (int)(clientWH.Y * ArtProbeY),
                 "art"),

                (clientTL.X + (int)(clientWH.X * ReadyButtonProbeX),
                 clientTL.Y + (int)(clientWH.Y * ReadyButtonProbeY),
                 "ready"),

                (clientTL.X + (int)(clientWH.X * AnetLogoProbeX),
                 clientTL.Y + (int)(clientWH.Y * AnetLogoProbeY),
                 "anet"),
            };

            int neededNonWhite = 3; // of total probes
            long stableFor = 0;
            int samplesTaken = 0;
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                samplesTaken++;

                int nonWhite = 0;
                int total = 0;

                foreach (var (x, y, _) in probes)
                {
                    total++;
                    if (!TryGetScreenPixel(x, y, out var r, out var g, out var b))
                        continue;

                    if (!IsNearWhite(r, g, b))
                        nonWhite++;
                }

                if (nonWhite >= neededNonWhite)
                {
                    stableFor += StabilizeCheckMs;
                    if (stableFor >= requiredStableMs)
                    {
                        diag = $"Ready after {sw.ElapsedMilliseconds}ms (nonWhite={nonWhite}/{probes.Length}, samples={samplesTaken})";
                        return true;
                    }
                }
                else
                {
                    stableFor = 0;
                }

                Thread.Sleep(StabilizeCheckMs);
            }

            diag = $"Timed out after {sw.ElapsedMilliseconds}ms (nonWhite < {neededNonWhite}/{probes.Length}, samples={samplesTaken})";
            return false;
        }

        private static bool WaitForNonWhiteStable(int x, int y, int timeoutMs, int requiredStableMs, out string diag)
        {
            diag = "";
            long stableFor = 0;
            int samplesTaken = 0;

            // Sample a small cross around the point to avoid a single unlucky pixel.
            var points = new (int dx, int dy)[] { (0, 0), (-10, 0), (10, 0), (0, -10), (0, 10) };

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                samplesTaken++;
                int nonWhite = 0;

                foreach (var (dx, dy) in points)
                {
                    if (!TryGetScreenPixel(x + dx, y + dy, out var r, out var g, out var b))
                        continue;

                    if (!IsNearWhite(r, g, b))
                        nonWhite++;
                }

                if (nonWhite >= 3)
                {
                    stableFor += StabilizeCheckMs;
                    if (stableFor >= requiredStableMs)
                    {
                        diag = $"Ready after {sw.ElapsedMilliseconds}ms (nonWhite={nonWhite}/5, samples={samplesTaken})";
                        return true;
                    }
                }
                else
                {
                    stableFor = 0;
                }

                Thread.Sleep(StabilizeCheckMs);
            }

            diag = $"Timed out after {sw.ElapsedMilliseconds}ms (samples={samplesTaken})";
            return false;
        }

        private static bool WaitForPlayScreen(int playX, int playY, int timeoutMs, out string diag)
        {
            diag = "";

            var samples = new (int dx, int dy)[]
            {
                (0, 0),
                (-18, 14),
                (18, 14),
            };

            long stableFor = 0;
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                int nonWhite = 0;

                foreach (var (dx, dy) in samples)
                {
                    if (!TryGetScreenPixel(playX + dx, playY + dy, out var r, out var g, out var b))
                        continue;

                    if (!IsNearWhite(r, g, b))
                        nonWhite++;
                }

                if (nonWhite >= 2)
                {
                    stableFor += PlayPollMs;
                    if (stableFor >= PlayRequiredStableMs)
                    {
                        diag = $"Ready after {sw.ElapsedMilliseconds}ms (nonWhite={nonWhite}/3)";
                        return true;
                    }
                }
                else
                {
                    stableFor = 0;
                }

                Thread.Sleep(PlayPollMs);
            }

            diag = $"Timed out after {sw.ElapsedMilliseconds}ms waiting for PLAY screen.";
            return false;
        }
    }
}
