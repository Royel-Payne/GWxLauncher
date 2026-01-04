using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI;
using GWxLauncher.UI.Controllers;
using System.ComponentModel;
using System.Reflection;

namespace GWxLauncher
{
    public partial class MainForm : Form
    {
        #region Fields / State

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

        #endregion

        #region Constructor

        public MainForm()
        {
            InitializeComponent();

            _statusBar = CreateStatusBarController();
            _viewUi = CreateViewUiController();

            EnableDoubleBuffering(flpProfiles);

            WireResizeHandling();
            WireSettingsButton();

            _ui = new WinFormsUiDispatcher(this);
            _launchController = CreateProfileLaunchController();

            (_nameFont, _subFont) = CreateProfileFonts(Font);

            BringToFrontAndWireContextMenu();

            _config = LoadConfigAndApplyTheme();

            WireSeparatorPaintEvents();
            InitializeTooltips();
            RestoreWindowPlacementFromConfig();

            LoadDataStores();

            _launchPolicy = new LaunchEligibilityPolicy(_views);

            _viewUi.InitializeFromStore();

            _selection = CreateSelectionController();

            _profileGrid = CreateProfileGridController();
            _profileGrid.InitializePanel();

            _refresher = CreateRefresher();
            _viewUi.SetRequestRefresh(r => _refresher.RequestRefresh(r));

            _profileMenu = CreateProfileContextMenuController();

            WireShownRefresh();

            _bulkLaunch = CreateBulkLaunchController();
        }

        #endregion

        #region Constructor helpers

        private StatusBarController CreateStatusBarController() => new StatusBarController(lblStatus);

        private ViewUiController CreateViewUiController()
        {
            return new ViewUiController(
                views: _views,
                txtView: txtView,
                chkShowCheckedOnly: chkArmBulk,
                setShowCheckedOnly: v => _showCheckedOnly = v,
                requestRefresh: null,
                setStatus: SetStatus);
        }

        private ProfileLaunchController CreateProfileLaunchController()
        {
            return new ProfileLaunchController(
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
        }

        private static (Font NameFont, Font SubFont) CreateProfileFonts(Font baseFont)
        {
            try
            {
                var nameFont = new Font("Segoe UI Variable", baseFont.Size + 1, FontStyle.Bold);
                var subFont = new Font("Segoe UI Variable", baseFont.Size - 1, FontStyle.Regular);
                return (nameFont, subFont);
            }
            catch
            {
                var nameFont = new Font(baseFont.FontFamily, baseFont.Size + 1, FontStyle.Bold);
                var subFont = new Font(baseFont.FontFamily, baseFont.Size - 1, FontStyle.Regular);
                return (nameFont, subFont);
            }
        }

        private void BringToFrontAndWireContextMenu()
        {
            chkArmBulk.BringToFront(); // ensure it's not obscured
            ctxProfiles.Opening += ctxProfiles_Opening;
        }

        private LauncherConfig LoadConfigAndApplyTheme()
        {
            var cfg = LauncherConfig.Load();

            ThemeService.SetTheme(ParseTheme(cfg.Theme));
            ThemeService.ApplyToForm(this);

            UpdateHeaderResponsiveness();

            return cfg;
        }

        private void RestoreWindowPlacementFromConfig()
        {
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
        }

        private void LoadDataStores()
        {
            // Data load
            _profileManager.Load();
            _views.Load();
        }

        private ProfileSelectionController CreateSelectionController()
        {
            // Initially null (set after _profileGrid creation)
            return new ProfileSelectionController(setSelectedInGrid: null);
        }

        private ProfileGridController CreateProfileGridController()
        {
            var grid = new ProfileGridController(
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
                onRightClicked: (id, pt) => ShowProfileContextMenu(id, pt));

            _selection.SetSelectedInGrid(id => grid.SetSelectedProfile(id));

            grid.InitializePanel();
            return grid;
        }

        private MainFormRefresher CreateRefresher()
        {
            return new MainFormRefresher(
                _ui,
                refreshProfileList: RefreshProfileList,
                updateBulkArmingUi: UpdateBulkArmingUi,
                applyResponsiveProfileCardLayout: () => _profileGrid.ApplyResponsiveLayout(force: true),
                refreshTheme: () => _profileGrid.RefreshTheme());
        }

        private ProfileContextMenuController CreateProfileContextMenuController()
        {
            return new ProfileContextMenuController(
                owner: this,
                profiles: _profileManager,
                selection: _selection,
                launchSession: _launchSession,
                refresher: _refresher,
                isShowCheckedOnly: () => _showCheckedOnly,
                setStatus: SetStatus,
                launchProfile: (p, bulkMode) => _launchController.LaunchProfile(p, bulkMode),
                trySelectProfileExecutable: TrySelectProfileExecutable);
        }

        private BulkLaunchController CreateBulkLaunchController()
        {
            return new BulkLaunchController(
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

        private void WireShownRefresh()
        {
            this.Shown += (_, __) => _refresher.RequestRefresh(RefreshReason.Startup);
        }

        private void WireResizeHandling()
        {
            this.SizeChanged += (s, e) =>
            {
                UpdateHeaderResponsiveness();
                ReflowStatus();
            };
        }

        private void WireSettingsButton()
        {
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
        }

        private void WireSeparatorPaintEvents()
        {
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
        }

        private void InitializeTooltips()
        {
            var tip = new ToolTip();
            tip.SetToolTip(chkArmBulk, "Show Checked Accounts Only (Enables 'Launch All')");
            tip.SetToolTip(btnNewView, "Create New Profile");
            tip.SetToolTip(btnAddAccount, "Add Game Account");
            tip.SetToolTip(btnLaunchAll, "Launch All Armed Accounts");
        }

        private void ReflowStatus() => lblStatus.Invalidate();

        #endregion

        #region Theme parsing / Form closing

        private static AppTheme ParseTheme(string? value)
        {
            var v = (value ?? "").Trim();

            if (string.Equals(v, "Dark", StringComparison.OrdinalIgnoreCase))
                return AppTheme.Dark;

            // Default + fallback
            return AppTheme.Light;
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
            if (c == null)
                return;

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

        #endregion

        #region Profile list / View logic

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

        private void ReloadProfilesAndViewsAfterImport()
        {
            _profileManager.Load();
            _views.Load();

            // Re-sync view UI from the store using the controller
            _viewUi.InitializeFromStore();

            _refresher.RequestRefresh(RefreshReason.ImportCompleted);
            SetStatus("Import complete.");
        }

        #endregion

        #region Context menu / selection handlers

        private void ShowProfileContextMenu(string id, Point screenPos)
        {
            _selection.Select(id);

            // Show context menu at screen position
            ctxProfiles.Show(screenPos);
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

        private void menuShowLastLaunchDetails_Click(object sender, EventArgs e) => _profileMenu.ShowLastLaunchDetails();

        private void menuLaunchProfile_Click(object sender, EventArgs e) => _profileMenu.LaunchSelectedProfile();

        private void menuEditProfile_Click(object sender, EventArgs e) => _profileMenu.EditSelectedProfile();

        private void menuCopyProfile_Click(object sender, EventArgs e) => _profileMenu.CopySelectedProfile();

        private void menuDeleteProfile_Click(object sender, EventArgs e) => _profileMenu.DeleteSelectedProfile();

        #endregion

        #region View controls

        private void chkArmBulk_CheckedChanged(object sender, EventArgs e) => _viewUi.OnShowCheckedOnlyChanged();

        private void btnViewPrev_Click(object sender, EventArgs e) => _viewUi.StepView(-1);

        private void btnViewNext_Click(object sender, EventArgs e) => _viewUi.StepView(+1);

        private void txtView_TextChanged(object sender, EventArgs e) => _viewUi.OnViewTextChanged();

        private void txtView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                _viewUi.OnViewKeyDown(e);
        }

        private void txtView_Leave(object sender, EventArgs e) => _viewUi.OnViewLeave();

        private void txtView_Enter(object sender, EventArgs e) => _viewUi.OnViewEnter();

        private void btnNewView_Click(object sender, EventArgs e) => _viewUi.CreateNewView();

        #endregion

        #region Toolbar buttons

        private async void btnLaunchAll_Click(object sender, EventArgs e) => await _bulkLaunch.LaunchAllAsync();

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

        #endregion

        #region File picker helpers

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

        #endregion

        #region UI helpers

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
            var layout = HeaderLayout.Compute(
                formWidth: this.Width,
                clientWidth: this.ClientSize.Width,
                settingsWidth: btnSettings.Width,
                viewNextWidth: btnViewNext.Width,
                viewPrevWidth: btnViewPrev.Width);

            btnSettings.Left = layout.SettingsLeft;

            btnLaunchAll.Text = layout.LaunchAllText;
            btnLaunchAll.Size = layout.LaunchAllSize;
            btnLaunchAll.Left = layout.LaunchAllLeft;

            btnViewNext.Left = layout.ViewNextLeft;

            btnNewView.Text = layout.NewProfileText;
            btnNewView.Size = layout.NewProfileSize;
            btnNewView.Left = layout.NewProfileLeft;

            btnAddAccount.Text = layout.AddAccountText;
            btnAddAccount.Size = layout.AddAccountSize;
            btnAddAccount.Left = layout.AddAccountLeft;

            btnViewPrev.Left = layout.ViewPrevLeft;

            txtView.Left = layout.ViewTextLeft;
            txtView.Width = layout.ViewTextWidth;

            chkArmBulk.Top = layout.ShowCheckedTop;
            chkArmBulk.Left = layout.ShowCheckedLeft;
            chkArmBulk.Text = layout.ShowCheckedText;
            chkArmBulk.ForeColor = ThemeService.Palette.SubtleFore;
        }

        private void SetStatus(string text) => _statusBar.SetText(text);

        #endregion

        #region Header layout helper

        private readonly struct HeaderLayout
        {
            public int SettingsLeft { get; }
            public string LaunchAllText { get; }
            public Size LaunchAllSize { get; }
            public int LaunchAllLeft { get; }

            public int ViewNextLeft { get; }

            public string NewProfileText { get; }
            public Size NewProfileSize { get; }
            public int NewProfileLeft { get; }

            public string AddAccountText { get; }
            public Size AddAccountSize { get; }
            public int AddAccountLeft { get; }

            public int ViewPrevLeft { get; }
            public int ViewTextLeft { get; }
            public int ViewTextWidth { get; }

            public int ShowCheckedTop { get; }
            public int ShowCheckedLeft { get; }
            public string ShowCheckedText { get; }

            private HeaderLayout(
                int settingsLeft,
                string launchAllText,
                Size launchAllSize,
                int launchAllLeft,
                int viewNextLeft,
                string newProfileText,
                Size newProfileSize,
                int newProfileLeft,
                string addAccountText,
                Size addAccountSize,
                int addAccountLeft,
                int viewPrevLeft,
                int viewTextLeft,
                int viewTextWidth,
                int showCheckedTop,
                int showCheckedLeft,
                string showCheckedText)
            {
                SettingsLeft = settingsLeft;
                LaunchAllText = launchAllText;
                LaunchAllSize = launchAllSize;
                LaunchAllLeft = launchAllLeft;
                ViewNextLeft = viewNextLeft;
                NewProfileText = newProfileText;
                NewProfileSize = newProfileSize;
                NewProfileLeft = newProfileLeft;
                AddAccountText = addAccountText;
                AddAccountSize = addAccountSize;
                AddAccountLeft = addAccountLeft;
                ViewPrevLeft = viewPrevLeft;
                ViewTextLeft = viewTextLeft;
                ViewTextWidth = viewTextWidth;
                ShowCheckedTop = showCheckedTop;
                ShowCheckedLeft = showCheckedLeft;
                ShowCheckedText = showCheckedText;
            }

            public static HeaderLayout Compute(int formWidth, int clientWidth, int settingsWidth, int viewNextWidth, int viewPrevWidth)
            {
                // Threshold for expansion
                bool isWide = formWidth > 480;

                // 1. Core Dimensions
                int btnWidth = isWide ? 110 : 32;
                int btnHeight = 28; // RESTORED: Separation gap returns when height is 28
                int rightPadding = 12;
                int leftPadding = 12;
                int gap = 8;

                // 2. PIN THE FAR-RIGHT GROUP (Settings & Launch)
                int settingsLeft = clientWidth - rightPadding - settingsWidth;

                string launchAllText = isWide ? "▶ Launch All" : "▶";
                var launchAllSize = new Size(isWide ? 100 : 32, btnHeight);
                int launchAllLeft = clientWidth - rightPadding - launchAllSize.Width;

                // 3. POSITION THE NAVIGATION '>' (Relative to Settings)
                int viewNextLeft = settingsLeft - viewNextWidth - gap;

                // 4. UPDATE THE LEFT GROUP (New/Add)
                string newProfileText = isWide ? "➕ New Profile" : "➕";
                var newProfileSize = new Size(btnWidth, btnHeight);
                int newProfileLeft = leftPadding;
                int newProfileRight = newProfileLeft + newProfileSize.Width;

                string addAccountText = isWide ? "👤 Add Account" : "👤";
                var addAccountSize = new Size(btnWidth, btnHeight);
                int addAccountLeft = leftPadding;

                // 5. CENTER NAVIGATION (Arrows & TextBox)
                int viewPrevLeft = newProfileRight + gap;
                int viewPrevRight = viewPrevLeft + viewPrevWidth;

                int viewTextLeft = viewPrevRight + 4;
                int viewTextWidth = Math.Max(32, viewNextLeft - viewTextLeft - 4);

                // 6. THE CHECKBOX ALIGNMENT FIX
                int showCheckedTop = 41;
                int showCheckedLeft = viewPrevLeft + 2;

                string showCheckedText = isWide ? "▶ Show Checked Accounts Only" : "▶ Show Checked";

                return new HeaderLayout(
                    settingsLeft,
                    launchAllText,
                    launchAllSize,
                    launchAllLeft,
                    viewNextLeft,
                    newProfileText,
                    newProfileSize,
                    newProfileLeft,
                    addAccountText,
                    addAccountSize,
                    addAccountLeft,
                    viewPrevLeft,
                    viewTextLeft,
                    viewTextWidth,
                    showCheckedTop,
                    showCheckedLeft,
                    showCheckedText);
            }
        }

        #endregion
    }
}
