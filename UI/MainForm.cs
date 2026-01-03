using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Reflection;


namespace GWxLauncher
{
    public partial class MainForm : Form
    {
        // -----------------------------
        // Fields / State
        // -----------------------------

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
        private string _viewNameBeforeEdit = "";
        private bool _viewNameDirty = false;
        private bool _suppressViewTextEvents = false;
        private bool _suppressArmBulkEvents = false;
        private bool _bulkLaunchInProgress = false;

        private readonly Font _nameFont;
        private readonly Font _subFont;

        private string? _selectedProfileId = null;
        // Responsive card layout tuning
        private const int CardOuterPad = 6;     // panel padding around the grid
        private const int CardGap = 6;          // spacing between cards (horizontal + vertical)
        private const int CardMinWidth = 230;   // minimum width before adding another column
        private const int CardMaxWidth = 520;   // cards expand until this, then new column is allowed
        private const int CardPreferredWidth = 340; // if another column still allows >= this width, add it

        // Reserve most of the scrollbar width to prevent wrap oscillation,
        // but reclaim a few pixels so the right gutter doesn't look oversized.
        private const int ScrollbarReserve = 10; // subtract slightly less than full scrollbar width

        private int _lastProfileLayoutWidth = -1;

        // -----------------------------
        // Status marquee (for long lblStatus text)
        // -----------------------------
        private readonly System.Windows.Forms.Timer _statusMarqueeTimer = new System.Windows.Forms.Timer();
        private string _statusFullText = "";
        private int _statusMarqueeIndex = 0;
        private const int StatusMarqueeIntervalMs = 120;
        private const string StatusMarqueeGap = "   •   ";

        // -----------------------------
        // Ctor / Form lifecycle
        // -----------------------------

        public MainForm()
        {
            InitializeComponent();
            EnableDoubleBuffering(flpProfiles);

            // Handle the resize on the form itself to trigger the reflow
            this.SizeChanged += (s, e) => {
                ReflowStatus();
            };
            this.SizeChanged += (s, e) => {
                UpdateHeaderResponsiveness();
                ReflowStatus();
            };
            btnSettings.Click += (s, e) =>
            {
                using var dlg = new GWxLauncher.UI.GlobalSettingsForm(_profileManager);
                dlg.ImportCompleted += (_, __) => ReloadProfilesAndViewsAfterImport();
                dlg.ProfilesBulkUpdated += (_, __) => RefreshProfileList();
                dlg.ShowDialog(this);
            };

            _statusMarqueeTimer.Interval = StatusMarqueeIntervalMs;
            _statusMarqueeTimer.Tick += (s, e) => TickStatusMarquee();
            _ui = new WinFormsUiDispatcher(this);

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

            // Apply persisted theme BEFORE styling anything.
            ThemeService.SetTheme(ParseTheme(_config.Theme));
            ThemeService.ApplyToForm(this);

            // Force a clean repaint of the card surface to prevent any stale pixels
            panelProfiles.Invalidate(true);
            flpProfiles.Invalidate(true);

            // And each card too (covers hover/selection borders)
            foreach (Control c in flpProfiles.Controls)
                c.Invalidate(true);

            Update();

            flpProfiles.SuspendLayout();
            flpProfiles.ResumeLayout(true);
            flpProfiles.Update();

            RefreshProfileCardTheme();
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
                _statusMarqueeIndex = 0;
                UpdateStatusMarquee();
                lblStatus.Invalidate();
            }

            //// View label / tooltip
            //chkArmBulk.Visible = true;
            //chkArmBulk.Text = "Show Checked \nAccounts Only";
            //chkArmBulk.AutoSize = true;
            //chkArmBulk.ForeColor = ThemeService.Palette.SubtleFore;

            var tip = new ToolTip();
            tip.SetToolTip(chkArmBulk, "Show Checked Accounts Only (Enables 'Launch All')");
            // Add tooltips for the new icon buttons so their purpose remains clear
            tip.SetToolTip(btnNewView, "Create New Profile");
            tip.SetToolTip(btnAddAccount, "Add Game Account");
            tip.SetToolTip(btnLaunchAll, "Launch All Armed Accounts");

            // Window-position restore
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

            // View -> UI sync
            _suppressViewTextEvents = true;
            txtView.Text = _views.ActiveViewName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;
            ConfigureProfilesFlowPanel();

            RefreshProfileList();
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
            _statusMarqueeTimer.Stop();

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
            System.Diagnostics.Debug.WriteLine("RefreshProfileList called:\n" + Environment.StackTrace);

            var selectedId = _selectedProfileId;

            flpProfiles.SuspendLayout();
            try
            {
                flpProfiles.Controls.Clear();

                var profiles = _profileManager.Profiles
                    .OrderBy(p => p.GameType) // GW1 before GW2
                    .ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
                    .AsEnumerable();

                // Visibility is controlled ONLY by Show Checked Accounts Only (view-scoped)
                if (_showCheckedOnly)
                    profiles = profiles.Where(p => _views.IsEligible(_views.ActiveViewName, p.Id));

                foreach (var profile in profiles)
                {
                    var card = new ProfileCardControl(profile, _gw1Image, _gw2Image, _nameFont, _subFont);
                    card.IsEligible = id => _views.IsEligible(_views.ActiveViewName, id);
                    card.ToggleEligible = id =>
                    {
                        _views.ToggleEligible(_views.ActiveViewName, id);
                        _views.Save();

                        if (_showCheckedOnly)
                        {
                            RefreshProfileList(); // eligibility affects visibility
                        }
                        else
                        {
                            card.Invalidate(); // cheap repaint
                        }

                        UpdateBulkArmingUi();
                    };

                    card.Clicked += (_, __) =>
                    {
                        _selectedProfileId = profile.Id;
                        UpdateCardSelectionVisuals();
                    };

                    card.DoubleClicked += (_, __) =>
                    {
                        _selectedProfileId = profile.Id;
                        UpdateCardSelectionVisuals();
                        menuLaunchProfile_Click(this, EventArgs.Empty);
                    };

                    card.RightClicked += (_, e) =>
                    {
                        _selectedProfileId = profile.Id;
                        UpdateCardSelectionVisuals();
                        ctxProfiles.Show(card, e.Location);
                    };

                    // Keep selection visual on rebuild
                    card.SetSelected(selectedId != null && selectedId == profile.Id);

                    flpProfiles.Controls.Add(card);
                }

                UpdateBulkArmingUi();
            }
            finally
            {
                flpProfiles.ResumeLayout(true);
            }
            // Force responsive layout after rebuilding cards.
            // The width may not have changed, but the card set did.
            _lastProfileLayoutWidth = -1;

            if (IsHandleCreated)
                BeginInvoke(new Action(ApplyResponsiveProfileCardLayout));
        }

        private void UpdateCardSelectionVisuals()
        {
            foreach (Control c in flpProfiles.Controls)
            {
                if (c is ProfileCardControl card)
                    card.SetSelected(_selectedProfileId != null && card.Profile.Id == _selectedProfileId);
            }
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

        private void ApplyViewScopedUiState()
        {
            _showCheckedOnly = _views.GetShowCheckedOnly(_views.ActiveViewName);

            _suppressArmBulkEvents = true;
            chkArmBulk.Checked = _showCheckedOnly;
            _suppressArmBulkEvents = false;
        }

        private void StepView(int delta)
        {
            // Step through saved View names (Team1/Team2/Team3), not profiles
            var newName = _views.StepActiveView(delta);

            // Persist the new ActiveViewName so views.json reflects the user's selection.
            _views.Save();

            _suppressViewTextEvents = true;
            txtView.Text = newName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;

            // Switching view changes which profiles are checked (per-view),
            // but does not hide profiles by name.
            RefreshProfileList();
        }

        private void CommitViewRenameIfDirty()
        {
            if (!_viewNameDirty)
                return;

            _viewNameDirty = false;

            string newName = (txtView.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                // Revert to old name if user blanked it
                _suppressViewTextEvents = true;
                txtView.Text = _views.ActiveViewName;
                _suppressViewTextEvents = false;
                return;
            }

            // Try rename. If it fails (duplicate name), revert & show a status hint.
            if (!_views.RenameActiveView(newName))
            {
                _suppressViewTextEvents = true;
                txtView.Text = _views.ActiveViewName;
                _suppressViewTextEvents = false;

                SetStatus("View rename failed (name already exists).");
                return;
            }

            _views.Save();
            RefreshProfileList();
        }

        // -----------------------------
        // Layout helpers
        // -----------------------------

        // Keep FlowLayoutPanel behaving consistently
        private void ConfigureProfilesFlowPanel()
        {
            flpProfiles.WrapContents = true;
            flpProfiles.FlowDirection = FlowDirection.LeftToRight;
            flpProfiles.AutoScroll = true;

            // Outer gutter; card spacing handled via per-card Margin.
            flpProfiles.Padding = new Padding(CardOuterPad);

            // Relayout when width changes
            flpProfiles.ClientSizeChanged += (_, __) =>
            {
                // Defer until WinForms finishes internal size/scroll calculations
                if (IsHandleCreated)
                    BeginInvoke(new Action(ApplyResponsiveProfileCardLayout));
            };
        }

        private void ApplyResponsiveProfileCardLayout()
        {
            if (!flpProfiles.IsHandleCreated)
                return;

            // Avoid churn when height changes (scrollbar, etc.) and only relayout on width change.
            int w = flpProfiles.ClientSize.Width;
            if (w <= 0 || w == _lastProfileLayoutWidth)
                return;

            _lastProfileLayoutWidth = w;

            var cards = flpProfiles.Controls.OfType<ProfileCardControl>().ToList();
            if (cards.Count == 0)
                return;

            int sbW = SystemInformation.VerticalScrollBarWidth;

            // Reserve *most* of the scrollbar width to keep transitions stable,
            // but don't waste the entire width as dead gutter.
            int reserve = Math.Max(0, sbW - ScrollbarReserve);
            int availW = Math.Max(0, flpProfiles.ClientSize.Width - (CardOuterPad * 2) - reserve);

            var (_, finalCardW) = ComputeGrid(availW);

            flpProfiles.SuspendLayout();
            try
            {
                flpProfiles.Padding = new Padding(CardOuterPad);

                // Consistent spacing without a fat right gutter:
                // split horizontal gap across both sides.
                int half = Math.Max(0, CardGap / 2);
                var margin = new Padding(half, 0, half, CardGap);

                foreach (var c in cards)
                {
                    c.Width = finalCardW;
                    c.Margin = margin;
                }
            }
            finally
            {
                flpProfiles.ResumeLayout(true);
            }
        }


        private (int columns, int cardWidth) ComputeGrid(int availableWidth)
        {
            if (availableWidth <= 0)
                return (1, CardMinWidth);

            // Start with as many columns as we can fit at minimum width
            int cols = Math.Max(1, (availableWidth + CardGap) / (CardMinWidth + CardGap));

            // Compute ideal width for that column count
            double ideal = (availableWidth - (CardGap * (cols - 1))) / (double)cols;

            // Add columns as soon as the next column still keeps cards at a "comfortable" width.
            // This makes columns appear earlier (not only when we hit max width).
            while (true)
            {
                int nextCols = cols + 1;
                double nextIdeal = (availableWidth - (CardGap * (nextCols - 1))) / (double)nextCols;

                // Stop if another column would make cards too narrow
                if (nextIdeal < CardMinWidth)
                    break;

                // Only add the column if cards would still be at least the preferred width
                if (nextIdeal < CardPreferredWidth)
                    break;

                cols = nextCols;
                ideal = nextIdeal;
            }

            int w = (int)Math.Floor(ideal);
            if (w < CardMinWidth) w = CardMinWidth;
            if (w > CardMaxWidth) w = CardMaxWidth;

            return (cols, w);
        }

        // -----------------------------
        // Context menu / selection helpers
        // -----------------------------

        private void ReloadProfilesAndViewsAfterImport()
        {
            _profileManager.Load();
            _views.Load();

            // Update view UI (same idea as startup)
            _suppressViewTextEvents = true;
            txtView.Text = _views.ActiveViewName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;

            RefreshProfileList();
            UpdateBulkArmingUi();

            SetStatus("Import complete.");
        }

        private GameProfile? GetSelectedProfile()
        {
            if (string.IsNullOrWhiteSpace(_selectedProfileId))
                return null;

            return _profileManager.Profiles.FirstOrDefault(p => p.Id == _selectedProfileId);
        }

        private void ctxProfiles_Opening(object? sender, CancelEventArgs e)
        {
            var profile = GetSelectedProfile();
            bool hasProfile = profile != null;

            menuLaunchProfile.Enabled = hasProfile;
            menuEditProfile.Enabled = hasProfile;
            menuCopyProfile.Enabled = hasProfile;
            deleteToolStripMenuItem.Enabled = hasProfile;
            menuShowLastLaunchDetails.Enabled = _launchSession.HasAnyReports;
        }

        private void menuShowLastLaunchDetails_Click(object sender, EventArgs e)
        {
            if (!_launchSession.HasAnyReports)
                return;

            var dlg = new LastLaunchDetailsForm(_launchSession.AllReports);
            dlg.ShowDialog(this);
        }

        private void menuLaunchProfile_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile != null)
            {
                LaunchProfile(profile, bulkMode: false);
            }
        }

        private void menuSetProfilePath_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null)
                return;

            TrySelectProfileExecutable(profile);
        }

        private void menuEditProfile_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null)
                return;

            using var dlg = new ProfileSettingsForm(profile);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _profileManager.Save();
                RefreshProfileList();
            }
        }
        private void menuCopyProfile_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null)
                return;

            var copied = _profileManager.CopyProfile(profile);

            // Intentionally unchecked in all views:
            // ViewStateStore returns false for unknown profile IDs, so no entry is created here.

            if (_showCheckedOnly)
            {
                MessageBox.Show(
                    this,
                    "Profile copied.\n\nIt starts unchecked in all views, so it may be hidden while \"Show Checked Accounts Only\" is enabled.\nDisable that option to see the new profile.",
                    "Copy Profile",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            _selectedProfileId = copied.Id;
            RefreshProfileList();
            UpdateCardSelectionVisuals();

            SetStatus($"Copied profile: {profile.Name} → {copied.Name}");
        }

        private void menuDeleteProfile_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null)
                return;

            var result = MessageBox.Show(
                $"Delete account \"{profile.Name}\"?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _profileManager.RemoveProfile(profile);
                _profileManager.Save();
                RefreshProfileList();
                SetStatus($"Deleted account: {profile.Name}.");
            }
        }

        private void menuGw1ToolboxToggle_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null || profile.GameType != GameType.GuildWars1)
            {
                // Safety: if somehow clicked with no GW1 profile, just uncheck.
                // menuGw1ToolboxToggle.Checked = false;
                return;
            }

            _profileManager.Save();

            SetStatus(
                $"GW1 Toolbox injection " +
                (profile.Gw1ToolboxEnabled ? "enabled" : "disabled") +
                $" for {profile.Name}.");
        }

        private void menuGw1ToolboxPath_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null || profile.GameType != GameType.GuildWars1)
                return;

            TrySelectGw1ToolboxDll(profile);
        }
        private void RefreshProfileCardTheme()
        {
            // Force full repaint of custom-drawn cards after theme change
            flpProfiles.SuspendLayout();

            foreach (Control c in flpProfiles.Controls)
            {
                if (c is ProfileCardControl card)
                {
                    card.Invalidate();
                    card.Update(); // flush paint immediately to avoid ghosting
                }
            }

            flpProfiles.ResumeLayout(true);
        }

        // -----------------------------
        // View controls
        // -----------------------------

        private void chkArmBulk_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressArmBulkEvents)
                return;

            _showCheckedOnly = chkArmBulk.Checked;

            _views.SetShowCheckedOnly(_views.ActiveViewName, _showCheckedOnly);
            _views.Save();

            RefreshProfileList();
        }

        private void btnViewPrev_Click(object sender, EventArgs e)
        {
            StepView(-1);
        }

        private void btnViewNext_Click(object sender, EventArgs e)
        {
            StepView(+1);
        }

        private void txtView_TextChanged(object sender, EventArgs e)
        {
            if (_suppressViewTextEvents)
                return;

            // Don’t mutate view store during typing.
            _viewNameDirty = true;
        }

        private void txtView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                CommitViewRenameIfDirty();
            }
        }

        private void txtView_Leave(object sender, EventArgs e)
        {
            CommitViewRenameIfDirty();
        }

        private void txtView_Enter(object sender, EventArgs e)
        {
            _viewNameBeforeEdit = _views.ActiveViewName;
            _viewNameDirty = false;
        }

        private void btnNewView_Click(object sender, EventArgs e)
        {
            var newName = _views.CreateNewView("New View");
            _views.Save();

            _suppressViewTextEvents = true;
            txtView.Text = newName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;

            RefreshProfileList();
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
            // Always evaluate the latest persisted settings (the settings form saves its own config instance).
            _config = LauncherConfig.Load();

            var eval = _launchPolicy.EvaluateBulkArming(
                allProfiles: _profileManager.Profiles,
                activeViewName: _views.ActiveViewName,
                showCheckedOnly: _showCheckedOnly,
                config: _config);

            bool armed = eval.Armed;

            if (!armed)
            {
                SetStatus($"Bulk launch not armed · View: {_views.ActiveViewName}");
                return;
            }

            var targets = _launchPolicy.BuildBulkTargets(_profileManager.Profiles, _views.ActiveViewName);

            // Guard: multiclient must be enabled for the eligible game type(s) or bulk launch won't work.
            // Reload config first so this reflects changes made in ProfileSettingsForm without restarting MainForm.
            if (!_launchPolicy.IsMulticlientEnabledForEligible(_profileManager.Profiles, _views.ActiveViewName, _config, out string missing))
            {
                string msg =
                    "Multiclient not enabled.\n\n" +
                    $"Not enabled for: {missing}\n\n" +
                    "Enable it now and continue?";

                var result = MessageBox.Show(this, msg, "Multiclient not enabled", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result != DialogResult.OK)
                {
                    SetStatus($"Bulk launch canceled · Multiclient missing: {missing}");
                    return;
                }

                _launchPolicy.EnableRequiredMulticlientFlagsForEligible(_profileManager.Profiles, _views.ActiveViewName, _config);

                _config.Save();

                // Refresh state after saving so any subsequent checks see the latest values.
                _config = LauncherConfig.Load();
                RefreshProfileList();
                UpdateBulkArmingUi();
            }

            if (targets.Count == 0)
            {
                SetStatus($"No checked profiles in view · View: {_views.ActiveViewName}");
                return;
            }

            _launchSession.BeginSession(bulkMode: true);

            // Prevent re-entrancy while bulk launch is running.
            _bulkLaunchInProgress = true;
            btnLaunchAll.Enabled = false;
            try
            {
                // Run GW2 launches off the UI thread to avoid paint starvation (GW2 automation contains sleeps/polling).
                // GW1 launches remain on UI thread (fast, minimal waiting), but can be moved later if needed.
                for (int i = 0; i < targets.Count; i++)
                {
                    var profile = targets[i];

                    // For GW1 readiness probing, take a pre-launch snapshot so we can identify the new PID.
                    var gw1Before = profile.GameType == GameType.GuildWars1
                        ? CaptureProcessIdsForExePath(GetEffectiveExePathForProfile(profile))
                        : null;

                    if (profile.GameType == GameType.GuildWars2)
                    {
                        // Run GW2 orchestration/automation off the UI thread; return the result so UI can apply it deterministically.
                        var gw2Result = await Task.Run(() => LaunchProfileGw2BulkWorker(profile));

                        if (gw2Result.Report != null)
                            ApplyLaunchReportToUi(gw2Result.Report);

                        if (gw2Result.HasMessageBox)
                        {
                            MessageBox.Show(
                                this,
                                gw2Result.MessageBoxText,
                                gw2Result.MessageBoxTitle,
                                MessageBoxButtons.OK,
                                gw2Result.MessageBoxIsError ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        // GW1 is fast and stays on the UI thread.
                        LaunchProfile(profile, bulkMode: true);
                    }

                    bool hasNext = (i < targets.Count - 1);

                    // Throttling is only meaningful when there is another account queued.
                    if (hasNext)
                        await ApplyBulkLaunchThrottlingAsync(profile, gw1Before);

                    // Give WinForms a chance to repaint between profiles.
                    await Task.Yield();
                }
            }
            finally
            {
                _bulkLaunchInProgress = false;
                UpdateBulkArmingUi();
            }
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

                    RefreshProfileList();
                    UpdateBulkArmingUi();
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

                    RefreshProfileList();
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
        // Launching
        // -----------------------------

        private void LaunchProfile(GameProfile profile, bool bulkMode)
        {
            if (profile == null)
                return;

            // Single launch = new "session". Bulk launch session is started once in btnLaunchAll_Click.
            if (!bulkMode)
                _launchSession.BeginSession(bulkMode: false);

            _config = LauncherConfig.Load();

            string exePath = profile.ExecutablePath;

            if (string.IsNullOrWhiteSpace(exePath))
            {
                exePath = profile.GameType == GameType.GuildWars1
                    ? _config.Gw1Path
                    : _config.Gw2Path;
            }

            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                MessageBox.Show(
                    "No valid executable path is configured for this profile.\n\n" +
                    "Edit the profile or configure the game path in settings.",
                    "Missing executable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Re-warn at launch time (warn-only, no elevation, user can continue)
            if (GWxLauncher.Services.ProtectedInstallPathPolicy.IsProtectedPath(exePath))
            {
                bool cont = GWxLauncher.UI.ProtectedInstallPathWarningDialog.ConfirmContinue(this, exePath);
                if (!cont)
                {
                    SetStatus($"Launch cancelled: protected install path for {profile.Name}.");
                    return;
                }
            }

            // GW1: delegate launch + injection to Gw1InjectionService
            if (profile.GameType == GameType.GuildWars1)
            {
                var gw1Service = new Gw1InjectionService();
                bool mcEnabled = _config.Gw1MulticlientEnabled;

                if (gw1Service.TryLaunchGw1(profile, exePath, mcEnabled, this, out var gw1Error, out var report))
                {
                    ApplyLaunchReportToUi(report);
                }
                else
                {
                    MessageBox.Show(
                        this,
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
                bool mcEnabled = _config.Gw2MulticlientEnabled;

                var result = _gw2Orchestrator.Launch(
                    profile: profile,
                    exePath: exePath,
                    mcEnabled: mcEnabled,
                    bulkMode: bulkMode,
                    automationCoordinator: _gw2Automation,
                    runAfterInvoker: _gw2RunAfterLauncher.Start);

                if (result.Report != null)
                {
                    GWxLauncher.Services.ProtectedInstallPathPolicy.TryAppendLaunchReportNote(result.Report, exePath);
                    ApplyLaunchReportToUi(result.Report);
                }

                if (result.HasMessageBox)
                {
                    MessageBox.Show(
                        this,
                        result.MessageBoxText,
                        result.MessageBoxTitle,
                        MessageBoxButtons.OK,
                        result.MessageBoxIsError ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
                }

                return;
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
            SafeUi(() =>
            {
                _statusFullText = text ?? "";
                _statusMarqueeIndex = 0;
                UpdateStatusMarquee();
            });
        }

        private void SafeUi(Action action)
        {
            _ui.Post(action);
        }
        private void UpdateStatusMarquee()
        {
            if (lblStatus == null) return;

            string full = _statusFullText ?? "";

            if (StatusTextFits(full))
            {
                StopStatusMarquee();
                lblStatus.Text = full;
            }
            else
            {
                StartStatusMarquee();
                // Force the first slice immediately
                lblStatus.Text = BuildMarqueeSlice(full, _statusMarqueeIndex);
            }
        }

        private bool StatusTextFits(string text)
        {
            if (string.IsNullOrEmpty(text)) return true;

            // Measure with specific flags to ensure we catch precise pixel widths
            var size = TextRenderer.MeasureText(text, lblStatus.Font,
                new Size(int.MaxValue, lblStatus.Height),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            // Available width inside the label minus the padding you set in the designer (6)
            int padding = lblStatus.Padding.Left + lblStatus.Padding.Right;

            // We use ClientSize.Width to get the internal area 
            int available = Math.Max(0, lblStatus.ClientSize.Width - padding);

            return size.Width <= available;
        }

        private void StartStatusMarquee()
        {
            if (!_statusMarqueeTimer.Enabled)
                _statusMarqueeTimer.Start();
        }

        private void StopStatusMarquee()
        {
            if (_statusMarqueeTimer.Enabled)
                _statusMarqueeTimer.Stop();
        }

        private void TickStatusMarquee()
        {
            if (lblStatus == null) return;

            string full = _statusFullText ?? "";

            if (StatusTextFits(full))
            {
                StopStatusMarquee();
                lblStatus.Text = full;
                return;
            }

            _statusMarqueeIndex++;

            // Safety check for empty strings
            if (string.IsNullOrEmpty(full)) return;

            int loopLen = (full + StatusMarqueeGap).Length;
            if (_statusMarqueeIndex >= loopLen) _statusMarqueeIndex = 0;

            // Direct text assignment triggers the repaint
            lblStatus.Text = BuildMarqueeSlice(full, _statusMarqueeIndex);
        }

        private static string BuildMarqueeSlice(string full, int index)
        {
            // Simple, stable marquee: rotate across "full + gap + full"
            string combined = full + StatusMarqueeGap + full;

            if (combined.Length == 0)
                return "";

            index %= (full + StatusMarqueeGap).Length;
            if (index < 0) index = 0;

            // Return from index to end (label will clip naturally)
            return combined.Substring(index);
        }

        private void ApplyLaunchReportToUi(LaunchReport report)
        {
            _launchSession.Record(report);
            SetStatus(_launchSession.BuildStatusText());
        }

        /// <summary>
        /// Bulk-launch worker for GW2 that runs off the UI thread so the listbox/cards can repaint
        /// while GW2 mutex handling + automation (polling/sleeps/pixel checks) are running.
        /// UI updates are marshaled back via SafeUi().
        /// </summary>
        private Gw2LaunchOrchestrator.Gw2LaunchResult LaunchProfileGw2BulkWorker(GameProfile profile)
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
