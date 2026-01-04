using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI;

namespace GWxLauncher.UI.Controllers
{
    internal sealed class ProfileLaunchController
    {
        private readonly IWin32Window _owner;
        private readonly LaunchSessionPresenter _launchSession;
        private readonly StatusBarController _statusBar;
        private readonly WinFormsUiDispatcher _ui;

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

        public void LaunchProfile(GameProfile profile, bool bulkMode)
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

            // GW1: delegate launch + injection to Gw1InjectionService
            if (profile.GameType == GameType.GuildWars1)
            {
                var gw1Service = new Gw1InjectionService();
                bool mcEnabled = cfg.Gw1MulticlientEnabled;

                if (gw1Service.TryLaunchGw1(profile, exePath, mcEnabled, _owner, out var gw1Error, out var report))
                {
                    ApplyLaunchReportToUi(report);
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

                return;
            }

            if (profile.GameType == GameType.GuildWars2)
            {
                bool mcEnabled = cfg.Gw2MulticlientEnabled;

                var result = _gw2Orchestrator.Launch(
                    profile: profile,
                    exePath: exePath,
                    mcEnabled: mcEnabled,
                    bulkMode: bulkMode,
                    automationCoordinator: _gw2Automation,
                    runAfterInvoker: _gw2RunAfterLauncher.Start);

                if (result.Report != null)
                {
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
        public Gw2LaunchOrchestrator.Gw2LaunchResult LaunchProfileGw2BulkWorker(GameProfile profile)
        {
            if (profile == null)
                return new Gw2LaunchOrchestrator.Gw2LaunchResult();

            // Use a fresh config snapshot (ProfileSettingsForm writes and saves its own config instance).
            var cfg = LauncherConfig.Load();

            string exePath = profile.ExecutablePath;
            if (string.IsNullOrWhiteSpace(exePath))
                exePath = cfg.Gw2Path;

            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                return new Gw2LaunchOrchestrator.Gw2LaunchResult
                {
                    Report = null,
                    MessageBoxText =
                        "No valid executable path is configured for this profile.\n\n" +
                        "Edit the profile or configure the game path in settings.",
                    MessageBoxTitle = "Missing executable",
                    MessageBoxIsError = false
                };
            }

            bool mcEnabled = cfg.Gw2MulticlientEnabled;

            return _gw2Orchestrator.Launch(
                profile: profile,
                exePath: exePath,
                mcEnabled: mcEnabled,
                bulkMode: true,
                automationCoordinator: _gw2Automation,
                runAfterInvoker: _gw2RunAfterLauncher.Start);
        }

        private void ApplyLaunchReportToUi(LaunchReport report)
        {
            _launchSession.Record(report);
            _setStatus(_launchSession.BuildStatusText());
        }
    }
}
