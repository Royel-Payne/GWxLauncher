using System.Diagnostics;
using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;

namespace GWxLauncher.UI.Controllers
{
    internal sealed class ProfileLaunchController
    {
        private readonly IWin32Window _owner;
        private readonly LaunchSessionPresenter _launchSession;
        private readonly StatusBarController _statusBar;
        private readonly WinFormsUiDispatcher _ui;

        private readonly Gw1InstanceTracker _gw1Instances;

        private readonly Gw2LaunchOrchestrator _gw2Orchestrator;
        private readonly Gw2AutomationCoordinator _gw2Automation;
        private readonly Gw2RunAfterLauncher _gw2RunAfterLauncher;

        private readonly Func<LauncherConfig> _getConfig;
        private readonly Action<LauncherConfig> _setConfig;
        private readonly Action<string> _setStatus;

        public ProfileLaunchController(
            IWin32Window owner,
            LaunchSessionPresenter launchSession,
            StatusBarController statusBar,
            WinFormsUiDispatcher ui,
            Gw1InstanceTracker gw1Instances,
            Gw2LaunchOrchestrator gw2Orchestrator,
            Gw2AutomationCoordinator gw2Automation,
            Gw2RunAfterLauncher gw2RunAfterLauncher,
            Func<LauncherConfig> getConfig,
            Action<LauncherConfig> setConfig,
            Action<string> setStatus)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _launchSession = launchSession ?? throw new ArgumentNullException(nameof(launchSession));
            _statusBar = statusBar ?? throw new ArgumentNullException(nameof(statusBar));
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));

            _gw1Instances = gw1Instances ?? throw new ArgumentNullException(nameof(gw1Instances));

            _gw2Orchestrator = gw2Orchestrator ?? throw new ArgumentNullException(nameof(gw2Orchestrator));
            _gw2Automation = gw2Automation ?? throw new ArgumentNullException(nameof(gw2Automation));
            _gw2RunAfterLauncher = gw2RunAfterLauncher ?? throw new ArgumentNullException(nameof(gw2RunAfterLauncher));

            _getConfig = getConfig ?? throw new ArgumentNullException(nameof(getConfig));
            _setConfig = setConfig ?? throw new ArgumentNullException(nameof(setConfig));
            _setStatus = setStatus ?? throw new ArgumentNullException(nameof(setStatus));
        }

        public string ResolveEffectiveExePath(GameProfile profile, LauncherConfig cfg)
        {
            if (profile == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(profile.ExecutablePath))
                return profile.ExecutablePath;

            return profile.GameType == GameType.GuildWars1 ? cfg.Gw1Path : cfg.Gw2Path;
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

                    if (ok && proc != null && winTitleEnabled)
                    {
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
                        WindowTitleService.TrySetMainWindowTitle(proc, title, TimeSpan.FromSeconds(15));
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

                // Move heavy work to background
                var result = await Task.Run(() => _gw2Orchestrator.Launch(
                    profile: profile,
                    exePath: exePath,
                    mcEnabled: mcEnabled,
                    bulkMode: bulkMode,
                    automationCoordinator: _gw2Automation,
                    runAfterInvoker: _gw2RunAfterLauncher.Start));

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

                return;
            }
        }

        private void ApplyLaunchReportToUi(LaunchReport report)
        {
            _launchSession.Record(report);
            _setStatus(_launchSession.BuildStatusText());
        }
    }
}
