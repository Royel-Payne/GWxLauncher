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

        private readonly Font _nameFont;
        private readonly Font _subFont;

        private int _hotIndex = -1;

        private const bool ShowSelectionBorder = false;

        // -----------------------------
        // Ctor / Form lifecycle
        // -----------------------------

        public MainForm()
        {
            InitializeComponent();
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

            // Hover tracking for custom list drawing
            lstProfiles.MouseMove += lstProfiles_MouseMove;
            lstProfiles.MouseLeave += lstProfiles_MouseLeave;

            chkArmBulk.BringToFront(); // ensure it's not obscured
            ctxProfiles.Opening += ctxProfiles_Opening;

            _config = LauncherConfig.Load();

            ThemeService.ApplyToForm(this);

            ReenableListScrollbarAndFillPanel();

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
            lblStatus.Resize += (s, e) => lblStatus.Invalidate();

            panelProfiles.Resize += (s, e) => panelProfiles.Invalidate();

            // View label / tooltip
            lblView.Visible = true;
            lblView.Text = "Show Checked \nAccounts Only";
            lblView.AutoSize = true;
            lblView.ForeColor = ThemeService.Palette.SubtleFore;

            var tip = new ToolTip();
            tip.SetToolTip(chkArmBulk, "Show Checked Accounts Only (enables launch all)");
            tip.SetToolTip(lblView, "Show Checked Accounts Only (enables launch all)");

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

            RefreshProfileList();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Intentionally empty (kept for designer hook)
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
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

            _config.Save();
        }

        // -----------------------------
        // Profile list / View logic
        // -----------------------------

        private void RefreshProfileList()
        {
            int topIndex = lstProfiles.TopIndex;
            object? selectedItem = lstProfiles.SelectedItem;

            lstProfiles.BeginUpdate();
            try
            {
                lstProfiles.Items.Clear();

                var profiles = _profileManager.Profiles
                    .OrderBy(p => p.GameType) // GW1 before GW2
                    .ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
                    .AsEnumerable();

                // Visibility is controlled ONLY by Show Checked Accounts Only (view-scoped)
                if (_showCheckedOnly)
                {
                    profiles = profiles.Where(p => _views.IsEligible(_views.ActiveViewName, p.Id));
                }

                foreach (var profile in profiles)
                    lstProfiles.Items.Add(profile);

                // Restore selection by object identity (works because you re-add the same profile instances)
                if (selectedItem != null)
                {
                    int idx = lstProfiles.Items.IndexOf(selectedItem);
                    if (idx >= 0)
                        lstProfiles.SelectedIndex = idx;
                }

                // Restore scroll position (clamp to list size)
                if (lstProfiles.Items.Count > 0)
                {
                    if (topIndex < 0) topIndex = 0;
                    if (topIndex >= lstProfiles.Items.Count) topIndex = lstProfiles.Items.Count - 1;
                    lstProfiles.TopIndex = topIndex;
                }

                UpdateBulkArmingUi();
            }
            finally
            {
                lstProfiles.EndUpdate();
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

            if (!string.IsNullOrWhiteSpace(eval.StatusText))
                SetStatus(eval.StatusText);
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

        private void ReenableListScrollbarAndFillPanel()
        {
            lstProfiles.Dock = DockStyle.Fill;
            lstProfiles.IntegralHeight = false;

            lstProfiles.Width = panelProfiles.ClientSize.Width;
            lstProfiles.Height = panelProfiles.ClientSize.Height;

            panelProfiles.Resize -= PanelProfiles_Resize_FillList;
            panelProfiles.Resize += PanelProfiles_Resize_FillList;
        }

        private void PanelProfiles_Resize_FillList(object? sender, EventArgs e)
        {
            if (lstProfiles.Dock == DockStyle.None)
            {
                lstProfiles.SetBounds(0, 0, panelProfiles.ClientSize.Width, panelProfiles.ClientSize.Height);
            }
        }

        // -----------------------------
        // Drawing
        // -----------------------------

        private void lstProfiles_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0 || e.Index >= lstProfiles.Items.Count)
                return;

            if (lstProfiles.Items[e.Index] is not GameProfile profile)
                return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool hot = (e.Index == _hotIndex);

            var g = e.Graphics;

            using (var rowBg = new SolidBrush(ThemeService.Palette.WindowBack))
            {
                g.FillRectangle(rowBg, e.Bounds);
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Card bounds
            Rectangle card = e.Bounds;
            card.Inflate(-ThemeService.CardMetrics.OuterPadding, -ThemeService.CardMetrics.OuterPadding);
            card.Width -= ThemeService.CardMetrics.RightGutter;

            Color backColor =
                selected ? ThemeService.CardPalette.SelectedBack :
                hot ? ThemeService.CardPalette.HoverBack :
                      ThemeService.CardPalette.Back;

            Color borderColor =
                selected ? ThemeService.CardPalette.Border :   // subtle border even when selected
                hot ? ThemeService.CardPalette.HoverBorder :
                      ThemeService.CardPalette.Border;

            using (var bgBrush = new SolidBrush(backColor))
            {
                g.FillRectangle(bgBrush, card);

                if (ShowSelectionBorder)
                {
                    using var borderPen = new Pen(borderColor);
                    g.DrawRectangle(borderPen, card);
                }
            }

            // Ensure listbox background matches overall theme
            lstProfiles.BackColor = ThemeService.Palette.WindowBack;
            lstProfiles.ForeColor = ThemeService.Palette.WindowFore;
            lstProfiles.BorderStyle = BorderStyle.None; // optional, but helps reduce contrast noise

            // Accent bar (left) for selected (and optional hover)
            if (selected || hot)
            {
                var accentRect = new Rectangle(
                    card.Left,
                    card.Top,
                    ThemeService.CardMetrics.AccentWidth,
                    card.Height);

                using var accentBrush = new SolidBrush(ThemeService.CardPalette.Accent);
                g.FillRectangle(accentBrush, accentRect);
            }

            // Eligibility checkbox area (bulk launch eligibility; not selection)
            Rectangle cb = new Rectangle(
                card.Left + ThemeService.CardMetrics.CheckboxOffsetX,
                card.Top + ThemeService.CardMetrics.CheckboxOffsetY,
                ThemeService.CardMetrics.CheckboxSize,
                ThemeService.CardMetrics.CheckboxSize);

            bool eligible = _views.IsEligible(_views.ActiveViewName, profile.Id);
            CheckBoxRenderer.DrawCheckBox(
                g,
                cb.Location,
                eligible ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal);

            // Icon area
            Rectangle iconRect = new Rectangle(
                card.Left + ThemeService.CardMetrics.IconOffsetX,
                card.Top + ThemeService.CardMetrics.IconOffsetY,
                ThemeService.CardMetrics.IconSize,
                ThemeService.CardMetrics.IconSize);

            Image? icon = profile.GameType == GameType.GuildWars1 ? _gw1Image : _gw2Image;
            if (icon != null)
                g.DrawImage(icon, iconRect);

            // Text area
            float textLeft = iconRect.Right + ThemeService.CardMetrics.TextOffsetX;
            float textTop = card.Top + ThemeService.CardMetrics.TextOffsetY;

            var nameFont = _nameFont;
            var subFont = _subFont;

            using (var nameBrush = new SolidBrush(ThemeService.CardPalette.NameFore))
            using (var subBrush = new SolidBrush(ThemeService.CardPalette.SubFore))
            {
                g.DrawString(profile.Name, nameFont, nameBrush, textLeft, textTop);

                string gameLabel = profile.GameType == GameType.GuildWars1 ? "Guild Wars 1" : "Guild Wars 2";
                g.DrawString(
                    gameLabel,
                    subFont,
                    subBrush,
                    textLeft,
                    textTop + nameFont.Height + ThemeService.CardMetrics.SubtitleGapY);
            }

            // Local helper: draw badge pills (right aligned)
            void DrawBadges(IReadOnlyList<string> badges)
            {
                if (badges.Count == 0)
                    return;

                int badgeRight = card.Right - ThemeService.CardMetrics.BadgeRightPadding;
                int badgeTop = card.Top + ThemeService.CardMetrics.BadgeTopPadding;

                using var badgeBg = new SolidBrush(ThemeService.CardPalette.BadgeBack);
                using var badgePen = new Pen(ThemeService.CardPalette.BadgeBorder);

                // Draw right-to-left so the right edge stays aligned
                for (int i = badges.Count - 1; i >= 0; i--)
                {
                    string badge = badges[i];

                    var sz = g.MeasureString(badge, ThemeService.Typography.BadgeFont);
                    int w = (int)sz.Width + ThemeService.CardMetrics.BadgeHorizontalPad;
                    int h = (int)sz.Height + ThemeService.CardMetrics.BadgeVerticalPad;

                    var rect = new Rectangle(badgeRight - w, badgeTop, w, h);

                    g.FillRectangle(badgeBg, rect);
                    g.DrawRectangle(badgePen, rect);

                    TextRenderer.DrawText(
                        g,
                        badge,
                        ThemeService.Typography.BadgeFont,
                        rect,
                        ThemeService.CardPalette.BadgeFore,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    badgeRight -= w + ThemeService.CardMetrics.BadgeSpacing;
                }
            }

            // --- Badges (GW1): TB, gMod, Py4GW ---
            if (profile.GameType == GameType.GuildWars1)
            {
                var badges = new List<string>(2);
                if (profile.Gw1ToolboxEnabled) badges.Add("TB");
                if (profile.Gw1GModEnabled) badges.Add("gMod");
                if (profile.Gw1Py4GwEnabled) badges.Add("Py4");

                DrawBadges(badges);
            }

            // --- Badges (GW2): Blish (when RunAfter is enabled and contains a Blish entry) ---
            if (profile.GameType == GameType.GuildWars2)
            {
                var progs = profile.Gw2RunAfterPrograms ?? new List<RunAfterProgram>();

                bool anyEnabled = profile.Gw2RunAfterEnabled && progs.Any(x => x.Enabled);

                if (anyEnabled)
                {
                    bool blish = progs.Any(x =>
                        x.Enabled &&
                        (
                            (x.Name?.IndexOf("Blish", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                            (x.ExePath?.IndexOf("Blish", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                        ));

                    if (blish)
                    {
                        DrawBadges(new List<string> { "Blish" });
                    }
                }
            }
        }

        // -----------------------------
        // Context menu / selection helpers
        // -----------------------------

        private GameProfile? GetSelectedProfile()
        {
            return lstProfiles.SelectedItem as GameProfile;
        }

        private void ctxProfiles_Opening(object sender, CancelEventArgs e)
        {
            var profile = GetSelectedProfile();
            bool hasProfile = profile != null;

            menuLaunchProfile.Enabled = hasProfile;
            menuEditProfile.Enabled = hasProfile;
            deleteToolStripMenuItem.Enabled = hasProfile;
            menuShowLastLaunchDetails.Enabled = _launchSession.HasAnyReports;
        }

        private void menuShowLastLaunchDetails_Click(object sender, EventArgs e)
        {
            if (!_launchSession.HasAnyReports)
                return;

            using var dlg = new LastLaunchDetailsForm(_launchSession.AllReports);
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
            var profile = lstProfiles.SelectedItem as GameProfile;
            if (profile == null)
                return;

            using var dlg = new ProfileSettingsForm(profile);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _profileManager.Save();
                RefreshProfileList();
            }
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

        // -----------------------------
        // Profile list input
        // -----------------------------

        private void lstProfiles_DoubleClick(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile != null)
            {
                LaunchProfile(profile, bulkMode: false);
            }
        }

        private void lstProfiles_MouseDown(object sender, MouseEventArgs e)
        {
            int index = lstProfiles.IndexFromPoint(e.Location);
            if (index < 0) return;

            // Map click to item bounds
            Rectangle itemRect = lstProfiles.GetItemRectangle(index);
            var profile = lstProfiles.Items[index] as GameProfile;
            if (profile == null) return;

            // Checkbox rectangle must match drawing logic
            Rectangle card = itemRect;
            card.Inflate(-ThemeService.CardMetrics.OuterPadding, -ThemeService.CardMetrics.OuterPadding);

            Rectangle cb = new Rectangle(
                card.Left + ThemeService.CardMetrics.CheckboxOffsetX,
                card.Top + ThemeService.CardMetrics.CheckboxOffsetY,
                ThemeService.CardMetrics.CheckboxSize,
                ThemeService.CardMetrics.CheckboxSize);

            if (e.Button == MouseButtons.Left && cb.Contains(e.Location))
            {
                _views.ToggleEligible(_views.ActiveViewName, profile.Id);
                _views.Save();

                if (_showCheckedOnly)
                {
                    // In "show checked only" mode, eligibility changes affect visibility,
                    // so we must rebuild the list.
                    RefreshProfileList();
                }
                else
                {
                    // Otherwise, a lightweight repaint is enough.
                    lstProfiles.Invalidate(lstProfiles.GetItemRectangle(index));
                }

                // Keep bulk launch UI state accurate either way.
                UpdateBulkArmingUi();
            }

            // Existing right-click selection behavior
            if (e.Button == MouseButtons.Right)
            {
                lstProfiles.SelectedIndex = index;
            }
        }

        private void lstProfiles_MouseMove(object? sender, MouseEventArgs e)
        {
            int idx = lstProfiles.IndexFromPoint(e.Location);

            if (idx == _hotIndex)
                return;

            int old = _hotIndex;
            _hotIndex = idx;

            // Old hot index might be invalid if the list was refreshed/filtered.
            if (old >= 0 && old < lstProfiles.Items.Count)
                lstProfiles.Invalidate(lstProfiles.GetItemRectangle(old));

            // New hot index might be -1 (no item) or invalid if list changed mid-event.
            if (_hotIndex >= 0 && _hotIndex < lstProfiles.Items.Count)
                lstProfiles.Invalidate(lstProfiles.GetItemRectangle(_hotIndex));
        }

        private void lstProfiles_MouseLeave(object? sender, EventArgs e)
        {
            if (_hotIndex < 0)
                return;

            int old = _hotIndex;
            _hotIndex = -1;

            if (old >= 0 && old < lstProfiles.Items.Count)
                lstProfiles.Invalidate(lstProfiles.GetItemRectangle(old));
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

            _launchSession.BeginSession(bulkMode: false);

            // Prevent re-entrancy while bulk launch is running.
            btnLaunchAll.Enabled = false;
            try
            {
                // Run GW2 launches off the UI thread to avoid paint starvation (GW2 automation contains sleeps/polling).
                // GW1 launches remain on UI thread (fast, minimal waiting), but can be moved later if needed.
                foreach (var profile in targets)
                {
                    if (profile.GameType == GameType.GuildWars2)
                    {
                        await Task.Run(() => LaunchProfileGw2BulkWorker(profile));
                    }
                    else
                    {
                        LaunchProfile(profile, bulkMode: true);
                    }

                    // Give WinForms a chance to repaint between profiles.
                    await Task.Yield();
                }
            }
            finally
            {
                UpdateBulkArmingUi();
            }
        }

        private void btnSetGw1Path_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Guild Wars 1 executable";
                dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

                // If we already have a path, use its folder as the starting point
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

            // Single launch = new "session". Bulk launch = append attempts to the same session.
            _launchSession.BeginSession(bulkMode);

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
                    ApplyLaunchReportToUi(result.Report);

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

        private void SetStatus(string text)
        {
            SafeUi(() => lblStatus.Text = text ?? "");
        }

        private void SafeUi(Action action)
        {
            _ui.Post(action);
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
        private void LaunchProfileGw2BulkWorker(GameProfile profile)
        {
            if (profile == null)
                return;

            // Use a fresh config snapshot (ProfileSettingsForm writes and saves its own config instance).
            var cfg = LauncherConfig.Load();

            string exePath = profile.ExecutablePath;
            if (string.IsNullOrWhiteSpace(exePath))
                exePath = cfg.Gw2Path;

            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                SafeUi(() =>
                {
                    MessageBox.Show(
                        this,
                        "No valid executable path is configured for this profile.\n\n" +
                        "Edit the profile or configure the game path in settings.",
                        "Missing executable",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                });
                return;
            }

            var report = new LaunchReport
            {
                GameName = "Guild Wars 2",
                ExecutablePath = exePath
            };

            bool mcEnabled = cfg.Gw2MulticlientEnabled;

            var result = _gw2Orchestrator.Launch(
                profile: profile,
                exePath: exePath,
                mcEnabled: mcEnabled,
                bulkMode: true,
                automationCoordinator: _gw2Automation,
                runAfterInvoker: _gw2RunAfterLauncher.Start);

            SafeUi(() =>
            {
                if (result.Report != null)
                    ApplyLaunchReportToUi(result.Report);

                if (result.HasMessageBox)
                {
                    MessageBox.Show(
                        this,
                        result.MessageBoxText,
                        result.MessageBoxTitle,
                        MessageBoxButtons.OK,
                        result.MessageBoxIsError ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
                }
            });
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
