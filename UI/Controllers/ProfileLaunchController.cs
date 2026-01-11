using System.Diagnostics;
using System.Text;
using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using Microsoft.Win32;

namespace GWxLauncher.UI.Controllers
{
    internal sealed class ProfileLaunchController
    {
        private readonly IWin32Window _owner;
        private readonly LaunchSessionPresenter _launchSession;
        private readonly StatusBarController _statusBar;
        private readonly WinFormsUiDispatcher _ui;

        private readonly Gw1InstanceTracker _gw1Instances;
        private readonly Gw2InstanceTracker _gw2Instances;

        private readonly Gw2LaunchOrchestrator _gw2Orchestrator;
        private readonly Gw2AutomationCoordinator _gw2Automation;
        private readonly Gw2RunAfterLauncher _gw2RunAfterLauncher;

        private readonly Func<LauncherConfig> _getConfig;
        private readonly Action<LauncherConfig> _setConfig;
        private readonly Action<string> _setStatus;
        private readonly Action _saveProfiles; // NEW

        public ProfileLaunchController(
            IWin32Window owner,
            LaunchSessionPresenter launchSession,
            StatusBarController statusBar,
            WinFormsUiDispatcher ui,
            Gw1InstanceTracker gw1Instances,
            Gw2InstanceTracker gw2Instances,
            Gw2LaunchOrchestrator gw2Orchestrator,
            Gw2AutomationCoordinator gw2Automation,
            Gw2RunAfterLauncher gw2RunAfterLauncher,
            Func<LauncherConfig> getConfig,
            Action<LauncherConfig> setConfig,
            Action<string> setStatus,
            Action saveProfiles = null) // NEW
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _launchSession = launchSession ?? throw new ArgumentNullException(nameof(launchSession));
            _statusBar = statusBar ?? throw new ArgumentNullException(nameof(statusBar));
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));

            _gw1Instances = gw1Instances ?? throw new ArgumentNullException(nameof(gw1Instances));
            _gw2Instances = gw2Instances ?? throw new ArgumentNullException(nameof(gw2Instances));

            _gw2Orchestrator = gw2Orchestrator ?? throw new ArgumentNullException(nameof(gw2Orchestrator));
            _gw2Automation = gw2Automation ?? throw new ArgumentNullException(nameof(gw2Automation));
            _gw2RunAfterLauncher = gw2RunAfterLauncher ?? throw new ArgumentNullException(nameof(gw2RunAfterLauncher));

            _getConfig = getConfig ?? throw new ArgumentNullException(nameof(getConfig));
            _setConfig = setConfig ?? throw new ArgumentNullException(nameof(setConfig));
            _setStatus = setStatus ?? throw new ArgumentNullException(nameof(setStatus));

            _saveProfiles = saveProfiles;
        }

        public string ResolveEffectiveExePath(GameProfile profile, LauncherConfig cfg)
        {
            if (profile == null)
                return string.Empty;

            // Always use the profile's executable path
            // If empty, the caller will show an appropriate error message
            return profile.ExecutablePath ?? string.Empty;
        }

        private void ApplyGw1RegistryFix(GameProfile profile)
        {
            if (profile == null || string.IsNullOrWhiteSpace(profile.ExecutablePath))
                return;

            try
            {
                // Open or create HKEY_CURRENT_USER\Software\ArenaNet\Guild Wars
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\ArenaNet\Guild Wars", writable: true))
                {
                    if (key == null)
                        return;

                    string targetPath = profile.ExecutablePath;
                    bool needsUpdate = false;

                    // Check if 'Path' exists and matches
                    var pathValue = key.GetValue("Path") as string;
                    if (pathValue != targetPath)
                    {
                        needsUpdate = true;
                    }

                    // Check if 'Src' exists and matches
                    var srcValue = key.GetValue("Src") as string;
                    if (srcValue != targetPath)
                    {
                        needsUpdate = true;
                    }

                    // Update registry values if needed
                    if (needsUpdate)
                    {
                        key.SetValue("Path", targetPath, RegistryValueKind.String);
                        key.SetValue("Src", targetPath, RegistryValueKind.String);
                    }
                }
            }
            catch
            {
                // Silently handle permission issues - HKCU typically doesn't require admin rights
                // but we don't want to block the launch if registry access fails
            }
        }

        public async Task LaunchProfileAsync(GameProfile profile, bool bulkMode)
        {
            if (profile == null)
                return;

            // Single launch = new "session". Bulk launch session is started once per batch.
            if (!bulkMode)
                _launchSession.BeginSession(bulkMode: false);

            _setConfig(LauncherConfig.Load());
            var cfg = _getConfig();

            string exePath = ResolveEffectiveExePath(profile, cfg);

            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                MessageBox.Show(
                    _owner,
                    "No valid executable path is configured for this profile.\n\n" +
                    "Edit the profile or configure the game path in settings.",
                    "Missing executable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Re-warn at launch time (warn-only, no elevation, user can continue)
            if (ProtectedInstallPathPolicy.IsProtectedPath(exePath))
            {
                bool cont = ProtectedInstallPathWarningDialog.ConfirmContinue(_owner, exePath);
                if (!cont)
                {
                    _setStatus($"Launch cancelled: protected install path for {profile.Name}.");
                    return;
                }
            }

            var stopwatch = Stopwatch.StartNew();

            // GW1: delegate launch + injection to Gw1InjectionService
            if (profile.GameType == GameType.GuildWars1)
            {
                if (_gw1Instances.IsRunning(profile.Id))
                {
                    MessageBox.Show(_owner, $"\"{profile.Name}\" is already running.", "Already Running",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Apply registry fix before launching
                ApplyGw1RegistryFix(profile);

                bool mcEnabled = cfg.Gw1MulticlientEnabled;
                bool winTitleEnabled = cfg.Gw1WindowTitleEnabled;
                string profileName = (profile.Name ?? "").Trim();
                string titleLabel = (profile.Gw1WindowTitleLabel ?? "").Trim();
                string titleTemplate = (cfg.Gw1WindowTitleTemplate ?? "").Trim();

                // Define UI-safe message callback
                Action<string, string, bool> showMessage = (msg, title, isError) =>
                {
                    _ui.Post(() => MessageBox.Show(
                        _owner,
                        msg,
                        title,
                        MessageBoxButtons.OK,
                        isError ? MessageBoxIcon.Error : MessageBoxIcon.Warning));
                };

                // Move heavy work to background
                var (success, launchedProcess, gw1Error, report) = await Task.Run(() =>
                {
                    var gw1Service = new Gw1InjectionService();

                    bool ok = gw1Service.TryLaunchGw1(
                        profile, cfg, exePath, mcEnabled, _owner,
                        out var proc, out var err, out var rep, showMessage); // Pass showMessage delegate

                    if (ok && proc != null)
                    {
                        // Apply Windowed Mode Settings (Position, Size, Locks)
                        try
                        {
                            WindowManagementService.ApplyWindowSettings(proc, profile);

                            // Start lifecycle management (enforcement + watching)
                            WindowManagementService.ManageWindowLifecycle(proc, profile, (_) =>
                            {
                                // Make sure we save on UI thread or safely
                                if (_saveProfiles != null)
                                    _ui.Post(() => _saveProfiles());
                            });
                        }
                        catch
                        {
                            // Best effort
                        }

                        // Window Title
                        if (winTitleEnabled)
                        {
                            var stepWindowTitle = new LaunchStep { Label = "Window Title" };
                            rep.Steps.Add(stepWindowTitle);

                            string title;
                            if (!string.IsNullOrWhiteSpace(titleLabel))
                            {
                                title = titleLabel;
                            }
                            else
                            {
                                title = string.IsNullOrWhiteSpace(titleTemplate)
                                    ? profileName
                                    : titleTemplate.Replace("{ProfileName}", profileName);
                            }

                            // This waits/retries, so do it in background
                            bool success = WindowTitleService.TrySetMainWindowTitle(proc, title, TimeSpan.FromSeconds(15));

                            if (success)
                            {
                                stepWindowTitle.Outcome = StepOutcome.Success;
                                stepWindowTitle.Detail = $"Set to \"{title}\"";
                            }
                            else
                            {
                                stepWindowTitle.Outcome = StepOutcome.Pending;
                                stepWindowTitle.Detail = "Window not detected within timeout (best-effort)";
                            }
                        }
                    }

                    return (ok, proc, err, rep);
                });

                stopwatch.Stop();

                // Record timing in report
                report.Steps.Add(new LaunchStep
                {
                    Label = "Launch timing",
                    Outcome = StepOutcome.Success,
                    Detail = $"Launcher measured elapsed time: {stopwatch.ElapsedMilliseconds}ms"
                });

                // Back on UI thread
                if (success)
                {
                    ApplyLaunchReportToUi(report);
                    if (launchedProcess != null)
                    {
                        _gw1Instances.TrackLaunched(profile.Id, launchedProcess);
                    }
                }
                else
                {
                    MessageBox.Show(
                        _owner,
                        gw1Error,
                        "Guild Wars 1 launch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    ApplyLaunchReportToUi(report);
                }

                // Optional: Log timing
                // _setStatus($"Launch took {stopwatch.ElapsedMilliseconds}ms");

                return;
            }

            if (profile.GameType == GameType.GuildWars2)
            {
                bool mcEnabled = cfg.Gw2MulticlientEnabled;

                // Use async version - heavy work runs on background threads internally
                var result = await _gw2Orchestrator.LaunchAsync(
                    profile: profile,
                    exePath: exePath,
                    mcEnabled: mcEnabled,
                    bulkMode: bulkMode,
                    automationCoordinator: _gw2Automation,
                    runAfterInvoker: _gw2RunAfterLauncher.Start).ConfigureAwait(false);

                stopwatch.Stop();

                if (result.Report != null)
                {
                    result.Report.Steps.Add(new LaunchStep
                    {
                        Label = "Launch timing",
                        Outcome = StepOutcome.Success,
                        Detail = $"Launcher measured elapsed time: {stopwatch.ElapsedMilliseconds}ms"
                    });

                    ProtectedInstallPathPolicy.TryAppendLaunchReportNote(result.Report, exePath);
                    ApplyLaunchReportToUi(result.Report);
                }

                if (result.HasMessageBox)
                {
                    MessageBox.Show(
                        _owner,
                        result.MessageBoxText,
                        result.MessageBoxTitle,
                        MessageBoxButtons.OK,
                        result.MessageBoxIsError ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
                }

                // Track GW2 instance
                if (result.LaunchedProcess != null)
                {
                    _gw2Instances.TrackLaunched(profile.Id, result.LaunchedProcess);

                    // Apply GW2 window title if enabled - wait for DX window to be fully ready
                    bool winTitleEnabled = cfg.Gw2WindowTitleEnabled;
                    if (winTitleEnabled)
                    {
                        string profileName = (profile.Name ?? "").Trim();
                        string titleLabel = (profile.Gw2WindowTitleLabel ?? "").Trim();
                        string titleTemplate = (cfg.Gw2WindowTitleTemplate ?? "").Trim();

                        string title;
                        if (!string.IsNullOrWhiteSpace(titleLabel))
                        {
                            title = titleLabel;
                        }
                        else
                        {
                            title = string.IsNullOrWhiteSpace(titleTemplate)
                                ? profileName
                                : titleTemplate.Replace("{ProfileName}", profileName);
                        }

                        // Create the step upfront so it appears in the report
                        var stepWindowTitle = new LaunchStep 
                        { 
                            Label = "Window Title",
                            Outcome = StepOutcome.Pending,
                            Detail = "Waiting for DX window..."
                        };
                        result.Report?.Steps.Add(stepWindowTitle);

                        // Run in background to avoid blocking
                        var process = result.LaunchedProcess;
                        _ = Task.Run(() =>
                        {
                            // Wait for DX window creation AND rendering before attempting to set title
                            // This ensures we're setting the title on the final game window, not intermediate launcher windows
                            if (WaitForGw2DxWindowReady(process, TimeSpan.FromSeconds(60), out IntPtr dxHwnd))
                            {
                                // Apply title directly to the fully-rendered DX window handle
                                bool success = NativeMethods.SetWindowText(dxHwnd, title);
                                
                                if (success)
                                {
                                    stepWindowTitle.Outcome = StepOutcome.Success;
                                    stepWindowTitle.Detail = $"Set to \"{title}\" (DX window verified)";
                                }
                                else
                                {
                                    stepWindowTitle.Outcome = StepOutcome.Failed;
                                    stepWindowTitle.Detail = "SetWindowText failed";
                                }
                            }
                            else
                            {
                                stepWindowTitle.Outcome = StepOutcome.Pending;
                                stepWindowTitle.Detail = "DX window not detected within timeout (best-effort)";
                            }
                        });
                    }
                }

                return;
            }
        }

        private void ApplyLaunchReportToUi(LaunchReport report)
        {
            _launchSession.Record(report);
            _setStatus(_launchSession.BuildStatusText());
        }

        private static bool WaitForGw2DxWindow(Process process, TimeSpan timeout, out IntPtr dxHwnd)
        {
            dxHwnd = IntPtr.Zero;

            if (process == null || process.HasExited)
                return false;

            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                if (process.HasExited)
                    return false;

                // Enumerate windows looking for GW2 DX window classes
                IntPtr found = IntPtr.Zero;
                NativeMethods.EnumWindows((hwnd, lParam) =>
                {
                    if (!NativeMethods.IsWindowVisible(hwnd))
                        return true;

                    NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
                    if (pid != (uint)process.Id)
                        return true;

                    var sb = new StringBuilder(256);
                    NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
                    string className = sb.ToString();

                    // GW2 3D window classes
                    if (className == "ArenaNet_Dx_Window_Class" || className == "ArenaNet_Gr_Window_Class")
                    {
                        found = hwnd;
                        return false; // Stop enumeration
                    }

                    return true;
                }, IntPtr.Zero);

                if (found != IntPtr.Zero)
                {
                    dxHwnd = found;
                    return true;
                }

                Thread.Sleep(200);
            }

            return false;
        }

        private static bool WaitForGw2DxWindowReady(Process process, TimeSpan timeout, out IntPtr dxHwnd)
        {
            dxHwnd = IntPtr.Zero;

            if (process == null || process.HasExited)
                return false;

            // Step 1: Wait for DX window to be created (up to 60 seconds)
            if (!WaitForGw2DxWindow(process, timeout, out dxHwnd))
                return false;

            // Step 2: Wait for the DX window to actually render (not just be white)
            // This prevents setting title on transitional windows
            if (!WaitForDxWindowRendering(dxHwnd, TimeSpan.FromSeconds(60)))
                return false;

            return true;
        }

        private static bool WaitForDxWindowRendering(IntPtr dxHwnd, TimeSpan timeout)
        {
            // Sample center of DX window to verify it's rendering (not white)
            if (!NativeMethods.GetClientRect(dxHwnd, out var cr))
                return false;

            var centerPt = new NativeMethods.POINT
            {
                X = (cr.Right - cr.Left) / 2,
                Y = (cr.Bottom - cr.Top) / 2
            };

            // Convert to screen coordinates
            if (!NativeMethods.ClientToScreen(dxHwnd, ref centerPt))
                return false;

            // Sample a small cross pattern around center
            var samples = new (int dx, int dy)[] { (0, 0), (-12, 0), (12, 0), (0, -3), (0, 3) };

            long stableFor = 0;
            var sw = Stopwatch.StartNew();

            while (sw.Elapsed < timeout)
            {
                int nonWhite = 0;

                foreach (var (dx, dy) in samples)
                {
                    if (!TryGetScreenPixel(centerPt.X + dx, centerPt.Y + dy, out var r, out var g, out var b))
                        continue;

                    // Window is rendering if pixels are not near-white
                    if (!IsNearWhite(r, g, b))
                        nonWhite++;
                }

                // Require 3+ non-white samples to be stable for 800ms
                if (nonWhite >= 3)
                {
                    stableFor += 200;
                    if (stableFor >= 800)
                        return true;
                }
                else
                {
                    stableFor = 0;
                }

                Thread.Sleep(200);
            }

            return false;
        }

        private static bool TryGetScreenPixel(int x, int y, out byte r, out byte g, out byte b)
        {
            r = g = b = 0;

            IntPtr hdc = NativeMethods.GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero)
                return false;

            try
            {
                uint c = NativeMethods.GetPixel(hdc, x, y);
                r = (byte)(c & 0xFF);
                g = (byte)((c >> 8) & 0xFF);
                b = (byte)((c >> 16) & 0xFF);
                return true;
            }
            finally
            {
                NativeMethods.ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        private static bool IsNearWhite(byte r, byte g, byte b)
        {
            return r >= 240 && g >= 240 && b >= 240;
        }
    }
}
