using System.Diagnostics;
using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;

namespace GWxLauncher.UI.Controllers
{
    internal sealed class BulkLaunchController
    {
        private readonly IWin32Window _owner;
        private readonly ProfileManager _profiles;
        private readonly ViewStateStore _views;
        private readonly LaunchEligibilityPolicy _policy;
        private readonly Func<bool> _getShowCheckedOnly;
        private readonly Func<string, bool> _isRunning;
        private readonly Func<LauncherConfig> _getConfig;
        private readonly Action<LauncherConfig> _setConfig;

        private readonly Action<RefreshReason> _requestRefresh;
        private readonly Action<string> _setStatus;
        private readonly Action _updateBulkArmingUi;
        private readonly Action<bool> _setBulkInProgress;

        private readonly Func<GameProfile, bool, Task> _launchProfile;
        private readonly Func<GameProfile, LauncherConfig, string> _resolveEffectiveExePath;

        public BulkLaunchController(
            IWin32Window owner,
            ProfileManager profiles,
            ViewStateStore views,
            LaunchEligibilityPolicy policy,
            Func<bool> getShowCheckedOnly,
            Func<string, bool> isRunning,
            Func<LauncherConfig> getConfig,
            Action<LauncherConfig> setConfig,
            Action<RefreshReason> requestRefresh,
            Action<string> setStatus,
            Action updateBulkArmingUi,
            Action<bool> setBulkInProgress,
            Func<GameProfile, bool, Task> launchProfile,
            Func<GameProfile, LauncherConfig, string> resolveEffectiveExePath)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));

            _getShowCheckedOnly = getShowCheckedOnly ?? throw new ArgumentNullException(nameof(getShowCheckedOnly));
            _isRunning = isRunning ?? throw new ArgumentNullException(nameof(isRunning));
            _getConfig = getConfig ?? throw new ArgumentNullException(nameof(getConfig));
            _setConfig = setConfig ?? throw new ArgumentNullException(nameof(setConfig));

            _requestRefresh = requestRefresh ?? throw new ArgumentNullException(nameof(requestRefresh));
            _setStatus = setStatus ?? throw new ArgumentNullException(nameof(setStatus));
            _updateBulkArmingUi = updateBulkArmingUi ?? throw new ArgumentNullException(nameof(updateBulkArmingUi));
            _setBulkInProgress = setBulkInProgress ?? throw new ArgumentNullException(nameof(setBulkInProgress));

            _launchProfile = launchProfile ?? throw new ArgumentNullException(nameof(launchProfile));
            _resolveEffectiveExePath = resolveEffectiveExePath ?? throw new ArgumentNullException(nameof(resolveEffectiveExePath));
        }

        public async Task LaunchAllAsync()
        {
            // Always evaluate the latest persisted settings (the settings form saves its own config instance).
            _setConfig(LauncherConfig.Load());

            var cfg = _getConfig();

            var eval = _policy.EvaluateBulkArming(
                allProfiles: _profiles.Profiles,
                activeViewName: _views.ActiveViewName,
                showCheckedOnly: _getShowCheckedOnly(),
                config: cfg);

            if (!eval.Armed)
            {
                if (!string.IsNullOrWhiteSpace(eval.StatusText))
                    _setStatus(eval.StatusText);
                return;
            }

            var targets = _policy.BuildBulkTargets(_profiles.Profiles, _views.ActiveViewName);

            // Guard: multiclient must be enabled for the eligible game type(s) or bulk launch won't work.
            if (!_policy.IsMulticlientEnabledForEligible(_profiles.Profiles, _views.ActiveViewName, cfg, out string missing))
            {
                string msg =
                    "Multiclient not enabled.\n\n" +
                    $"Not enabled for: {missing}\n\n" +
                    "Enable it now and continue?";

                var result = MessageBox.Show(_owner, msg, "Multiclient not enabled", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result != DialogResult.OK)
                {
                    _setStatus($"Bulk launch canceled · Multiclient missing: {missing}");
                    return;
                }

                _policy.EnableRequiredMulticlientFlagsForEligible(_profiles.Profiles, _views.ActiveViewName, cfg);
                cfg.Save();

                // Refresh state after saving so any subsequent checks see the latest values.
                _setConfig(LauncherConfig.Load());
                _requestRefresh(RefreshReason.BulkLaunchStateChanged);
            }

            if (targets.Count == 0)
            {
                _setStatus("No checked profiles to launch.");
                return;
            }

            _setBulkInProgress(true);
            _updateBulkArmingUi();

            // Bulk launch session begins once per batch.
            // (MainForm-owned LaunchSessionPresenter is already responsible for session semantics via LaunchProfile.)
            try
            {
                // Track GW1 PIDs for throttling detection (GW1 only).
                // Capture initial PIDs on background thread to avoid blocking UI
                HashSet<int>? gw1Before = null;

                for (int i = 0; i < targets.Count; i++)
                {
                    var profile = targets[i];
                    bool hasNext = i + 1 < targets.Count;

                    if (profile.GameType == GameType.GuildWars1)
                    {
                        // Capture process IDs on background thread
                        var exePath = _resolveEffectiveExePath(profile, _getConfig());
                        gw1Before ??= await Task.Run(() => CaptureProcessIdsForExePath(exePath)).ConfigureAwait(false);
                    }

                    if (_isRunning(profile.Id))
                    {
                        // Skip already-running profiles (bulk launch should be best-effort).
                        _setStatus($"Skipping running: {profile.Name}");
                        continue;
                    }

                    await _launchProfile(profile, true).ConfigureAwait(false);

                    if (hasNext)
                        await ApplyBulkLaunchThrottlingAsync(profile, gw1Before).ConfigureAwait(false);

                    // Give WinForms a chance to repaint between profiles.
                    await Task.Yield();
                }
            }
            finally
            {
                _setBulkInProgress(false);
                _updateBulkArmingUi();
            }
        }

        private async Task ApplyBulkLaunchThrottlingAsync(GameProfile lastLaunchedProfile, HashSet<int>? gw1BeforePids)
        {
            if (lastLaunchedProfile == null)
                return;

            var cfg = _getConfig();

            int requestedDelaySeconds = lastLaunchedProfile.GameType == GameType.GuildWars1
                ? cfg.Gw1BulkLaunchDelaySeconds
                : cfg.Gw2BulkLaunchDelaySeconds;

            Func<bool>? readiness = null;

            if (lastLaunchedProfile.GameType == GameType.GuildWars1)
            {
                // Step 3 probe is optional; if we cannot resolve PID or init fails, policy falls back to delay-only.
                var exePath = _resolveEffectiveExePath(lastLaunchedProfile, cfg);

                // Capture process IDs on background thread
                var gw1After = await Task.Run(() => CaptureProcessIdsForExePath(exePath)).ConfigureAwait(false);

                Process? gwProcess = null;
                if (gw1BeforePids != null)
                {
                    var newPids = gw1After.Except(gw1BeforePids).ToList();
                    if (newPids.Count == 1)
                    {
                        try { gwProcess = Process.GetProcessById(newPids[0]); }
                        catch { gwProcess = null; }
                    }
                }

                if (gwProcess != null)
                {
                    var probe = new Gw1ClientStateProbe();
                    if (probe.TryInitialize(gwProcess) && probe.IsAvailable)
                        readiness = probe.IsReady;
                    else
                        probe.Dispose();
                }
            }

            Action<string> status = s =>
            {
                if (!string.IsNullOrWhiteSpace(s))
                    _setStatus(s);
            };

            // NOTE: This matches the existing BulkLaunchThrottlingPolicy.ApplyAsync signature used in MainForm.
            await BulkLaunchThrottlingPolicy.ApplyAsync(
                gameType: lastLaunchedProfile.GameType,
                requestedDelaySeconds: requestedDelaySeconds,
                readinessCheck: readiness,
                statusCallback: status,
                report: null).ConfigureAwait(false);
        }

        private static HashSet<int> CaptureProcessIdsForExePath(string exePath)
        {
            var set = new HashSet<int>();

            if (string.IsNullOrWhiteSpace(exePath))
                return set;

            var processes = Gw1InstanceTracker.FindProcessesByExactPath(exePath);
            foreach (var p in processes)
            {
                set.Add(p.Id);
            }

            return set;
        }
    }
}
