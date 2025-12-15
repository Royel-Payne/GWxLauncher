using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Collections.Generic;
using System.Text.Json;

namespace GWxLauncher
{
    public partial class MainForm : Form
    {

        private LauncherConfig _config;
        private readonly ProfileManager _profileManager = new();

        private readonly Image _gw1Image = Properties.Resources.Gw1;
        private readonly Image _gw2Image = Properties.Resources.Gw2;

        private LaunchReport? _lastLaunchReport;

        private readonly ViewStateStore _views = new();
        private bool _showCheckedOnly = false;
        private string _viewNameBeforeEdit = "";
        private bool _viewNameDirty = false;
        private bool _suppressViewTextEvents = false;
        private bool _suppressArmBulkEvents = false;
        private readonly Font _nameFont;
        private readonly Font _subFont;

        private int _hotIndex = -1;

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
            // Always allow toggling this checkbox. It is a visibility control (UI-only).
            chkArmBulk.Enabled = true;

            bool anyEligible = _views.AnyEligibleInActiveView(_profileManager.Profiles);

            // Bulk armed (future) is a derived state: (any eligible) + (show checked only)
            bool armed = anyEligible && _showCheckedOnly;

            btnLaunchAll.Enabled = armed;

            if (armed)
                lblStatus.Text = $"Bulk launch armed · View: {_views.ActiveViewName}";
            else if (_showCheckedOnly && !anyEligible)
                lblStatus.Text = $"No checked profiles in view · View: {_views.ActiveViewName}";
        }
        private void chkArmBulk_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressArmBulkEvents)
                return;

            _showCheckedOnly = chkArmBulk.Checked;

            _views.SetShowCheckedOnly(_views.ActiveViewName, _showCheckedOnly);
            _views.Save();

            RefreshProfileList();
        }

        private void ReenableListScrollbarAndFillPanel()
        {
            // Stop using the "hide scrollbar by widening listbox" trick while iterating.
            // Keep listbox exactly sized to the panel.
            lstProfiles.Dock = DockStyle.Fill;
            lstProfiles.IntegralHeight = false;

            // Remove any previous "widened listbox" sizing effects
            lstProfiles.Width = panelProfiles.ClientSize.Width;
            lstProfiles.Height = panelProfiles.ClientSize.Height;

            // Keep it correct on resize
            panelProfiles.Resize -= PanelProfiles_Resize_FillList;
            panelProfiles.Resize += PanelProfiles_Resize_FillList;
        }

        private void PanelProfiles_Resize_FillList(object? sender, EventArgs e)
        {
            // Ensure fill stays accurate even if Dock is changed later
            if (lstProfiles.Dock == DockStyle.None)
            {
                lstProfiles.SetBounds(0, 0, panelProfiles.ClientSize.Width, panelProfiles.ClientSize.Height);
            }
        }


        public MainForm()
        {
            InitializeComponent();
            var baseFont = Font; // form font
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

            lstProfiles.MouseMove += lstProfiles_MouseMove;
            lstProfiles.MouseLeave += lstProfiles_MouseLeave;

            chkArmBulk.BringToFront(); // ensure it's not obscured

            ctxProfiles.Opening += ctxProfiles_Opening;

            _config = LauncherConfig.Load();

            ThemeService.ApplyToForm(this);

            ReenableListScrollbarAndFillPanel();

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

            lblView.Visible = true;
            lblView.Text = "Show checked \n Arm launch";
            lblView.AutoSize = true;
            lblView.ForeColor = ThemeService.Palette.SubtleFore;
            var tip = new ToolTip();
            tip.SetToolTip(chkArmBulk, "Show checked profiles only (arms launch all)");
            tip.SetToolTip(lblView, "Show checked profiles only (arms launch all)");

            // (your window-position restore logic)
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

            _profileManager.Load();
            _views.Load();

            _suppressViewTextEvents = true;
            txtView.Text = _views.ActiveViewName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;

            RefreshProfileList();
        }

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
            using (var borderPen = new Pen(borderColor))
            {
                g.FillRectangle(bgBrush, card);
                g.DrawRectangle(borderPen, card);
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

            // --- Badges (GW1): TB, gMod ---
            if (profile.GameType == GameType.GuildWars1)
            {
                var badges = new List<string>(2);
                if (profile.Gw1ToolboxEnabled) badges.Add("TB");
                if (profile.Gw1GModEnabled) badges.Add("gMod");

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

            if (selected)
                e.DrawFocusRectangle();
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

        private void ctxProfiles_Opening(object? sender, CancelEventArgs e)
        {
            // NEW: only enabled if we have a report this session
            menuShowLastLaunchDetails.Enabled = _lastLaunchReport != null;
        }


        private void btnLaunchGw1_Click(object sender, EventArgs e)
        {
            LaunchGame(_config.Gw1Path, "Guild Wars 1");
        }
        private void btnLaunchGw2_Click(object sender, EventArgs e)
        {
            LaunchGame(_config.Gw2Path, "Guild Wars 2");
        }
        private GameProfile? GetSelectedProfile()
        {
            return lstProfiles.SelectedItem as GameProfile;
        }
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
                    lblStatus.Text = $"Updated path for {profile.Name}.";

                    return true;
                }

                lblStatus.Text = $"No path selected for {profile.Name}.";
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
                    lblStatus.Text = $"Set GW1 Toolbox DLL for {profile.Name}.";
                    return true;
                }

                lblStatus.Text = $"No Toolbox DLL selected for {profile.Name}.";
                return false;
            }
        }

        private void LaunchProfile(GameProfile profile)
        {
            if (profile == null)
                return;

            // Resolve executable path: per-profile override first, then global config
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
            var gameName = profile.GameType == GameType.GuildWars1
                ? "Guild Wars 1"
                : "Guild Wars 2";

            // If this is a GW1 profile, delegate launch + injection to Gw1InjectionService
            if (profile.GameType == GameType.GuildWars1)
            {
                var gw1Service = new Gw1InjectionService();

                if (gw1Service.TryLaunchGw1(profile, exePath, this, out var gw1Error, out var report))
                {
                    _lastLaunchReport = report;
                    lblStatus.Text = report.BuildSummary();
                }
                else
                {
                    _lastLaunchReport = report;

                    MessageBox.Show(
                        this,
                        gw1Error,
                        "Guild Wars 1 launch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    lblStatus.Text = report.BuildSummary();
                }

                return;
            }


            // Default: no injection (GW2, or GW1 with no DLLs enabled)
            LaunchGame(exePath, gameName);
            // GW2: launch + optional helper programs
            try
            {
                Process.Start(exePath);

                StartGw2RunAfterPrograms(profile);
                lblStatus.Text = "Launched Guild Wars 2.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Failed to launch {gameName}:\n\n{ex.Message}",
                    "Launch failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

        }
        private void StartGw2RunAfterPrograms(GameProfile profile)
        {
            if (profile.GameType != GameType.GuildWars2)
                return;

            if (!profile.Gw2RunAfterEnabled)
                return;

            if (profile.Gw2RunAfterPrograms == null || profile.Gw2RunAfterPrograms.Count == 0)
                return;

            foreach (var p in profile.Gw2RunAfterPrograms)
            {
                if (!p.Enabled)
                    continue;

                if (string.IsNullOrWhiteSpace(p.ExePath) || !File.Exists(p.ExePath))
                    continue;

                try
                {
                    Process.Start(p.ExePath);
                }
                catch
                {
                    // swallow for now; later we can surface this in LaunchReport-like UI
                }
            }
        }

        private void lstProfiles_DoubleClick(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile != null)
            {
                LaunchProfile(profile);
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
                //RefreshProfileList();
                // after toggling eligibility...
                lstProfiles.Invalidate(lstProfiles.GetItemRectangle(index));
            }

            // Existing right-click selection behavior
            if (e.Button == MouseButtons.Right)
            {
                lstProfiles.SelectedIndex = index;
            }
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

        private void btnViewPrev_Click(object sender, EventArgs e)
        {
            StepView(-1);
        }

        private void btnViewNext_Click(object sender, EventArgs e)
        {
            StepView(+1);
        }
        private void ApplyViewScopedUiState()
        {
            _showCheckedOnly = _views.GetShowCheckedOnly(_views.ActiveViewName);

            _suppressArmBulkEvents = true;
            chkArmBulk.Checked = _showCheckedOnly;
            _suppressArmBulkEvents = false;
        }


        private void txtView_TextChanged(object sender, EventArgs e)
        {
            if (_suppressViewTextEvents)
                return;

            // Don’t mutate view store during typing.
            _viewNameDirty = true;
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

                lblStatus.Text = "View rename failed (name already exists).";
                return;
            }

            _views.Save();
            RefreshProfileList();
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

        private void menuShowLastLaunchDetails_Click(object sender, EventArgs e)
        {
            if (_lastLaunchReport == null)
                return;

            using var dlg = new LastLaunchDetailsForm(_lastLaunchReport);
            dlg.ShowDialog(this);
        }

        private void menuLaunchProfile_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile != null)
            {
                LaunchProfile(profile);
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
                lblStatus.Text = $"Deleted account: {profile.Name}.";
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

            //profile.Gw1ToolboxEnabled = menuGw1ToolboxToggle.Checked;
            _profileManager.Save();

            lblStatus.Text = $"GW1 Toolbox injection " +
                             (profile.Gw1ToolboxEnabled ? "enabled" : "disabled") +
                             $" for {profile.Name}.";
        }

        private void menuGw1ToolboxPath_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null || profile.GameType != GameType.GuildWars1)
                return;

            TrySelectGw1ToolboxDll(profile);
        }

        private void LaunchGame(string exePath, string gameName)
        {
            try
            {
                // 🔹 New: handle missing/blank path from config
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

                Process.Start(startInfo);

                if (lblStatus != null)
                    lblStatus.Text = $"{gameName} launched.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to launch {gameName}.\n\n{ex.Message}",
                    "Launch Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                if (lblStatus != null)
                    lblStatus.Text = $"Error launching {gameName}.";
            }
        }
        private void btnLaunchAll_Click(object sender, EventArgs e)
        {
            // Guard: bulk launch must be explicitly armed.
            bool anyEligible = _views.AnyEligibleInActiveView(_profileManager.Profiles);
            bool armed = anyEligible && _showCheckedOnly;

            if (!armed)
            {
                lblStatus.Text = $"Bulk launch not armed · View: {_views.ActiveViewName}";
                return;
            }

            var targets = _profileManager.Profiles
                .Where(p => _views.IsEligible(_views.ActiveViewName, p.Id))
                .OrderBy(p => p.GameType)
                .ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            if (targets.Count == 0)
            {
                lblStatus.Text = $"No checked profiles in view · View: {_views.ActiveViewName}";
                return;
            }

            foreach (var profile in targets)
                LaunchProfile(profile);
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
                    lblStatus.Text = "GW1 path updated.";
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
                    lblStatus.Text = "GW2 path updated.";
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
                    _profileManager.Save();     // 🔹 persist to profiles.json

                    RefreshProfileList();
                    UpdateBulkArmingUi();
                    lblStatus.Text = $"Added account: {dialog.CreatedProfile.Name}";
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
        private void lstProfiles_MouseMove(object? sender, MouseEventArgs e)
        {
            int idx = lstProfiles.IndexFromPoint(e.Location);

            if (idx == _hotIndex)
                return;

            int old = _hotIndex;
            _hotIndex = idx;

            if (old >= 0)
                lstProfiles.Invalidate(lstProfiles.GetItemRectangle(old));

            if (_hotIndex >= 0)
                lstProfiles.Invalidate(lstProfiles.GetItemRectangle(_hotIndex));
        }

        private void lstProfiles_MouseLeave(object? sender, EventArgs e)
        {
            if (_hotIndex < 0)
                return;

            int old = _hotIndex;
            _hotIndex = -1;
            lstProfiles.Invalidate(lstProfiles.GetItemRectangle(old));
        }

    }
}

