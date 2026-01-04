using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI;
using GWxLauncher.UI.Controllers;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;



namespace GWxLauncher
{
    public partial class MainForm : Form
    {
        // -----------------------------
        // Fields / State
        // -----------------------------

        private ProfileGridController _profileGrid;
        private readonly ViewUiController _viewUi;
        private readonly MainFormRefresher _refresher;
        private readonly StatusBarController _statusBar;

        private LauncherConfig _config;
        private readonly ViewStateStore _views = new();
        private readonly ProfileManager _profileManager = new();
        private readonly LaunchSessionPresenter _launchSession = new();
        private readonly Gw2RunAfterLauncher _gw2RunAfterLauncher = new();
        private readonly Gw2AutomationCoordinator _gw2Automation = new Gw2AutomationCoordinator();
        private readonly Gw2LaunchOrchestrator _gw2Orchestrator = new Gw2LaunchOrchestrator();
        private readonly LaunchEligibilityPolicy _launchPolicy;
        private readonly WinFormsUiDispatcher _ui;

        private readonly Image _gw1Image = Properties.Resources.Gw1;
        private readonly Image _gw2Image = Properties.Resources.Gw2;

        private bool _showCheckedOnly = false;
        private bool _bulkLaunchInProgress = false;
        private readonly Font _nameFont;
        private readonly Font _subFont;

        private readonly ProfileSelectionController _selection;
        private readonly ProfileContextMenuController _profileMenu;
        private readonly BulkLaunchController _bulkLaunch;
        private readonly ProfileLaunchController _launchController;


        // -----------------------------
        // Ctor / Form lifecycle
        // -----------------------------

        public MainForm()
        {
            InitializeComponent();
            _statusBar = new StatusBarController(lblStatus);
            _viewUi = new ViewUiController(
                views: _views,
                txtView: txtView,
                chkShowCheckedOnly: chkArmBulk,
                setShowCheckedOnly: v => _showCheckedOnly = v,
                requestRefresh: null,
                setStatus: SetStatus);

            EnableDoubleBuffering(flpProfiles);

            this.SizeChanged += (s, e) => { UpdateHeaderResponsiveness(); ReflowStatus(); };

            btnSettings.Click += (s, e) =>
            {
                using var dlg = new GWxLauncher.UI.GlobalSettingsForm(_profileManager);
                dlg.ImportCompleted += (_, __) => ReloadProfilesAndViewsAfterImport();
                dlg.ProfilesBulkUpdated += (_, __) => _refresher.RequestRefresh(RefreshReason.ProfilesChanged);
                dlg.ShowDialog(this);

                // Theme may have changed; route through the unified refresher pipeline.
                _refresher.RequestRefresh(RefreshReason.ThemeChanged);
                UpdateHeaderResponsiveness();
            };
            _ui = new WinFormsUiDispatcher(this);

            _launchController = new ProfileLaunchController(
                owner: this,
                launchSession: _launchSession,
                statusBar: _statusBar,
                ui: _ui,
                gw2Orchestrator: _gw2Orchestrator,
                gw2Automation: _gw2Automation,
                gw2RunAfterLauncher: _gw2RunAfterLauncher,
                getConfig: () => _config,
                setConfig: c => _config = c,
                setStatus: SetStatus);

            var baseFont = Font;
            try
            {
                _nameFont = new Font("Segoe UI Variable", baseFont.Size + 1, FontStyle.Bold);
                _subFont = new Font("Segoe UI Variable", baseFont.Size - 1, FontStyle.Regular);
            }
            catch
            {
                _nameFont = new Font(baseFont.FontFamily, baseFont.Size + 1, FontStyle.Bold);
                _subFont = new Font(baseFont.FontFamily, baseFont.Size - 1, FontStyle.Regular);
            }

            chkArmBulk.BringToFront(); // ensure it's not obscured
            ctxProfiles.Opening += ctxProfiles_Opening;

            _config = LauncherConfig.Load();

            ThemeService.SetTheme(ParseTheme(_config.Theme));
            ThemeService.ApplyToForm(this);

            UpdateHeaderResponsiveness();

            // Separators
            panelView.Paint += (s, e) =>
            {
                var r = panelView.ClientRectangle;
                using var pen = new Pen(ThemeService.Palette.Separator);
                e.Graphics.DrawLine(pen, r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);
            };

            lblStatus.Paint += (s, e) =>
            {
                using var pen = new Pen(ThemeService.Palette.Separator);
                e.Graphics.DrawLine(pen, 0, 0, lblStatus.Width - 1, 0);
            };

            void ReflowStatus()
            {
                lblStatus.Invalidate();
            }

            var tip = new ToolTip();
            tip.SetToolTip(chkArmBulk, "Show Checked Accounts Only (Enables 'Launch All')");
            tip.SetToolTip(btnNewView, "Create New Profile");
            tip.SetToolTip(btnAddAccount, "Add Game Account");
            tip.SetToolTip(btnLaunchAll, "Launch All Armed Accounts");

            if (_config.WindowX >= 0 && _config.WindowY >= 0)
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(_config.WindowX, _config.WindowY);
            }

            if (_config.WindowWidth > 0 && _config.WindowHeight > 0)
            {
                Size = new Size(_config.WindowWidth, _config.WindowHeight);
            }

            if (_config.WindowMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }

            // Data load
            _profileManager.Load();
            _views.Load();
            _launchPolicy = new LaunchEligibilityPolicy(_views);
            _viewUi.InitializeFromStore();
            _selection = new ProfileSelectionController(setSelectedInGrid: null);

            _profileGrid = new ProfileGridController(
                panel: flpProfiles,
                isEligible: id => _views.IsEligible(_views.ActiveViewName, id),
                toggleEligible: id =>
                {
                    _views.ToggleEligible(_views.ActiveViewName, id);
                    _views.Save();
                    _refresher.RequestRefresh(RefreshReason.EligibilityChanged);
                },
                onSelected: id => _selection.Select(id),
                onDoubleClicked: id => LaunchProfileFromGrid(id),
                onRightClicked: (id, pt) => ShowProfileContextMenu(id, pt)
            );

            _selection.SetSelectedInGrid(id => _profileGrid.SetSelectedProfile(id));

            _profileGrid.InitializePanel();
            _refresher = new MainFormRefresher(
                _ui,
                refreshProfileList: RefreshProfileList,
                updateBulkArmingUi: UpdateBulkArmingUi,
                applyResponsiveProfileCardLayout: () => _profileGrid.ApplyResponsiveLayout(force: true),
                refreshTheme: () => _profileGrid.RefreshTheme());

            _viewUi.SetRequestRefresh(r => _refresher.RequestRefresh(r));

            _profileMenu = new ProfileContextMenuController(
                owner: this,
                profiles: _profileManager,
                selection: _selection,
                launchSession: _launchSession,
                refresher: _refresher,
                isShowCheckedOnly: () => _showCheckedOnly,
                setStatus: SetStatus,
                launchProfile: (p, bulkMode) => _launchController.LaunchProfile(p, bulkMode),
                trySelectProfileExecutable: TrySelectProfileExecutable,
                trySelectGw1ToolboxDll: TrySelectGw1ToolboxDll);

            this.Shown += (_, __) => _refresher.RequestRefresh(RefreshReason.Startup);

            _bulkLaunch = new BulkLaunchController(
                owner: this,
                profiles: _profileManager,
                views: _views,
                policy: _launchPolicy,
                getShowCheckedOnly: () => _showCheckedOnly,
                getConfig: () => _config,
                setConfig: c => _config = c,
                requestRefresh: r => _refresher.RequestRefresh(r),
                setStatus: SetStatus,
                updateBulkArmingUi: UpdateBulkArmingUi,
                setBulkInProgress: v => _bulkLaunchInProgress = v,
                launchProfile: (p, bulkMode) => _launchController.LaunchProfile(p, bulkMode),
                resolveEffectiveExePath: (p, cfg) => _launchController.ResolveEffectiveExePath(p, cfg));
        }

        private static AppTheme ParseTheme(string? value)
        {
            var v = (value ?? "").Trim();

            if (string.Equals(v, "Dark", StringComparison.OrdinalIgnoreCase))
                return AppTheme.Dark;

            // Default + fallback
            return AppTheme.Light;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Intentionally empty (kept for designer hook)
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // IMPORTANT: reload the latest persisted config to avoid overwriting
            // values saved by other forms (e.g., GlobalSettingsForm saving Last*Path).
            _config = LauncherConfig.Load();
            _statusBar.Dispose();

            if (WindowState == FormWindowState.Normal)
            {
                _config.WindowX = Left;
                _config.WindowY = Top;
                _config.WindowWidth = Width;
                _config.WindowHeight = Height;
                _config.WindowMaximized = false;
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                _config.WindowMaximized = true;

                // Remember where the window would be in normal state
                var bounds = RestoreBounds;
                _config.WindowX = bounds.Left;
                _config.WindowY = bounds.Top;
                _config.WindowWidth = bounds.Width;
                _config.WindowHeight = bounds.Height;
            }

            _nameFont?.Dispose();
            _subFont?.Dispose();
            _config.Theme = ThemeService.CurrentTheme.ToString();
            _config.Save();
        }

        private static void EnableDoubleBuffering(Control c)
        {
            if (c == null) return;

            try
            {
                // Control.DoubleBuffered is protected; enable it via reflection for stock FlowLayoutPanel.
                typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(c, true, null);

                // Helps reduce redraw artifacts during resize / theme swaps.
                c.GetType().GetProperty("ResizeRedraw", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(c, true, null);
            }
            catch
            {
                // best-effort; if it fails, no functional impact
            }
        }

        // -----------------------------
        // Profile list / View logic
        // -----------------------------
        private void RefreshProfileList()
        {
            _selection.EnsureSelectionValid(_profileManager.Profiles);

            IEnumerable<GameProfile> profiles = _profileManager.Profiles;

            if (_showCheckedOnly)
                profiles = profiles.Where(p => _views.IsEligible(_views.ActiveViewName, p.Id));

            _profileGrid.Rebuild(
                profiles,
                _selection.SelectedProfileId,
                _gw1Image,
                _gw2Image,
                _nameFont,
                _subFont);
        }

        private void UpdateBulkArmingUi()
        {
            // Ensure we always evaluate the latest persisted settings (e.g., after ProfileSettingsForm saves).
            _config = LauncherConfig.Load();

            // Always allow toggling this checkbox. It is a visibility control (UI-only).
            chkArmBulk.Enabled = true;

            var eval = _launchPolicy.EvaluateBulkArming(
                allProfiles: _profileManager.Profiles,
                activeViewName: _views.ActiveViewName,
                showCheckedOnly: _showCheckedOnly,
                config: _config);

            btnLaunchAll.Enabled = eval.Armed;

            // Only update status when we're idle.
            // During bulk launch, throttling + LaunchReport own the status line.
            if (!_bulkLaunchInProgress && !string.IsNullOrWhiteSpace(eval.StatusText))
            {
                SetStatus(eval.StatusText);
            }
        }

        // -----------------------------
        // Context menu / selection helpers
        // -----------------------------

        private void EditProfile(string id)
        {
            var profile = _profileManager.Profiles.FirstOrDefault(p =>
                string.Equals(p.Id, id, StringComparison.Ordinal));
            if (profile == null)
                return;

            using var dlg = new ProfileSettingsForm(profile);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _profileManager.Save();
                _refresher.RequestRefresh(RefreshReason.ProfilesChanged);
            }
        }

        private void ShowProfileContextMenu(string id, Point screenPos)
        {
            _selection.Select(id);

            // Show context menu at screen position
            ctxProfiles.Show(screenPos);
        }

        private void ReloadProfilesAndViewsAfterImport()
        {
            _profileManager.Load();
            _views.Load();

            // Re-sync view UI from the store using the controller
            _viewUi.InitializeFromStore();

            _refresher.RequestRefresh(RefreshReason.ImportCompleted);

            SetStatus("Import complete.");
        }
        private void ctxProfiles_Opening(object? sender, CancelEventArgs e)
        {
            var state = _profileMenu.GetContextMenuState();

            menuLaunchProfile.Enabled = state.HasSelectedProfile;
            menuEditProfile.Enabled = state.HasSelectedProfile;
            menuCopyProfile.Enabled = state.HasSelectedProfile;
            deleteToolStripMenuItem.Enabled = state.HasSelectedProfile;
            menuShowLastLaunchDetails.Enabled = state.CanShowLastLaunchDetails;
        }

        private void menuShowLastLaunchDetails_Click(object sender, EventArgs e)
        {
            _profileMenu.ShowLastLaunchDetails();
        }

        private void menuLaunchProfile_Click(object sender, EventArgs e)
        {
            _profileMenu.LaunchSelectedProfile();
        }
        private void menuSetProfilePath_Click(object sender, EventArgs e)
        {
            _profileMenu.SetSelectedProfilePath();
        }
        private void menuEditProfile_Click(object sender, EventArgs e)
        {
            _profileMenu.EditSelectedProfile();
        }
        private void menuCopyProfile_Click(object sender, EventArgs e)
        {
            _profileMenu.CopySelectedProfile();
        }
        private void menuDeleteProfile_Click(object sender, EventArgs e)
        {
            _profileMenu.DeleteSelectedProfile();
        }
        private void menuGw1ToolboxToggle_Click(object sender, EventArgs e)
        {
            _profileMenu.ToggleGw1Toolbox();
        }
        private void menuGw1ToolboxPath_Click(object sender, EventArgs e)
        {
            _profileMenu.SetGw1ToolboxPath();
        }

        // -----------------------------
        // View controls
        // -----------------------------

        private void chkArmBulk_CheckedChanged(object sender, EventArgs e)
        {
            _viewUi.OnShowCheckedOnlyChanged();
        }
        private void btnViewPrev_Click(object sender, EventArgs e)
        {
            _viewUi.StepView(-1);
        }
        private void btnViewNext_Click(object sender, EventArgs e)
        {
            _viewUi.StepView(+1);
        }
        private void txtView_TextChanged(object sender, EventArgs e)
        {
            _viewUi.OnViewTextChanged();
        }
        private void txtView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _viewUi.OnViewKeyDown(e);
            }
        }
        private void txtView_Leave(object sender, EventArgs e)
        {
            _viewUi.OnViewLeave();
        }
        private void txtView_Enter(object sender, EventArgs e)
        {
            _viewUi.OnViewEnter();
        }
        private void btnNewView_Click(object sender, EventArgs e)
        {
            _viewUi.CreateNewView();
        }

        // -----------------------------
        // Toolbar buttons
        // -----------------------------

        private void btnLaunchGw1_Click(object sender, EventArgs e)
        {
            LaunchGame(_config.Gw1Path, "Guild Wars 1");
        }

        private void btnLaunchGw2_Click(object sender, EventArgs e)
        {
            LaunchGame(_config.Gw2Path, "Guild Wars 2");
        }

        private async void btnLaunchAll_Click(object sender, EventArgs e)
        {
            await _bulkLaunch.LaunchAllAsync();
        }

        private void btnSetGw1Path_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Guild Wars 1 executable";
                dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

                if (!string.IsNullOrWhiteSpace(_config.Gw1Path))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(_config.Gw1Path);
                    }
                    catch
                    {
                        // ignore invalid paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _config.Gw1Path = dialog.FileName;
                    _config.Save();
                    SetStatus("GW1 path updated.");
                }
            }
        }

        private void btnSetGw2Path_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Guild Wars 2 executable";
                dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

                if (!string.IsNullOrWhiteSpace(_config.Gw2Path))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(_config.Gw2Path);
                    }
                    catch
                    {
                        // ignore invalid paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _config.Gw2Path = dialog.FileName;
                    _config.Save();
                    SetStatus("GW2 path updated.");
                }
            }
        }

        private void btnAddAcount_Click(object sender, EventArgs e)
        {
            using (var dialog = new AddAccountDialog())
            {
                var result = dialog.ShowDialog(this);
                if (result == DialogResult.OK && dialog.CreatedProfile != null)
                {
                    _profileManager.AddProfile(dialog.CreatedProfile);
                    _profileManager.Save(); // persist to profiles.json

                    _refresher.RequestRefresh(RefreshReason.ProfilesChanged);
                    SetStatus($"Added account: {dialog.CreatedProfile.Name}");
                }
            }
        }

        // -----------------------------
        // File picker helpers
        // -----------------------------

        // Lets the user pick an executable for a profile, then saves & refreshes the list.
        // Returns true if a path was chosen, false if the user cancelled.
        private bool TrySelectProfileExecutable(GameProfile profile)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = $"Select executable for {profile.Name}";
                dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

                // Start from the profile's own path if we have one…
                string initialPath = profile.ExecutablePath;

                // …otherwise fall back to the legacy global config just for convenience
                if (string.IsNullOrWhiteSpace(initialPath))
                {
                    initialPath = profile.GameType == GameType.GuildWars1
                        ? _config.Gw1Path
                        : _config.Gw2Path;
                }

                if (!string.IsNullOrWhiteSpace(initialPath))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(initialPath);
                    }
                    catch
                    {
                        // ignore bad paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (GWxLauncher.Services.ProtectedInstallPathPolicy.IsProtectedPath(dialog.FileName))
                    {
                        bool cont = GWxLauncher.UI.ProtectedInstallPathWarningDialog.ConfirmContinue(this, dialog.FileName);
                        if (!cont)
                        {
                            SetStatus($"Cancelled: protected install path for {profile.Name}.");
                            return false;
                        }
                    }
                    profile.ExecutablePath = dialog.FileName;
                    _profileManager.Save();

                    _refresher.RequestRefresh(RefreshReason.ProfilesChanged);
                    SetStatus($"Updated path for {profile.Name}.");
                    return true;
                }

                SetStatus($"No path selected for {profile.Name}.");
                return false;
            }
        }

        private bool TrySelectGw1ToolboxDll(GameProfile profile)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = $"Select GW1 Toolbox DLL for {profile.Name}";
                dialog.Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*";

                if (!string.IsNullOrWhiteSpace(profile.Gw1ToolboxDllPath))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(profile.Gw1ToolboxDllPath);
                    }
                    catch
                    {
                        // ignore bad paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    profile.Gw1ToolboxDllPath = dialog.FileName;
                    _profileManager.Save();
                    SetStatus($"Set GW1 Toolbox DLL for {profile.Name}.");
                    return true;
                }

                SetStatus($"No Toolbox DLL selected for {profile.Name}.");
                return false;
            }
        }

        // -----------------------------
        // Bulk launch throttling
        // -----------------------------

        private async Task ApplyBulkLaunchThrottlingAsync(GameProfile lastLaunchedProfile, HashSet<int>? gw1BeforePids)
        {
            if (lastLaunchedProfile == null)
                return;

            // Always use latest persisted settings.
            _config = LauncherConfig.Load();

            int requestedDelaySeconds = lastLaunchedProfile.GameType == GameType.GuildWars1
                ? _config.Gw1BulkLaunchDelaySeconds
                : _config.Gw2BulkLaunchDelaySeconds;

            // Attach throttling step to the most recent attempt's report.
            var report = _launchSession.LastReport;

            Func<bool>? readiness = null;

            if (lastLaunchedProfile.GameType == GameType.GuildWars1)
            {
                // Step 3 probe is optional; if we cannot resolve PID or init fails, policy falls back to delay-only.
                var exePath = GetEffectiveExePathForProfile(lastLaunchedProfile);

                var gw1After = CaptureProcessIdsForExePath(exePath);

                Process? gwProcess = null;
                if (gw1BeforePids != null)
                {
                    var newPids = gw1After.Except(gw1BeforePids).ToList();
                    if (newPids.Count == 1)
                    {
                        try
                        {
                            gwProcess = Process.GetProcessById(newPids[0]);
                        }
                        catch
                        {
                            gwProcess = null;
                        }
                    }
                }

                if (gwProcess != null)
                {
                    var probe = new Gw1ClientStateProbe();
                    if (probe.TryInitialize(gwProcess) && probe.IsAvailable)
                    {
                        readiness = probe.IsReady;
                    }
                    else
                    {
                        probe.Dispose();
                    }
                }
            }

            // Status callback uses the required "Throttling" language.
            Action<string> status = s =>
            {
                if (!string.IsNullOrWhiteSpace(s))
                    SetStatus(s);
            };

            await BulkLaunchThrottlingPolicy.ApplyAsync(
                gameType: lastLaunchedProfile.GameType,
                requestedDelaySeconds: requestedDelaySeconds,
                readinessCheck: readiness,
                statusCallback: status,
                report: report);

            // After throttling completes, restore the normal status text (now includes "Throttling ✓" in the summary).
            SetStatus(_launchSession.BuildStatusText());
        }

        private string GetEffectiveExePathForProfile(GameProfile profile)
        {
            if (profile == null)
                return "";

            if (!string.IsNullOrWhiteSpace(profile.ExecutablePath))
                return profile.ExecutablePath;

            // _config is already loaded in the bulk entrypoint; this reload is safe and matches existing patterns.
            _config = LauncherConfig.Load();

            return profile.GameType == GameType.GuildWars1
                ? _config.Gw1Path
                : _config.Gw2Path;
        }

        private static HashSet<int> CaptureProcessIdsForExePath(string exePath)
        {
            var set = new HashSet<int>();

            if (string.IsNullOrWhiteSpace(exePath))
                return set;

            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    // Accessing MainModule can throw; we treat failures as "not match".
                    string? path = p.MainModule?.FileName;
                    if (!string.IsNullOrWhiteSpace(path) &&
                        string.Equals(path, exePath, StringComparison.OrdinalIgnoreCase))
                    {
                        set.Add(p.Id);
                    }
                }
                catch
                {
                    // ignore
                }
                finally
                {
                    try { p.Dispose(); } catch { }
                }
            }

            return set;
        }

        // -----------------------------
        // UI helpers
        // -----------------------------

        private void LaunchProfileFromGrid(string profileId)
        {
            _selection.Select(profileId);

            var profile = _profileManager.Profiles
                .FirstOrDefault(p => string.Equals(p.Id, profileId, StringComparison.Ordinal));

            if (profile == null)
                return;

            _launchController.LaunchProfile(profile, bulkMode: false);
        }

        private void UpdateHeaderResponsiveness()
        {
            // Threshold for expansion
            bool isWide = this.Width > 480;

            // 1. Core Dimensions
            int btnWidth = isWide ? 110 : 32;
            int btnHeight = 28; // RESTORED: Separation gap returns when height is 28
            int rightPadding = 12;
            int leftPadding = 12;
            int gap = 8;

            // 2. PIN THE FAR-RIGHT GROUP (Settings & Launch)
            // Settings stays top-right
            btnSettings.Left = this.ClientSize.Width - rightPadding - btnSettings.Width;

            // Launch All aligns flush with the right edge
            btnLaunchAll.Text = isWide ? "▶ Launch All" : "▶";
            btnLaunchAll.Size = new Size(isWide ? 100 : 32, btnHeight);
            btnLaunchAll.Left = this.ClientSize.Width - rightPadding - btnLaunchAll.Width;

            // 3. POSITION THE NAVIGATION '>' (Relative to Settings)
            btnViewNext.Left = btnSettings.Left - btnViewNext.Width - gap;

            // 4. UPDATE THE LEFT GROUP (New/Add)
            btnNewView.Text = isWide ? "➕ New Profile" : "➕";
            btnNewView.Size = new Size(btnWidth, btnHeight);
            btnNewView.Left = leftPadding;

            btnAddAccount.Text = isWide ? "👤 Add Account" : "👤";
            btnAddAccount.Size = new Size(btnWidth, btnHeight);
            btnAddAccount.Left = leftPadding;

            // 5. CENTER NAVIGATION (Arrows & TextBox)
            btnViewPrev.Left = btnNewView.Right + gap;

            txtView.Left = btnViewPrev.Right + 4;
            txtView.Width = Math.Max(32, btnViewNext.Left - txtView.Left - 4);

            // 6. THE CHECKBOX ALIGNMENT FIX
            // Moving Top to 41 lifts it up to align with the buttons in row 2
            chkArmBulk.Top = 41;
            chkArmBulk.Left = btnViewPrev.Left + 2;

            // Toggle label based on width preference
            chkArmBulk.Text = isWide ? "▶ Show Checked Accounts Only" : "▶ Show Checked";
            chkArmBulk.ForeColor = ThemeService.Palette.SubtleFore;
        }

        private void SetStatus(string text)
        {
            _statusBar.SetText(text);
        }

        private void ApplyLaunchReportToUi(LaunchReport report)
        {
            _launchSession.Record(report);
            SetStatus(_launchSession.BuildStatusText());
        }

        private void LaunchGame(string exePath, string gameName)
        {
            try
            {
                // New: handle missing/blank path from config
                if (string.IsNullOrWhiteSpace(exePath))
                {
                    MessageBox.Show(
                        $"{gameName} path is not set.\n\n" +
                        "Please set the correct EXE path.",
                        "Path Not Set",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                // Existing check: path is set, but file not found on disk
                if (!File.Exists(exePath))
                {
                    MessageBox.Show(
                        $"Could not find {gameName} executable.\n\nPath:\n{exePath}",
                        "Executable Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false
                };

                _ = Process.Start(startInfo);

                if (lblStatus != null)
                    SetStatus($"{gameName} launched.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to launch {gameName}.\n\n{ex.Message}",
                    "Launch Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                if (lblStatus != null)
                    SetStatus($"Error launching {gameName}.");
            }
        }
    }
}
